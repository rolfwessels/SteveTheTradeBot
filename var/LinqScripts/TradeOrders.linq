<Query Kind="Program">
  <Connection>
    <ID>18641041-a990-4c4d-b2dd-334372ce0522</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost</Server>
    <UserName>postgres</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAANRiZPWt3i0eTcfnz8b/cSwAAAAACAAAAAAAQZgAAAAEAACAAAAAzRGQtPIz0fWP4ZcuVKDqPmSKOVK4QwzGg4csqUyJFMgAAAAAOgAAAAAIAACAAAACf/UG3QqStqkJucj0NLSm/BulV+Waws09KXTCx/pX9NhAAAAD2ncCaYOP3XH9KCTdrFzzNQAAAAGLxdy0rvXH+mDzR2RsWlD3vj9WNWEK8Rz9ewQ9AZoJU4wu2j2yxuCyiHNGcF8KxsUxMruMzLKoQfCTQaaD8yCI=</Password>
    <Database>SteveTheTradeBotSample</Database>
    <DisplayName>SteveTheTradeBotSample</DisplayName>
    <DriverData>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
      <UseNativeScaffolder>True</UseNativeScaffolder>
      <Port>15432</Port>
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
	//CheckRun();
	var strategies = Strategies.Where(x => !x.IsBackTest)
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
		RunningFor = (DateTime.Now.ToLocalTime() - x.CreateDate.ToLocalTime()).Days + " days",
		LastRunAgo = (DateTime.Now.ToLocalTime() - x.LastDate.ToLocalTime()).Humanize(),
		BoughtInAt = x.Trades.Where(x => x.IsActive).Select(x => x.BuyPrice).FirstOrDefault(),
		x.Status,
		x.Id
	})
	.OrderByDescending(x => x.Value)
	.ToList()
	//.Dump("strategies")
	;
	new { 
	Investment = strategies.Sum(x=>x.InvestmentAmount),
	Values = strategies.Sum(x=>x.Value),
	Profit = strategies.Average(x=>x.PercentProfit)
	}.Dump();
	var summary = strategies.Select(x => new {x.Reference,x.PercentMarketProfit,x.PercentProfit,x.RunningFor, x.TotalActiveTrades , x.TotalNumberOfTrades , x.LargestLoss,x.LargestProfit, x.AverageTradesPerMonth,x.PercentOfProfitableTrades});
	summary.OrderByDescending(x=>x.PercentProfit).Take(5).Dump("Best performers");
	summary.OrderBy(x=>x.PercentProfit).Take(5).Dump("Worst performers");
	strategies.Dump("strategies");
	var trades = Trades.Where(x=>strategies.Select(r=>r.Id).Contains(x.StrategyInstanceId)).ToList().Dump("Trades");
	TradeOrders.Where(x=>trades.Select(r=>r.Id).Contains(x.StrategyTradeId)).Dump("TradeOrders");
}

public void RemoveStrategy(string id)
{
	var strategies = Strategies.Where(x => x.Id == id).ToList();
	//strategies = Strategies.Where(x => x.IsBackTest).ToList();
	var trades = Trades.Where(x => strategies.Select(r => r.Id).Contains(x.StrategyInstanceId)).ToList();
	var tradeOrders = TradeOrders.Where(x => trades.Select(r => r.Id).Contains(x.StrategyTradeId)).ToList();
	$"Found {strategies.Count} stategy with {trades.Count} trades and {tradeOrders.Count} trade orders".Dump("Confirm by typing yes!");
	var result = Util.ReadLine();
	if (result.ToLower() == "yes!") {
		TradeOrders.RemoveRange(tradeOrders);
		Trades.RemoveRange(trades);
		Strategies.RemoveRange(strategies);
		var updated = SaveChanges();
		updated.Dump("Removed");
	}
}
// You can define other methods, fields, classes and namespaces here
public void CheckRun() {

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
public T CastIt<T>(object value) {
	var x = JsonSerializer.Serialize(value);
	return JsonSerializer.Deserialize<T>(x);
}