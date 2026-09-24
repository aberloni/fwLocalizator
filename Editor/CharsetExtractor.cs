using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator.editor
{
	/// <summary>
	/// extract uniq characters from generated localization files
	/// values only (keys & comments are ignored)
	///
	/// output is meant for TMP Font Asset Creator : Character Set > Characters from File
	/// </summary>
	static public class CharsetExtractor
	{
		const string menu_path = LocalizationVars.menupath_routines + "charsets/";

		/// <summary>
		/// Resources/localization/charsets/
		/// </summary>
		static public string pathCharsets = LocalizationPaths.exportPathBase + "charsets/";
		static public string sysCharsets => Path.Combine(Application.dataPath, pathCharsets).Replace("\\", "/");

		const string file_prefix = "charset_";
		const string file_all = file_prefix + "all";

		/// <summary>
		/// hand-written file, never overwritten
		/// its characters are merged into "all"
		/// </summary>
		const string file_custom = file_prefix + "custom";
		static public string sysCustom => sysCharsets + file_custom + ".txt";

		[MenuItem(menu_path + "extract")]
		static void menuExtract() => extract();

		[MenuItem(menu_path + "log")]
		static void menuLog()
		{
			foreach (var kp in solveAll())
			{
				Debug.Log("Loca> charset." + kp.Key + " x" + kp.Value.Count + "\n" + stringify(kp.Value));
			}
		}

		[MenuItem(menu_path + "open folder")]
		static void menuBrowse()
		{
			// nothing extracted yet : extract first
			if (!Directory.Exists(sysCharsets)) extract();
			if (!Directory.Exists(sysCharsets)) return;

			OpenInFileBrowser.OpenLocalFile(sysCharsets);
		}

		/// <summary>
		/// one file per language + one file merging all languages
		/// </summary>
		static public void extract()
		{
			var sets = solveAll();
			if (sets.Count <= 1)
			{
				Debug.LogWarning("Loca> no language file found, nothing to extract");
				return;
			}

			if (!Directory.Exists(sysCharsets)) Directory.CreateDirectory(sysCharsets);

			StringBuilder report = new();
			report.AppendLine(file_custom + " x" + solveCustom().Count + " (merged in " + file_all + ")");
			foreach (var kp in sets)
			{
				string path = Path.Combine(sysCharsets, kp.Key + ".txt");
				File.WriteAllText(path, stringify(kp.Value), new UTF8Encoding(false));
				report.AppendLine(kp.Key + " x" + kp.Value.Count);
			}

			AssetDatabase.Refresh();

			Debug.Log("Loca> charsets extracted @ " + sysCharsets + "\n" + report);
		}

		/// <summary>
		/// file name => sorted uniq codepoints
		/// </summary>
		static SortedDictionary<string, SortedSet<int>> solveAll()
		{
			SortedDictionary<string, SortedSet<int>> output = new();
			SortedSet<int> all = new();

			foreach (IsoLanguages iso in System.Enum.GetValues(typeof(IsoLanguages)))
			{
				var set = solve(iso);
				if (set == null) continue;

				output.Add(file_prefix + iso, set);
				all.UnionWith(set);
			}

			all.UnionWith(solveCustom());

			output.Add(file_all, all);

			return output;
		}

		/// <summary>
		/// characters from custom file
		/// generates an empty one if missing
		/// </summary>
		static SortedSet<int> solveCustom()
		{
			SortedSet<int> set = new();

			if (!File.Exists(sysCustom))
			{
				if (!Directory.Exists(sysCharsets)) Directory.CreateDirectory(sysCharsets);
				File.WriteAllText(sysCustom, string.Empty, new UTF8Encoding(false));
				AssetDatabase.Refresh();

				Debug.Log("Loca> generated empty custom charset @ " + sysCustom);
				return set;
			}

			addCodepoints(File.ReadAllText(sysCustom, Encoding.UTF8), set);
			return set;
		}

		/// <summary>
		/// null : no file for this language
		/// </summary>
		static public SortedSet<int> solve(IsoLanguages iso)
		{
			// check presence first : LocalizationFile warns when missing
			string resPath = Path.Combine(LocalizationPaths.folderLocalization, LocalizationPaths.folderLangs, "lang_" + iso);
			if (Resources.Load<TextAsset>(resPath) == null) return null;

			LocalizationFile file = new LocalizationFile(iso);
			if (!file.IsLoaded) return null;

			SortedSet<int> set = new();
			foreach (var line in file.GetLines())
			{
				if (string.IsNullOrEmpty(line.value)) continue;
				addCodepoints(line.value, set);
			}
			return set;
		}

		/// <summary>
		/// codepoints to handle surrogate pairs (emojis, rare CJK)
		/// </summary>
		static void addCodepoints(string value, SortedSet<int> set)
		{
			for (int i = 0; i < value.Length; i++)
			{
				int cp;
				if (char.IsSurrogatePair(value, i))
				{
					cp = char.ConvertToUtf32(value, i);
					i++;
				}
				else
				{
					char c = value[i];
					if (char.IsControl(c) || char.IsSurrogate(c)) continue; // \n \r \t & orphan surrogates
					cp = c;
				}
				set.Add(cp);
			}
		}

		static string stringify(SortedSet<int> set)
		{
			StringBuilder sb = new();
			foreach (int cp in set) sb.Append(char.ConvertFromUtf32(cp));
			return sb.ToString();
		}
	}
}
