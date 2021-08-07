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
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Npgsql.EntityFrameworkCore.PostgreSQL</EFProvider>
      <UseNativeScaffolder>True</UseNativeScaffolder>
    </DriverData>
  </Connection>
  <NuGetReference>CsvHelper</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>CsvHelper</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

void Main()
{
	"Reading".Dump();
	var records= 
		TradeFeedCandles
		.Where(x => x.PeriodSize == 8 && x.Feed == "valr" && x.CurrencyPair == "BTCZAR" )
		.OrderBy(x=>x.Date)
		//.Take(10000)
		.ToArray()
		.Where(x=>!x.Metric.Contains("ema200\": null"))
		.ToList();
		
	var future = (60/5)*6; // 12 hours
	var output = records
			.Take(records.Count - future)
			.Select((x, i) => new { x, metric= JsonConvert.DeserializeObject<Dictionary<string,decimal?>>(x.Metric).ToDictionary(x=>x.Key,x=>x.Value.HasValue? Math.Round(x.Value.Value,5) : x.Value), future = records[i + future] })
			.Select(x => new {
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
				FutureChange = MovementPercent(x.future.Close,x.x.Close),
				});
				//.Dump();
				//.Chart(x=>x.Date,x=>x.FutureChange).Dump();


	"Saving".Dump();
	var file = @"C:\temp\btc-data.txt";
	using (var writer = new StreamWriter(file))
	using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
	{
		csv.WriteRecords(output);
	}
	$"Saved {file}".Dump();
}

public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
{
	if (fromValue == 0) fromValue = 0.00001m;
	return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
}

// You can define other methods, fields, classes and namespaces here
