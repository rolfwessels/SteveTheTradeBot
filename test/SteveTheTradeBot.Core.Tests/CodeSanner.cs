using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteveTheTradeBot.Core.Tests
{
    public class CodeSanner
    {
        private readonly Lazy<string[]> _lazy;
        private readonly ICodeSanner[] _runners;

        public CodeSanner()
        {
            _lazy = new Lazy<string[]>(() => Directory.GetFiles(GetSourcePath(), "*.cs", SearchOption.AllDirectories)
                .Where(x => !x.Contains(@"\obj\")).ToArray());
            _runners = new ICodeSanner[]
            {
                new TestsShouldEndWithFileNameTests(),
                new ClassesWithoutTests()
            };
        }

        public string GetSourcePath()
        {
            // todo: Rolf make this dynamic.
            return @"D:\Work\Synced\SteveTheTradeBot\src";
        }

        public List<FileReport> ScanNow()
        {
            var fileReports = new List<FileReport>();
            foreach (var fileName in _lazy.Value)
                if (_runners.Any(x => x.ShouldScan(fileName)))
                {
                    var fileReport = new FileReport
                    {
                        FileName = fileName,
                        ShortName = fileName.Replace(GetSourcePath(), "")
                    };
                    var readAllLines = File.ReadAllLines(fileName);
                    foreach (var runner in _runners.Where(x => x.ShouldScan(fileName)))
                    {
                        var isFail = runner.IsFail(fileName, readAllLines, _lazy.Value);
                        fileReport.Issues.AddRange(isFail);
                    }

                    if (fileReport.Issues.Any())
                    {
                        fileReport.LinesOfCode = readAllLines.Length;
                        fileReports.Add(fileReport);
                    }
                }

            return fileReports;
        }

        #region Nested type: ClassesWithoutTests

        public class ClassesWithoutTests : ICodeSanner
        {
            #region Implementation of ICodeSanner

            public bool ShouldScan(string fileName)
            {
                return !fileName.Contains(".Tests") && !fileName.Contains("AssemblyInfo") &&
                       !fileName.Contains("Model") && !fileName.Contains("Constants") &&
                       !Path.GetFileName(fileName).StartsWith("I");
            }

            public IEnumerable<Issue> IsFail(string fileName, string[] fileLines, string[] allFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var testclassName = fileNameWithoutExtension + "Tests.cs";
                if (!allFiles.Any(x => x.Contains(testclassName)))
                    yield return
                        new Issue
                        {
                            Type = GetType().Name,
                            Description =
                                $"Expect the file {fileNameWithoutExtension} to have a test class somewhere {testclassName}."
                        };
            }

            #endregion
        }

        #endregion

        #region Nested type: FileReport

        public class FileReport
        {
            public FileReport()
            {
                Issues = new List<Issue>();
            }

            public string FileName { get; set; }
            public List<Issue> Issues { get; set; }
            public int LinesOfCode { get; set; }
            public string ShortName { get; set; }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"FileName: {ShortName} [ lines : {LinesOfCode} , Issues {Issues.Count}]");
                stringBuilder.AppendLine("----------------");
                foreach (var issue in Issues) stringBuilder.AppendLine(issue.ToString());
                stringBuilder.AppendLine("");
                return stringBuilder.ToString();
            }
        }

        #endregion

        #region Nested type: ICodeSanner

        public interface ICodeSanner
        {
            bool ShouldScan(string fileName);
            IEnumerable<Issue> IsFail(string fileName, string[] fileLines, string[] allFiles);
        }

        #endregion

        #region Nested type: Issue

        public class Issue
        {
            public string Type { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Issue: {Description} ");
                stringBuilder.AppendLine("");
                return stringBuilder.ToString();
            }
        }

        #endregion

        #region Nested type: TestsShouldEndWithFileNameTests

        public class TestsShouldEndWithFileNameTests : ICodeSanner
        {
            #region Implementation of ICodeSanner

            public bool ShouldScan(string fileName)
            {
                return fileName.Contains(".Tests");
            }

            public IEnumerable<Issue> IsFail(string fileName, string[] fileLines, string[] allFiles)
            {
                var name = Path.GetFileName(fileName) ?? "";
                var nameNoExtention = Path.GetFileNameWithoutExtension(fileName) ?? "";
                var endsWithTests = name.EndsWith("Test.cs");
                if (endsWithTests)
                    yield return
                        new Issue
                        {
                            Type = GetType().Name,
                            Description = $"File should be called {name.Replace("Test.cs", "Tests.cs")}."
                        };
                var endsWithTest = name.EndsWith("Tests.cs");
                if (endsWithTest && !fileLines.Any(x => x.Contains(nameNoExtention)))
                    yield return
                        new Issue
                        {
                            Type = GetType().Name,
                            Description = $"File {name} should contain class {nameNoExtention}."
                        };
            }

            #endregion
        }

        #endregion
    }
}