using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// all tools to clean and generate stuff
/// from imported content
/// </summary>
namespace fwp.localizator.editor
{
	using fwp.localizator;
	using UnityEditor;
	using System.IO;
	using System.Text;

	public class GenerateSheetUtils
	{
		static public bool verbose => LocalizationManager.verbose;

		static public string raw_file_locate(DataSheetTab tab)
		{
			// fetching all tabs txts
			string[] tabsFiles = Directory.GetFiles(
				LocalizationPaths.sysImports,
				"*" + LocalizationPaths.langExtDot);

			if (tabsFiles.Length <= 0)
			{
				Debug.LogWarning("no tabs txt @ " + LocalizationPaths.sysImports);
				return null;
			}

			foreach(var t in tabsFiles)
			{
				if (t.Contains(tab.tabUrlId))
					return t;
			}

			return null;
		}

		static public void csv_file_generate(DataSheetTab tab)
		{
			string path = raw_file_locate(tab);

			Debug.Log("parsing : " + path);

			// read and create csv
			var csv = new CsvParser(tab, path);

			if (tab.parseType == SheetParseType.autofill)
			{
				// solve all UIDs inc numbering
				csv.fillAutoUid((int)tab.tabParams.uidColumn);
			}

			// save
			csv.save();
		}

		static public void csvs_generate(LocaDataSheet[] sheets)
		{
			foreach(var sheet in sheets)
			{
				foreach(var tab in sheet.tabs)
				{
					csv_file_generate(tab);
				}
			}
		}

		static public void trads_generate(LocaDataSheet[] sheets)
		{
			foreach (var sheet in sheets)
			{
				foreach (var tab in sheet.tabs)
				{
					trad_generate(tab);
				}
			}
		}

		static public void trad_generate(DataSheetTab tab)
		{
			var sups = LocalizationManager.instance.getSupportedLanguages();

			if (verbose)
				Debug.Log("generating for x" + sups.Length + " languages");

			EditorUtility.DisplayProgressBar("converting loca", "loading...", 0f);

			for (int i = 0; i < sups.Length; i++)
			{
				IsoLanguages lang = sups[i];

				EditorUtility.DisplayProgressBar("converting loca", "langage : " + lang.ToString(), (1f * i) / (1f * sups.Length));

				trad_file_generate(lang);
			}

			EditorUtility.ClearProgressBar();

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// merge all tabs into a single file for given language
		/// </summary>
		static public void trad_file_generate(IsoLanguages lang)
		{
			var parsers = CsvParser.loadParsers();

			StringBuilder output = new StringBuilder();

			// all tabs
			for (int i = 0; i < parsers.Length; i++)
			{
				var csv = parsers[i];

				// generate file
				string tabOutput = generateLangFile(csv, lang);

				output.AppendLine(tabOutput);
			}

			//save

			if (!Directory.Exists(LocalizationPaths.sysLangs))
				Directory.CreateDirectory(LocalizationPaths.sysLangs);

			string outputPath = Path.Combine(
				LocalizationPaths.sysLangs,
				"lang_" + lang + LocalizationPaths.langExtDot);

			if (verbose)
				Debug.Log("saving : " + outputPath + " (" + output.Length + " char)");

			File.WriteAllText(outputPath, output.ToString());
		}

		/// <summary>
		/// generate the list of key=value
		/// for localization per lang
		/// </summary>
		static string generateLangFile(CsvParser csv, IsoLanguages lang)
		{
			//string tabContent = File.ReadAllText(filePath);
			//CsvParser csv = CsvParser.parse(tabContent, param.langLineIndex);

			int langColumnIndex = csv.getLangColumnIndex(lang); //  search for matching language column
			if (langColumnIndex < 0) return string.Empty;

			StringBuilder output = new StringBuilder();

			var param = csv.Tab.tabParams;

			Debug.Log($"solveTabAutoId <b>lang : {lang}</b> , ssheet UIDs column <b>#{param.uidColumn}</b> , lines x" + csv.lines.Count);

			//first line is languages
			for (int j = 1; j < csv.lines.Count; j++)
			{
				var values = csv.lines[j].cells;

				if (values.Count < 2)
				{
					if (verbose) Debug.LogWarning("empty line #" + j);
					continue; // skip empty lines
				}

				// lang column overflow provided datas
				if (langColumnIndex >= values.Count)
				{
					if (verbose) Debug.LogWarning("lang overflow #" + j);
					continue;
				}

				var key = values[(int)param.uidColumn]; // uid key

				if (verbose) Debug.Log("  checking key=" + key);

				string langValue = sanitizeValue(values[langColumnIndex]);

				// skipping empty values
				if (langValue.Length <= 2)
				{
					if (verbose) Debug.LogWarning("empty value (key=" + key + ") #" + j);
					continue;
				}

				string line = key + "=" + langValue;

				if (verbose)
					Debug.Log("     append : " + line);

				output.AppendLine(line);
			}

			return output.ToString();
		}

		static string sanitizeValue(string val)
		{
			val = val.Trim();

			//remove "" around escaped values
			val = val.Replace(ParserStatics.SPREAD_CELL_ESCAPE_VALUE.ToString(), string.Empty);

			return val;
		}

	}
}