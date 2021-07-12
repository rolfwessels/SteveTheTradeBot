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
</Query>

void Main()
{
	HistoricalTrades.Count().Dump("cnt");
	HistoricalTrades.OrderBy(x => x.TradedAt).Take(1).Dump();
	HistoricalTrades.OrderByDescending(x => x.TradedAt).Take(1).Dump();

	TradeFeedCandles.GroupBy(x => x.PeriodSize).Select(x => new { x.Key, Cnt = x.Count() }).Dump("cnt");

	
	var winChart = TradeFeedCandles
		.Where(x=>x.PeriodSize == 8)
		.OrderBy(x=>x.Date)
		.ToList()
		.GroupBy(x => x.Date.Date)
		.Select(x => new {Date = x.Key, AvgPrice = x.Last().Close })
		.ToList()
		.Chart(c => c.Date, c => c.AvgPrice)
		.ToWindowsChart();
	// Make tweaks/customizations:
	winChart.Series.First().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
	winChart.Dump();
}

// You can define other methods, fields, classes and namespaces here
