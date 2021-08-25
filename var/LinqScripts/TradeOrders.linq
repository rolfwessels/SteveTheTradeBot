<Query Kind="Program">
  <Connection>
    <ID>a366774d-8d24-465f-a121-d2144bef0089</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>192.168.1.250</Server>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAKvXpSbg/+E2KO5TjxpufZgAAAAACAAAAAAAQZgAAAAEAACAAAABfJWr5G1GsTK2MSO61f/U5lj5E3VC0+FKcn4G3WsQJdwAAAAAOgAAAAAIAACAAAADAuSEP2ohkFxjpIIDrhUJxPkX+1FkYDoeujkjMPXns0SAAAADL2MhnaJjj+jQ9pu6wGxZbxsEA9tzItxtMjImUNA25DEAAAABJ/TnKZ/k8iH9E8YQhK8U8Ai8s6szMuM1vpnoW4mbLIE6jCIR/YJeLabgQxgzp/9WxSZEOaeEMqRKjUjm1cYvm</Password>
    <UserName>postgres</UserName>
    <Database>steve_the_trade_bot_staging</Database>
    <IsProduction>true</IsProduction>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
      <UseNativeScaffolder>True</UseNativeScaffolder>
    </DriverData>
  </Connection>
  <Reference Relative="..\..\src\SteveTheTradeBot.Cmd\bin\Debug\netcoreapp3.1\SteveTheTradeBot.Core.dll">D:\Work\Home\SteveTheTradeBot\src\SteveTheTradeBot.Cmd\bin\Debug\netcoreapp3.1\SteveTheTradeBot.Core.dll</Reference>
  <Reference Relative="..\..\src\SteveTheTradeBot.Cmd\bin\Debug\netcoreapp3.1\SteveTheTradeBot.Dal.dll">D:\Work\Home\SteveTheTradeBot\src\SteveTheTradeBot.Cmd\bin\Debug\netcoreapp3.1\SteveTheTradeBot.Dal.dll</Reference>
  <NuGetReference>Humanizer</NuGetReference>
  <Namespace>Humanizer</Namespace>
  <Namespace>SteveTheTradeBot.Core.Components.Broker</Namespace>
  <Namespace>SteveTheTradeBot.Core.Utils</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void Main()
{
	//RemoveStrategy("8220e8f153be4abdaf66c35957b5e257");
	Kuling();
	CheckRun();
	var strategies = Strategies.Where(x => !x.IsBackTest && x.IsActive)
	.ToList()
	.Select(x => new
	{
		x.Reference,
		Value = Math.Round(x.QuoteAmount + (x.BaseAmount * x.LastClose), 2),
		x.InvestmentAmount,
		x.BaseAmount,
		x.BaseAmountCurrency,
		x.QuoteAmount,
		x.QuoteAmountCurrency,
		x.PercentMarketProfit,
		x.PercentProfit,
		x.TotalActiveTrades,
		x.TotalNumberOfTrades,
		x.LastClose,
		x.LargestLoss,
		x.LargestProfit,
		x.LastDate,
		x.PercentOfProfitableTrades,
		x.AverageTradesPerMonth,
		UpdateStopLossAt = x.StrategyProperties.Where(x => x.Key == "UpdateStopLossAt" ).Select(x => Decimal.Parse(x.Value)).FirstOrDefault(),
		StopLoss = x.StrategyProperties.Where(x => x.Key == "StopLoss" ).Select(x => Decimal.Parse(x.Value)).FirstOrDefault(),
		RunningFor = Math.Round((DateTime.Now.ToLocalTime() - x.CreateDate.ToLocalTime()).TotalDays) + " days",
		LastRunAgo = (DateTime.Now.ToLocalTime() - x.LastDate.ToLocalTime()).Humanize(),
		BoughtInAt = x.Trades.Where(x => x.IsActive).Select(x => x.BuyPrice).FirstOrDefault(),
		CurrentTradeProfit = x.TotalActiveTrades == 1 ? MovementPercent(x.LastClose, x.Trades.Where(x => x.IsActive).Select(x => x.BuyPrice).FirstOrDefault()) : 0,
		x.Status,
		x.Id
	})
	.OrderByDescending(x => x.Value)
	.ToList()
	//.Dump("strategies")
	;
	new
	{
		Investment = strategies.Sum(x => x.InvestmentAmount),
		Values = strategies.Sum(x => x.Value),
		Profit = strategies.Average(x => x.PercentProfit)
	}.Dump();
	var summary = strategies.Select(x => new
	{
		x.Reference,
		x.PercentMarketProfit,
		x.PercentProfit,
		x.RunningFor,
		x.TotalActiveTrades,
		x.TotalNumberOfTrades,
		x.LargestLoss,
		x.LargestProfit,
		x.AverageTradesPerMonth,
		x.PercentOfProfitableTrades,
		x.CurrentTradeProfit,
		GarenteeTradeProfit = x.BoughtInAt > 0? MovementPercent(x.StopLoss, x.BoughtInAt):0,
		PercentTillStopLossUpdate = MovementPercent(x.UpdateStopLossAt, x.LastClose),
		PercentTillStopLoss = MovementPercent(x.StopLoss, x.LastClose)
	});
	summary.OrderByDescending(x => x.PercentProfit).Take(5).Dump("Best performers");
	summary.OrderBy(x => x.PercentProfit).Take(5).Dump("Worst performers");
	strategies.Dump("strategies");
	var trades = Trades.Where(x => strategies.Select(r => r.Id).Contains(x.StrategyInstanceId)).ToList().Dump("Trades");
	TradeOrders.Where(x => trades.Select(r => r.Id).Contains(x.StrategyTradeId)).Dump("TradeOrders");
}

void Kuling()
{
	var strats =  Strategies.Where(x=>x.IsActive && x.UpdateDate - x.CreateDate > TimeSpan.FromDays(20))
			.Where(x=>x.Trades.Count == 0 ||  (x.PercentProfit < -10))
			.ToList();
	if (strats.Any()) {
		strats.Dump("De-Activate?");
		DeActivateStrategies(strats.Select(x=>x.Id).ToArray());
	}
	
}


public void DeActivateStrategies(string[] id)
{
	var strategies = Strategies.Where(x => id.Contains(x.Id)).ToList();
	
	$"Found {strategies.Count} stategies would you like to deactivate them".Dump("Confirm by typing yes!");
	var result = Util.ReadLine();
	if (result.ToLower() == "yes!")
	{
		foreach (var st in strategies)
		{
			st.IsActive = false;
		}
		strategies.ForEach(e=>e.IsActive = false);
		var updated = SaveChanges();
		updated.Dump($"deactivated {updated}");
	}
}

public void RemoveStrategies(string[] id)
{
	var strategies = Strategies.Where(x => id.Contains(x.Id)).ToList();
	//strategies = Strategies.Where(x => x.IsBackTest).ToList();
	var trades = Trades.Where(x => strategies.Select(r => r.Id).Contains(x.StrategyInstanceId)).ToList();
	var tradeOrders = TradeOrders.Where(x => trades.Select(r => r.Id).Contains(x.StrategyTradeId)).ToList();
	$"Found {strategies.Count} stategy with {trades.Count} trades and {tradeOrders.Count} trade orders".Dump("Confirm by typing yes!");
	var result = Util.ReadLine();
	if (result.ToLower() == "yes!")
	{
		TradeOrders.RemoveRange(tradeOrders);
		Trades.RemoveRange(trades);
		Strategies.RemoveRange(strategies);
		var updated = SaveChanges();
		updated.Dump("Removed");
	}
}

// You can define other methods, fields, classes and namespaces here
public void CheckRun()
{

	var candles = TradeQuotes.Where(x => x.Feed == "valr" && x.CurrencyPair == "BTCZAR" && x.PeriodSize == 11 && x.Date > DateTime.Now.AddHours(-2))
						.OrderByDescending(x => x.Date);
	new
	{
		LastTrade = HistoricalTrades.Where(x => x.CurrencyPair == "BTCZAR")
			.OrderByDescending(x => x.TradedAt)
			.Take(1).First().TradedAt,
		LastCandle = candles.Take(1).First().Date,
		LastCandleWithMetric = candles
						.Take(100)
						.ToList()
						.Where(x => x.Metric.Count() > 3)
						.Take(1).First().Date,
		Metric = DateTime.Parse(SimpleParams.Where(x => x.Key == "metric_populate_valr_ETHZAR_OneMinute").Select(x => x.Value).First()).ToUniversalTime()
	}.Dump("");


}

public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
{
	if (fromValue == 0) fromValue = 0.00001m;
	return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
}

public T CastIt<T>(object value)
{
	var x = JsonSerializer.Serialize(value);
	return JsonSerializer.Deserialize<T>(x);
}