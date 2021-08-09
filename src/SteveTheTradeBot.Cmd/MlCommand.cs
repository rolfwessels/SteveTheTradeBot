using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.ML.Model;
using SteveTheTradeBotML.Model;

namespace SteveTheTradeBot.Cmd
{
    public class MlCommand
    {
        public class BuildTrainingData : AsyncCommandWithToken<BuildTrainingData.Settings>
        {
            public class Settings : BaseCommandSettings
            {
                [CommandOption("--output")]
                [Description(@"Csv with training data [grey][[C:\temp\btc - data.txt]][/]")]
                public string TrainOnCsv { get; set; } = @"C:\temp\btc-data.txt";
            }

            #region Overrides of AsyncCommandWithToken<Settings>

            public override async Task ExecuteAsync(Settings settings, CancellationToken token)
            {
                await AnsiConsole.Status()
                    .StartAsync("Starting...", async ctx =>
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        await using var writer = new StreamWriter(settings.TrainOnCsv);
                        var strategyStore = IocApi.Instance.Resolve<ITradeQuoteStore>();
                        var fromDate = DateTime.UtcNow.AddYears(-2);
                        var toDate = DateTime.UtcNow.AddMonths(-4);
                        ctx.Status($"Reading data  {fromDate} and {toDate}");

                        var records = strategyStore.FindAllBetween(fromDate, toDate, "valr",
                                CurrencyPair.BTCZAR, PeriodSize.FiveMinutes)
                            .ToList()
                            .Where(x => x.Metric.ContainsKey("ema200"))
                            .ToList();
                        AnsiConsole.MarkupLine(
                            $"[grey]Found[/] [white]{records.Count}[/] records between {fromDate} and {toDate}.");
                        ctx.Status($"Writing to {settings.TrainOnCsv} csv file.");


                        var future = (60 / 5) * 6; // 12 hours
                        var output = records
                            .Take(records.Count - future)
                            .Select((x, i) => new
                            {
                                x,
                                metric = x.Metric.ToDictionary(x => x.Key,
                                    x => x.Value.HasValue ? Math.Round(x.Value.Value, 5) : x.Value),
                                future = records[i + future]
                            })
                            .Select(x => new
                            {
                                x.x.Date,
                                x.x.Close,
                                x.x.Volume,
                                macd = x.metric["macd"],
                                rsi14 = x.metric["rsi14"],
                                ema100 = x.metric["ema100"],
                                ema200 = x.metric["ema200"],
                                roc100 = x.metric["roc100"],
                                roc200 = x.metric["roc200"],
                                roc100sma = x.metric["roc100-sma"],
                                roc200sma = x.metric["roc200-sma"],
                                supertrend = x.metric["supertrend"],
                                macdsignal = x.metric["macd-signal"],
                                macdhistogram = x.metric["macd-histogram"],
                                supertrendlower = x.metric["supertrend-lower"],
                                supertrendupper = x.metric["supertrend-upper"],
                                FutureDate = x.future.Date,
                                FutureClose = x.future.Close,
                                FutureChange = TradeUtils.MovementPercent(x.future.Close, x.x.Close),
                            });


                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            await csv.WriteRecordsAsync(output, token);
                        }

                        AnsiConsole.MarkupLine($"[grey]Done in [/] [white]{stopwatch.Elapsed.ToShort()}[/] 👍.");
                    });
            }

            #endregion
        }

        public class PlotModel : AsyncCommandWithToken<PlotModel.Settings>
        {
            public class Settings : BaseCommandSettings
            {
                [CommandOption("--model")]
                [Description(@"ModelOutput [grey][[C:\temp\MLModel.zip]][/]")]
                public string Model { get; set; } = @"C:\temp\MLModel.zip";

            }

            public override async Task ExecuteAsync(Settings settings, CancellationToken token)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                await AnsiConsole.Status()
                    .StartAsync("Starting...", async ctx =>
                    {
                        var strategyStore = IocApi.Instance.Resolve<ITradeQuoteStore>();
                        var dynoDynamicGraphs = IocApi.Instance.Resolve<IDynamicGraphs>();
                        var tradeFeedQuotes = strategyStore.FindAllBetween(DateTime.Now.AddMonths(-12), DateTime.Now,
                            "valr",
                            CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
                        var feedName = "ml-magic";
                        ctx.Status("Clearing old records");
                        await dynoDynamicGraphs.Clear(feedName);
                        var consumeModel = new ConsumeModel(settings.Model);
                        var counter = 0;
                        foreach (var tradeFeedCandle in tradeFeedQuotes)
                        {
                            var modelInput = tradeFeedCandle.ToModelInput();
                            var predictionResult = consumeModel.Predict(modelInput);
                            await dynoDynamicGraphs.Plot(feedName, tradeFeedCandle.Date, "ml",
                                (decimal) predictionResult.Score);
                            await dynoDynamicGraphs.Plot(feedName, tradeFeedCandle.Date.AddHours(6), "value",
                                tradeFeedCandle.Close * (100m + (decimal) predictionResult.Score) / 100);

                            counter++;
                            
                            if (counter % 10000 == 0) ctx.Status($"Processing at {counter}");
                        }
                        ctx.Status($"Saving at {counter} records");
                        await dynoDynamicGraphs.Flush();
                        AnsiConsole.MarkupLine($"[grey]Done with {counter} records in [/] [white]{stopwatch.Elapsed.ToShort()}[/] 👍.");
                    });
            }
        }


        public class ModelBuilder : AsyncCommandWithToken<ModelBuilder.Settings>
        {
            public class Settings : BaseCommandSettings
            {
                [CommandOption("--train")]
                [Description(@"Csv with training data [grey][[C:\temp\btc - data.txt]][/]")]
                public string TrainOnCsv { get; set; } = @"C:\temp\btc-data.txt";

                [CommandOption("--output")]
                [Description(@"ModelOutput [grey][[C:\temp\MLModel.zip]][/]")]
                public string Output { get; set; } = @"C:\temp\MLModel.zip";


                [CommandOption("--trees")]
                [Description(@"Set the amount of trees  [grey][[5000]][/]")]
                public int Trees { get; set; } = 5000;
            }

            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            private readonly MLContext _mlContext = new MLContext(seed: 1);

            #region Overrides of AsyncCommandWithToken<BaseCommandSettings>

            public override async Task ExecuteAsync(ModelBuilder.Settings settings, CancellationToken token)
            {
                await AnsiConsole.Status()
                    .StartAsync("Starting...", ctx =>
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();


                        ctx.Status($"[grey]Load data from [/][white]{settings.TrainOnCsv}[/]");
                        IDataView trainingDataView = _mlContext.Data.LoadFromTextFile<ModelInput>(
                            path: settings.TrainOnCsv,
                            hasHeader: true,
                            separatorChar: ',',
                            allowQuoting: true,
                            allowSparse: false);
                        AnsiConsole.MarkupLine($"[grey]Done reading [/][white]{settings.TrainOnCsv}[/]");
                        ctx.Status($"[grey]Build pipeline[/]");
                        IEstimator<ITransformer> trainingPipeline = BuildTrainingPipeline(_mlContext, settings.Trees);

                        ctx.Status("Train model");
                        ITransformer mlModel = TrainModel(_mlContext, trainingDataView, trainingPipeline);

                        ctx.Status("Evaluate quality of Model");
                        Evaluate(_mlContext, trainingDataView, trainingPipeline);

                        // Save model
                        ctx.Status("[grey]Save model to [/][white]{settings.Output}[/]");
                        SaveModel(_mlContext, mlModel, settings.Output, trainingDataView.Schema);
                        AnsiConsole.MarkupLine(
                            $"[grey]Done writing model to [/][white]{GetAbsolutePath(settings.Output)}[/] in {stopwatch.Elapsed.ToShort()}");
                        return Task.CompletedTask;
                    });
            }

            #endregion

            public static IEstimator<ITransformer> BuildTrainingPipeline(MLContext mlContext, int numberOfTrees)
            {
                // Data process configuration with pipeline data transformations 
                var dataProcessPipeline = mlContext.Transforms.Concatenate("Features",
                    new[]
                    {
                        "Close", "Volume", "macd", "rsi14", "ema100", "ema200", "roc100", "roc200", "roc100sma",
                        "roc200sma", "supertrend", "macdsignal", "macdhistogram", "supertrendlower", "supertrendupper"
                    });
                // Set the training algorithm 
                var trainer = mlContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options()
                {
                    NumberOfLeaves = 60,
                    MinimumExampleCountPerLeaf = 10,
                    NumberOfTrees = numberOfTrees,
                    LearningRate = 0.1300971f,
                    Shrinkage = 1.791386f,
                    LabelColumnName = "FutureChange",
                    FeatureColumnName = "Features"
                });

                var trainingPipeline = dataProcessPipeline.Append(trainer);

                return trainingPipeline;
            }

            public static ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView,
                IEstimator<ITransformer> trainingPipeline)
            {
                ITransformer model = trainingPipeline.Fit(trainingDataView);
                return model;
            }

            private static void Evaluate(MLContext mlContext, IDataView trainingDataView,
                IEstimator<ITransformer> trainingPipeline)
            {
                // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
                // in order to evaluate and get the model's accuracy metrics
                var crossValidationResults = mlContext.Regression.CrossValidate(trainingDataView, trainingPipeline,
                    numberOfFolds: 5, labelColumnName: "FutureChange");
                PrintRegressionFoldsAverageMetrics(crossValidationResults);
                
            }

            private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath,
                DataViewSchema modelInputSchema)
            {
                // Save/persist the trained model to a .ZIP file
                mlContext.Model.Save(mlModel, modelInputSchema, GetAbsolutePath(modelRelativePath));
            }

            public static string GetAbsolutePath(string relativePath)
            {
                string assemblyFolderPath = new FileInfo(typeof(Program).Assembly.Location).Directory.FullName;
                string fullPath = Path.Combine(assemblyFolderPath, relativePath);
                return fullPath;
            }

            public static void PrintRegressionMetrics(RegressionMetrics metrics)
            {
                AnsiConsole.MarkupLine($"*************************************************");
                AnsiConsole.MarkupLine($"*       Metrics for Regression model      ");
                AnsiConsole.MarkupLine($"*------------------------------------------------");
                AnsiConsole.MarkupLine($"*       LossFn:        {metrics.LossFunction:0.##}");
                AnsiConsole.MarkupLine($"*       R2 Score:      {metrics.RSquared:0.##}");
                AnsiConsole.MarkupLine($"*       Absolute loss: {metrics.MeanAbsoluteError:#.##}");
                AnsiConsole.MarkupLine($"*       Squared loss:  {metrics.MeanSquaredError:#.##}");
                AnsiConsole.MarkupLine($"*       RMS loss:      {metrics.RootMeanSquaredError:#.##}");
                AnsiConsole.MarkupLine($"*************************************************");
            }

            public static void PrintRegressionFoldsAverageMetrics(
                IEnumerable<TrainCatalogBase.CrossValidationResult<RegressionMetrics>> crossValidationResults)
            {
                var L1 = crossValidationResults.Select(r => r.Metrics.MeanAbsoluteError);
                var L2 = crossValidationResults.Select(r => r.Metrics.MeanSquaredError);
                var RMS = crossValidationResults.Select(r => r.Metrics.RootMeanSquaredError);
                var lossFunction = crossValidationResults.Select(r => r.Metrics.LossFunction);
                var R2 = crossValidationResults.Select(r => r.Metrics.RSquared);

                var table = new Table {Title = new TableTitle("Metrics for Regression model")};
                table.AddColumn("Metric");
                table.AddColumn("Value");
                table.AddRow("Average L1 Loss", $"[green]{L1.Average():0.###}[/]");
                table.AddRow("Average L2 Loss", $"[green]{L2.Average():0.###}[/]");
                table.AddRow("Average RMS", $"[green]{RMS.Average():0.###}[/]");
                table.AddRow("Average Loss Function", $"[green]{lossFunction.Average():0.###}[/]");
                table.AddRow("Average R-squared", $"[green]{R2.Average():0.###}[/]");
                AnsiConsole.Render(table);
            }
        }
    }
}