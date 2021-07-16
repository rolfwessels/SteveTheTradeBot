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
    </DriverData>
  </Connection>
  <Reference Relative="..\..\src\SteveTheTradeBot.Api\bin\Debug\netcoreapp3.1\Newtonsoft.Json.dll">D:\Work\Home\SteveTheTradeBot\src\SteveTheTradeBot.Api\bin\Debug\netcoreapp3.1\Newtonsoft.Json.dll</Reference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
</Query>

void Main()
{
	HistoricalTrades.Count().Dump("cnt");
	HistoricalTrades.OrderBy(x => x.TradedAt).Take(1).Dump();
	HistoricalTrades.OrderByDescending(x => x.TradedAt).Take(1).Dump();

	//TradeFeedCandles.GroupBy(x => x.PeriodSize).Select(x => new { x.Key, Cnt = x.Count() }).Dump("cnt");

	DynamicPlots.Select(x=>x.Feed).Distinct().Dump("");
	
	var winChart = DynamicPlots
		.Where(x=>x.Feed == "afsd");
		.OrderBy(x => x.Date)
		.ToList()
		.Chart(c => c.Date, c => c.Value)
		.ToWindowsChart();
	// Make tweaks/customizations:
	winChart.Series.First().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
	winChart.Dump();

	var winChart1 = TradeFeedCandles
		.Where(x=>x.PeriodSize == 8)
		.OrderBy(x=>x.Date)
		.ToList()
		.ToList()
		.Chart(c => c.Date, c => c.Close)
		.ToWindowsChart();
	// Make tweaks/customizations:
	winChart1.Series.First().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
	winChart1.Dump();

	
}
void TestFeeds()
{
	var values = ((JArray)JsonConvert.DeserializeObject(json)).OfType<dynamic>().Select(x => new { startTime = DateTime.Parse(x.startTime.ToString()), x.close, x.high, x.low, x.open, x.volume }).ToList().Dump("asd");
	TradeFeedCandles
		.Where(x => x.PeriodSize == 6 && values.Select(e => e.startTime).Contains(x.Date))
		.OrderByDescending(x => x.Date)
		.Dump();

}
// You can define other methods, fields, classes and namespaces here
string json = @"[{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T03:30:00Z','open':'573671','high':'575300','low':'573001','close':'573001','volume':'1.5544325','quoteVolume':'893010.63200719'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T03:00:00Z','open':'570999','high':'575997','low':'570997','close':'573686','volume':'0.39741114','quoteVolume':'227713.75233002'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T02:30:00Z','open':'570423','high':'571148','low':'568001','close':'570090','volume':'2.88009665','quoteVolume':'1644668.20067655'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T02:00:00Z','open':'567001','high':'571998','low':'567001','close':'571998','volume':'0.8038486','quoteVolume':'457577.88699273'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T01:30:00Z','open':'570363','high':'570363','low':'568000','close':'568000','volume':'0.48940255','quoteVolume':'278606.20282481'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T01:00:00Z','open':'572426','high':'573213','low':'570002','close':'570647','volume':'3.41653786','quoteVolume':'1948337.12815064'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T00:30:00Z','open':'577214','high':'577214','low':'570693','close':'571916','volume':'4.20792644','quoteVolume':'2428109.69644623'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-15T00:00:00Z','open':'574495','high':'578000','low':'570408','close':'577189','volume':'2.6087976','quoteVolume':'1500846.79850065'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-14T23:30:00Z','open':'573000','high':'574933','low':'572244','close':'573863','volume':'0.63903021','quoteVolume':'366055.64648335'},{'currencyPairSymbol':'BTCZAR','bucketPeriodInSeconds':1800,'startTime':'2021-06-14T23:00:00Z','open':'569070','high':'572999','low':'568620','close':'572169','volume':'0.24053961','quoteVolume':'136969.15973339'}]";