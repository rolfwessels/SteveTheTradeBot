<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.Tasks.dll</Reference>
  <NuGetReference>Humanizer</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Humanizer</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>


void Main()
{
	var processor = new Processor( @"..\..\",@"User",@"Project");
	processor.Scaffold();
}
class Processor
{
	string _location;
	string _template;
	string _toName;
	string[] _fileTypes;
	string[] _exclude;
	string _focus;
	string _templateFolder;
	string _toNameFolder;
	bool _copyScaffold;
	public Processor(string location,
		string template,
		string toName,
		string focus = "",
		string templateFolder = null,
		string toNameFolder = null,
		bool copyScaffold = false)
	{
		_location = location;
		_template = template;
		_toName = toName;
		_focus = focus;
		_templateFolder = templateFolder;
		_toNameFolder = toNameFolder;
		_copyScaffold = copyScaffold;

		 _fileTypes = new[] { @".cs", ".js", ".ts", ".html", ".scss", ".txt", ".json", ".less" };
		 _exclude = new[] { @"bower_components", ".OAuth2.", "RequestClientDetailsHelper", "Mappers\\MapClient.cs", "obj\\", "Enums\\", "node_modules", ".tmp", "build\\" };
	}

	public void Scaffold()
	{
		_location = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), _location)).Dump();
		var files = Directory.GetFiles(_location, "*" + _template + "*", SearchOption.AllDirectories)
					.Where(file => _fileTypes.Contains(Path.GetExtension(file)) && !_exclude.Any(x => file.Contains(x)))
					.OrderByDescending(x => x.Contains(_focus));
		_templateFolder = _templateFolder ?? GetFolderName(_template);
		_toNameFolder = GetFolderName(_toName) ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_toName.Humanize().Split(' ').Last().Humanize());

		var fileReplaces = files.Select(x => new { File = x, Replace = ReplaceAll(x), Exists = File.Exists(ReplaceAll(x)) }).ToList();
		//fileReplaces.Where(x=>x.Exists).Select(x=> x.Replace.Replace(_location,"")).Dump("Existing files");
		fileReplaces.Where(x => !x.Exists).Select(x => x.Replace.Replace(_location, "")).Dump("Missing files");



		var replaceOption = "";
		foreach (var replace in fileReplaces)
		{
			var file = replace.File;
			var newFile = replace.Replace;

			if (!replace.Exists)
			{

				replaceOption = replaceOption == "A" ? replaceOption : Util.ReadLine("Would you like to create " + newFile + " [Y/n/a/p]").ToUpper();
				replaceOption = string.IsNullOrEmpty(replaceOption) ? "Y" : replaceOption;
				if (replaceOption == "Y" || replaceOption == "A")
				{
					var fileContent = File.ReadAllText(file);

					fileContent = InjectScaffolding(fileContent);
					var path = Path.GetDirectoryName(newFile);
					if (!Directory.Exists(path)) Directory.CreateDirectory(path);
					var replacedContent = ReplaceAll(fileContent);
					File.WriteAllText(newFile, replacedContent);
					newFile.Dump("Created");
					AddFileToProject(newFile, file);

				}
				else if (replaceOption == "P")
				{
					var fileContent = File.ReadAllText(file);
					fileContent.Dump("Preview " + ReplaceAll(fileContent));
				}
				else
				{
					newFile.Dump("Skip");
				}
			}

		}
	}
	public string GetFolderName(string filename)
	{
		return Directory.GetFiles(_location, filename + ".cs", SearchOption.AllDirectories).Select(x => Path.GetFileName(Path.GetDirectoryName(x))).FirstOrDefault().Dump(filename);
	}


	public string InjectScaffolding(string fileData)
	{
		var prefix = "/* scaffolding";
		var suffix = "scaffolding */";
		var start = fileData.IndexOf(prefix);
		var end = fileData.IndexOf(suffix, Math.Max(0, start));
		if (start < 0 || end < 0) return fileData;
		var data = fileData.Substring(start + prefix.Length, end - start - prefix.Length);
		var injections = JsonConvert.DeserializeObject<List<FileInjection>>(data);
		var allfiles = Directory.GetFiles(_location, "*", SearchOption.AllDirectories)
		.Where(file => !_exclude.Any(x => file.Contains(x)))
		.ToArray();

		foreach (var inject in injections)
		{
			var inFile = allfiles.Where(x => x.EndsWith("\\" + inject.FileName)).FirstOrDefault();
			if (inFile != null)
			{
				var projectFile = File.ReadAllLines(inFile).ToList();
				var found = false;
				for (int i = 0; i < projectFile.Count; i++)
				{

					if (projectFile[i].Contains(inject.Indexline))
					{
						found = true;
						var indent = Regex.Match(projectFile[i], @"(\s*)[^\s]").Groups[1].Value;
						var indexOf = projectFile[i].IndexOf(inject.Indexline) + inject.Indexline.Length;
						foreach (var addLine in inject.Lines)
						{
							var insertLine = ReplaceAll(addLine);
							if (inject.InsertInline)
							{
								projectFile[i] = projectFile[i].Insert(indexOf, insertLine);
							}
							else
							{
								var inLine = i + (inject.InsertAbove ? -1 : 0);
								projectFile.Insert(inLine + 1, indent + insertLine);
								i++;
							}
						}
						break;
					}
				}
				if (!found) ("****** could not find " + inject.Indexline + " in the file " + inFile).Dump();
				//if (inject.InsertInline)projectFile.Dump();
				TryIt(10,() => File.WriteAllLines(inFile, projectFile.ToArray()));

			}
			else
			{
				("****** could not find " + inject.FileName).Dump();
			}

		}

		if (_copyScaffold) return fileData;
		return fileData.Substring(0, start).Trim();
	}
	public void TryIt(int retryCount, Action action)
	{
		Exception last = null;
		for (int i = 0; i < retryCount; i++)
		{
			try
			{
				action();
				return;
			}
			catch (Exception ex)
			{
				last = ex;
				ex.Message.Dump();
				Thread.Sleep(1);
			}
		}
		throw last;
	}
	class Injection
	{
		public List<FileInjection> Injections { get; set; }
	}
	public class FileInjection
	{
		public string FileName { get; set; }
		public string Indexline { get; set; }
		public bool InsertAbove { get; set; }
		public bool InsertInline { get; set; }
		public string[] Lines { get; set; }

	}

	public string AddFileToProject(string fileName, string oldFile)
	{
		string projectName = null;
		var path = Path.GetDirectoryName(fileName);
		do
		{
			projectName = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
			path = Path.GetDirectoryName(path);
		} while (string.IsNullOrEmpty(projectName) && !string.IsNullOrEmpty(path));
		if (!string.IsNullOrEmpty(projectName))
		{
			var projectFile = File.ReadAllLines(projectName).ToList();
			for (int i = 0; i < projectFile.Count; i++)
			{

				if (projectFile[i].Contains("\\" + Path.GetFileName(oldFile)))
				{

					projectFile.Insert(i + 1, ReplaceAll(projectFile[i]).Dump());
				}
			}
			File.WriteAllLines(projectName, projectFile.ToArray());
		}
		return projectName;
	}


	public string ReplaceAll(string text)
	{
		if (text == null) return null;
		var replaced = text
			.Replace(_template.Pluralize(), _toName.Pluralize()) // StockCategories Samples
			.Replace(InitialLower(_template.Pluralize()), InitialLower(_toName.Pluralize()))  // stockCategories samples
			.Replace(_template, _toName) // StockCategory Sample
			.Replace(InitialLower(_template), InitialLower(_toName)); // stockCategory sample
		if (_templateFolder != null)
		{
			replaced = replaced
				.Replace("\\Accounts\\" + _templateFolder + "\\", "\\" + _toNameFolder + "\\")
				.Replace("\\" + _templateFolder + "\\", "\\" + _toNameFolder + "\\")
				.Replace(".Accounts." + _templateFolder, "." + _toNameFolder)
				.Replace("." + _templateFolder, "." + _toNameFolder);
		}
		return replaced;
	}


	public string InitialLower(string text)
	{
		return text.Substring(0, 1).ToLower() + text.Substring(1);
	}
}


// Define other methods and classes here