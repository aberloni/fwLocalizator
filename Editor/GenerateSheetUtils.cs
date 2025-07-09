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

			// [path]/tab_uid.txt
			foreach (var t in tabsFiles)
			{
				if (t.Contains(tab.TxtFileName)) return t;
			}

			return null;
		}

		static public void csv_file_generate(DataSheetTab tab)
		{
			string path = raw_file_locate(tab);

			Debug.Log("parsing (" + tab.tabUrlId + "|" + tab.parseType + ") : " + path);

			// read and create csv
			var csv = new CsvParser(tab, path);

			if (tab.parseType == SheetParseType.autofill)
			{
				// solve all UIDs inc numbering
				csv.fillAutoUid((int)tab.tabParams.uidColumn);
			}

			csv.generateLocalization();

			// save
			csv.save();
		}

		static public void csv_generate(LocaDataSheet sheet)
		{
			foreach (var tab in sheet.tabs)
			{
				csv_file_generate(tab);
			}
		}
		static public void csvs_generate(LocaDataSheet[] sheets)
		{
			foreach (var sheet in sheets)
			{
				csv_generate(sheet);
			}
		}

		static public void trads_generate()
		{
			var sups = LocalizationManager.instance.getSupportedLanguages();

			if (verbose)
				Debug.Log("generating for x" + sups.Length + " languages");

			EditorUtility.DisplayProgressBar("converting loca", "loading...", 0f);

			for (int i = 0; i < sups.Length; i++)
			{
				IsoLanguages lang = sups[i];

				EditorUtility.DisplayProgressBar(
					"converting loca", "langage : " + lang.ToString(), (1f * i) / (1f * sups.Length));

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

			Debug.Log("generating file : " + lang);
			Debug.Log("parsers x" + parsers.Length);

			foreach (var csv in parsers)
			{
				string content = csv.getLangFileContent(lang);
				Debug.Log("	+csv	" + csv.ParserFileName + "	(" + lang + ")	len:" + content.Length);

				output.AppendLine(content);
			}

			//save

			if (!Directory.Exists(LocalizationPaths.sysLangs))
			{
				Debug.Log("create missing directory : " + LocalizationPaths.sysLangs);
				Directory.CreateDirectory(LocalizationPaths.sysLangs);
			}

			string outputPath = Path.Combine(
				LocalizationPaths.sysLangs,
				"lang_" + lang + LocalizationPaths.langExtDot);

			Debug.Log("saving : " + outputPath + " (" + output.Length + " char)");

			// https://learn.microsoft.com/en-us/dotnet/api/system.io.file.writealltext?view=net-9.0
			File.WriteAllText(outputPath, output.ToString());
		}

	}
}