using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace fwp.localizator
{
	/// <summary>
	/// contains raw text (from spreadsheet)
	/// and parsed lines
	/// </summary>
	public class LocalizationFile
	{
		const char KEY_VALUE_SPLIT = '=';
		public const char SPREADSHEET_CELL_BREAK = ','; // spreadsheet cell separator

		public const char LOCALIZ_CHAR_COMMENT = '#'; //id=content
		public const string LOCALIZ_CHAR_COMMENT2 = "\\\\"; //id=content
		public const char LOCALIZ_CHAR_SPLIT = '='; //id=content
		public const string LOCALIZ_MULTIPLE_BEGIN = "<multiple>"; //id=content
		public const string LOCALIZ_MULTIPLE_END = "</multiple>"; //id=content
		public const string FILESEPARATOR = "@";

		public override string ToString()
		{
			return base.ToString() + "_" + iso;
		}

		//public string lang_name = ""; // fr, en, ...
		public IsoLanguages iso;

		public TextAsset textAsset;

		public bool editor_fold;

		string[] rawLines = null;
		FileLine[] lines = null;

		public struct FileLine
		{
			public override string ToString() => key + KEY_VALUE_SPLIT + value;

			/// <summary>
			/// key present in file (might have -digits)
			/// </summary>
			public string key;

			/// <summary>
			/// key without trailing numbers
			/// </summary>
			public string keyRoot;

			/// <summary>
			/// =value
			/// </summary>
			public string value;
		}

		// public string[] GetRawLines() => rawLines;
		public FileLine[] GetLines() => lines;
		public int GetLinesCount() => lines.Length;

		public bool IsLoaded => textAsset != null;

		public LocalizationFile(IsoLanguages lang)
		{
			iso = lang;
			set();
		}

		public void edRefresh() => set();

		void set()
		{
			string langFilePath = getLangFileResourcesPath(iso, false); // resources/, path to lang file, no ext
			textAsset = Resources.Load(langFilePath) as TextAsset;

			if (textAsset == null)
			{
				Debug.LogWarning("no file @ " + langFilePath);
				return;
			}

			rawLines = splitLineBreak(textAsset.text);

			List<string> tmp = new List<string>();
			bool multipleLine = false;
			bool firstMultipleLine = false;
			foreach (string line in rawLines)
			{
				if (line.StartsWith(LOCALIZ_CHAR_COMMENT)) continue;
				if (line.Length <= 1)
				{
					if (multipleLine) tmp[tmp.Count - 1] += "\n";
					else continue;
				}

				if (line.StartsWith(LOCALIZ_MULTIPLE_BEGIN)) { multipleLine = true; firstMultipleLine = true; continue; }
				if (line.StartsWith(LOCALIZ_MULTIPLE_END)) { multipleLine = false; continue; }

				if (multipleLine)
				{
					if (firstMultipleLine)
						tmp.Add(line);
					else
						tmp[tmp.Count - 1] += "\n" + line;
					firstMultipleLine = false;
				}
				else
					tmp.Add(line);
			}
			rawLines = tmp.ToArray();

			List<FileLine> _lines = new();
			foreach (var raw in rawLines)
			{
				var kp = raw.Split(KEY_VALUE_SPLIT);

				var line = new FileLine()
				{
					key = kp[0],
					value = kp[1]
				};

				// key-num
				int dashIndex = line.key.LastIndexOf("-");

				// has "-" && last char is digit ?
				if (dashIndex > 0 && char.IsDigit(line.key[^1])) line.keyRoot = line.key.Substring(0, dashIndex);
				else line.keyRoot = line.key;

				_lines.Add(line);
			}
			lines = _lines.ToArray();
			//Debug.Log("  " + path + " | " + lines.Length + " lines");
		}

		/// <summary>
		/// loca file has a key matching param
		/// removeDigit : should we compare with ending digits or not
		/// 
		/// this is a strict comparison : keys MUST match (space & case)
		/// </summary>
		public bool hasId(string id, bool ignoreDigits)
		{
			if (string.IsNullOrEmpty(id)) return false;

			if (lines.Length <= 0)
			{
				Debug.LogWarning("hasId :: no lines");
				return false;
			}

			foreach (var l in lines)
			{
				// must ignore digits ? for pattern : key{-num} => remove it
				if (ignoreDigits && l.keyRoot == id) return true;
				if (l.key == id) return true;
			}
			return false;
		}

		public string getContentById(string id)
		{
			if (id == null) return "[no id]";

			if (id.Length <= 0)
			{
				if (LocalizationMind.Verbose) Debug.LogWarning("no id given to gather content loca ?");
				return "[no id given / empty]";
			}

			//Debug.Log("searching for " + id);
			for (int i = 0; i < rawLines.Length; i++)
			{
				string key = rawLines[i].Split('=')[0];

				key = key.Trim();
				id = id.Trim();

				//srt_outro_museum_05 == srt_outro_museum_05
				//Debug.Log(key + " ("+key.Length+") == " + id+" ("+id.Length+")");

				if (key == id) return getContentAtLine(i);
			}

			if (LocalizationMind.Verbose)
			{
				Debug.LogWarning($"getContentById() FAILED	# <b>" + id + "</b>");
				Debug.LogWarning($"lang:<b>{iso}</b> & lines x" + rawLines.Length);
			}

			return "['" + id + "' missing in " + iso + "]";
		}

		public string getContentAtLine(int idx)
		{
			if (!rawLines[idx].Contains("" + LocalizationFile.LOCALIZ_CHAR_SPLIT)) return "";
			string[] split = rawLines[idx].Split(LocalizationFile.LOCALIZ_CHAR_SPLIT);
			string output = "";
			for (int i = 1; i < split.Length; i++)
			{
				if (i != 1) output += "=";
				output += split[i];
			}

			//dans la trad on a des | pour faire des \n
			output = output.Replace(ParserStatics.CELL_LINE_BREAK.ToString(), System.Environment.NewLine);

			return output;
		}

#if UNITY_EDITOR
		/// <summary>
		/// compare two files to check if keys[] match
		/// all keys contained into this file must exist in given other
		/// </summary>
		public bool edCompareKeys(LocalizationFile other)
		{
			foreach (var l in lines)
			{
				if (!other.GetLines().Any(o => o.key == l.key))
				{
					Debug.LogError($"{other.iso} is missing id " + l.key + " (presnet in file {iso})");
					return false;
				}
			}
			return true;
		}
#endif

		string getIdAtLine(int idx)
		{
			if (!rawLines[idx].Contains(LocalizationFile.LOCALIZ_CHAR_SPLIT)) return string.Empty;
			string[] split = rawLines[idx].Split(LocalizationFile.LOCALIZ_CHAR_SPLIT);
			return split[0];
		}

		public bool overrideKey(string key, string newText)
		{
			for (int i = 0; i < rawLines.Length; i++)
			{
				if (!rawLines[i].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
				if (!rawLines[i].Contains(key)) continue;

				string[] splited = rawLines[i].Split(LOCALIZ_CHAR_SPLIT);
				if (splited[0] != key) continue;
				if (splited[1] == newText) continue;
				splited[1] = newText;
				rawLines[i] = splited[0] + LOCALIZ_CHAR_SPLIT + splited[1];
				return true;
			}
			return false;
		}

		public void rewriteAsset()
		{
			StreamWriter file = new StreamWriter("Assets/Resources/" + getLangFileResourcesPath(iso));

			string[] rawLines = splitLineBreak(textAsset.text);
			int numberOfRewrite = 0;

			for (int i = 0; i < rawLines.Length; i++)
			{
				if (!rawLines[i].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
				string[] splited = rawLines[i].Split(LOCALIZ_CHAR_SPLIT);

				for (int j = 0; j < rawLines.Length; j++)
				{
					if (!rawLines[j].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
					string[] rawSplited = rawLines[j].Split(LOCALIZ_CHAR_SPLIT);
					if (rawSplited[0] != splited[0]) continue;
					rawLines[j] = rawLines[i];
					numberOfRewrite++;
				}
			}

			Debug.Log("rewrited " + numberOfRewrite + " lines");

			string allNew = "";
			for (int i = 0; i < rawLines.Length; i++)
			{
				allNew += rawLines[i] + Environment.NewLine;
			}

			file.Write(allNew);
			file.Close();
		}

		/// <summary>
		/// format path to language file
		/// path within Resources/
		/// </summary>
		public string getLangFileResourcesPath(IsoLanguages iso, bool ext = true)
		{
			return Path.Combine(LocalizationPaths.folderLocalization,
				LocalizationPaths.folderLangs,
				"lang_" + iso + (ext ? LocalizationPaths.langExtDot : string.Empty));
		}

		static public string[] splitLineBreak(string fileContent)
		{
			//return fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.None);
			return fileContent.Split(new string[] { "\r\n", Environment.NewLine }, StringSplitOptions.None);
		}

	}
}