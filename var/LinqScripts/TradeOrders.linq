<Query Kind="Program">
  <Connection>
    <ID>59553889-9cb7-45f9-a747-6a218e67e869</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>192.168.1.250</Server>
    <Database>steve_the_trade_bot_prod</Database>
    <UserName>sttb_prod</UserName>
    <SqlSecurity>true</SqlSecurity>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAKvXpSbg/+E2KO5TjxpufZgAAAAACAAAAAAAQZgAAAAEAACAAAAAtP4ZBCQIDPzDo2a5crzLdOk6849tdqjT+pXPc1GrBeAAAAAAOgAAAAAIAACAAAAAgQfg49zfj1DEoRDgdZZegKXOfP5Gciaq+Hxn6J6qOWSAAAABCZiZjCRBS1q6p28rH3Z7p4KXrDohziTx4ZSaxhMDTWkAAAAB/K+FUYvfBwvYysKWJh4cnAJqFk6pXmflg2CxYinI0wh3lbwZ7k4fIugQLjFaODym/ZmP6wz8IQuMP1lL+tCSO</Password>
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
	CheckRun();
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