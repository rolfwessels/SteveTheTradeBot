<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.Tasks.dll</Reference>
  <NuGetReference>Flurl</NuGetReference>
  <NuGetReference>Flurl.Http</NuGetReference>
  <NuGetReference>Humanizer</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Flurl</Namespace>
  <Namespace>Flurl.Util</Namespace>
  <Namespace>Humanizer</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>Flurl.Http</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

void Main()
{

	
	var person = "http://localhost:5000/connect/token"
	
	.PostAsync(new FormUrlEncodedContent(new Dictionary<String,String> {
		{"grant_type", "password"},
			{"scope", "api"},
			{"username", "admin@admin.com"},
			{"password", "admin!"},
			{"client_id","coredocker.api"},
			{"client_secret","super_secure_password"}
	}))
	.ReceiveJson<Token>()
	.Result
	.Let(t => new { Authorization = $"{t.Token_Type} {t.Access_Token}"})
	.ToJson()
	.Dump();
}
public class Token
{
	public string Access_Token { get; set; }
	public int Expires_In { get; set; }
	public string Token_Type { get; set; }
	public string Scope { get; set; }
}

static class ObjectExtensions
{
	// Kotlin: fun <T, R> T.let(block: (T) -> R): R
	public static R Let<T, R>(this T self, Func<T, R> block)
	{
		return block(self);
	}

	public static string ToJson<T>(this T self)
	{
		return Newtonsoft.Json.JsonConvert.SerializeObject(self);
	}
	
	// Kotlin: fun <T> T.also(block: (T) -> Unit): T
	public static T Also<T>(this T self, Action<T> block)
	{
		block(self);
		return self;
	}
}