using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator
{
	using System.IO;
	using System.Text;

	/// <summary>
	/// must fill with leading numbers
	/// </summary>
	public enum SheetParseType
	{
		none = 0,
		autofill, // add 01,02,03 if UID doesn't change
	}

	[System.Serializable]
	public class CsvLineRaw
	{
		public string raw = string.Empty;
		public List<string> cells = new();

		public CsvLineRaw(string raw)
		{
			this.raw = raw;
		}

		public string stringify()
		{
			string output = string.Empty;
			for (int i = 0; i < cells.Count; i++)
			{
				if (i > 0) output += ParserStatics.SPREAD_CELL_SEPARATOR;
				output += cells[i];
			}
			return output;
		}
	}

	/// <summary>
	/// parsed values
	/// array match index of iso enum
	/// </summary>
	[System.Serializable]
	public class CsvLineLang
	{
		/// <summary>
		/// key to query to find localization
		/// </summary>
		public string key = string.Empty;

		/// <summary>
		/// each localization sorted by IsoLanguage enum
		/// </summary>
		public List<string> localized = new();

		public CsvLineLang(CsvLineRaw raw, int uidColumn)
		{
			key = raw.cells[uidColumn];
		}

		public void addLang(IsoLanguages iso, string loca)
		{
			if ((int)iso >= localized.Count)
			{
				while (localized.Count <= (int)iso)
				{
					localized.Add(string.Empty);
				}
			}

			localized[(int)iso] = loca;
		}
	}

	[System.Serializable]
	public class CsvParser
	{
		public string tabUid;
		public string fileContentRaw;

		public DataSheetTab Tab => LocalizatorUtils.tab_fetch(tabUid);

		/// <summary>
		/// mirror of txt CSV cell repartition but include some text treatment
		/// </summary>
		public List<CsvLineRaw> lines = new();

		/// <summary>
		/// sorted by languages
		/// </summary>
		public List<CsvLineLang> localizes = new();

		public void logRaw()
		{
			for (int i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				//Debug.Log("#" + i + " => " + line.stringify());
				Debug.Log("#" + i + " cells x" + line.cells.Count);
				foreach (var c in line.cells) Debug.Log(" > " + c);
			}
		}

		public void logLocalized(IsoLanguages iso)
		{
			Debug.Log("parser.log : " + iso + " x" + localizes.Count);
			foreach (var l in localizes)
			{
				Debug.Log(l.key + " = " + l.localized[(int)iso]);
			}
		}

		public int getLangColumnIndex(IsoLanguages lang, bool warning = false)
		{
			if (lines.Count <= 0)
			{
				Debug.Assert(lines.Count > 0, "no lines in csv ?");
				return -1;
			}

			// after CSV treatment on import, header is removed,
			// language are on first line
			string[] langs = lines[0].cells.ToArray(); // each lang of cur line

			string langStr = lang.ToString().ToLower();

			int langColumnIndex = -1;
			for (int j = 0; j < langs.Length; j++)
			{
				string cellLang = langs[j].Trim().ToLower();

				if (cellLang.Length < 2) // empty
					continue;

				//if(verbose) Debug.Log(cellLang + " (x" + langs.Length + ") vs " + langStr);

				// is the right language ?
				if (cellLang == langStr)
				{
					langColumnIndex = j;
					//Debug.Log("found lang "+lang+" at column #"+ langColumnIndex);
				}
			}

			if (langColumnIndex < 0)
			{
				if (warning)
				{
					Debug.LogWarning("sheet import : <b>no column</b> for lang : <b>" + lang + "</b>");
					Debug.LogWarning("out of x" + langs.Length);
				}

				return -1;
			}

			return langColumnIndex;
		}

		public CsvParser(DataSheetTab tab, string path)
		{
			generateFromPath(tab, path);
		}

		public void save() => CsvSerializer.save(this,
			LocalizationPaths.sysImports + tabUid + CsvSerializer.parserExtDot);

		public static CsvParser loadFromTabUid(string tabUid) => CsvSerializer.load(
				LocalizationPaths.sysImports + tabUid + CsvSerializer.parserExtDot);

		void generateFromPath(DataSheetTab tab, string path)
		{
			string raw = File.ReadAllText(path);

			tabUid = tab.tabUrlId;

			// some cleaning
			raw = presetupRaw(raw);

			fileContentRaw = raw;
			//Debug.Log(raw);

			// this will split lines endings & remove empty lines
			string[] rawLines = raw.Split(new char[] { ParserStatics.SPREAD_LINE_BREAK }, System.StringSplitOptions.RemoveEmptyEntries);

			int originalCount = rawLines.Length;

			for (int i = tab.tabParams.langLineIndex; i < rawLines.Length; i++)
			{
				// don't keep empty lines (if any)
				if (string.IsNullOrEmpty(rawLines[i].Trim())) continue;

				CsvLineRaw line = new CsvLineRaw(rawLines[i]);

				string[] split = cellsSeparator(rawLines[i]);

				// check if any content in line
				int cntCellWithContent = 0;
				for (int j = 0; j < split.Length; j++)
				{
					if (split[j].Length > 0)
					{
						//Debug.Log(split[j] + " (" + split[j].Length + ")");
						cntCellWithContent++;
					}
					line.cells.Add(split[j]);
				}

				//Debug.Log(line.raw + " ? " + cntCellWithContent);

				//skip line of only empty cells
				if (cntCellWithContent > 0) lines.Add(line);
			}

			foreach (var line in lines)
			{
				Debug.Assert(line != null);
				CsvLineLang llang = new(line, (int)tab.tabParams.uidColumn);
				var langs = System.Enum.GetValues(typeof(IsoLanguages));
				for (int i = 0; i < langs.Length; i++)
				{
					int langCol = getLangColumnIndex((IsoLanguages)i);
					if (langCol < 0) continue;
					llang.addLang((IsoLanguages)i, line.cells[langCol]);
				}
				localizes.Add(llang);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void fillAutoUid(int uidColumn)
		{
			string _activeUid = string.Empty;
			int cnt = 0;

			// search for first valid UID line
			int startIndex = 1;
			while (_activeUid.Length <= 0)
			{
				var lineFields = lines[startIndex].cells;
				if (lineFields.Count < 2) continue;
				var uid = lineFields[uidColumn].Trim();
				if (!string.IsNullOrEmpty(uid)) _activeUid = uid;
				else startIndex++;
			}

			//Debug.Log("autofill start # " + startIndex);

			string sheetType = lines[0].cells[uidColumn].StartsWith("dialog") ? "dialog" : "uid";

			//first line is languages header
			for (int j = startIndex; j < lines.Count; j++)
			{
				if (isEmptyLine(j)) continue;

				var key = lines[j].cells[uidColumn].Trim(); // uid key

				//Debug.Log("#" + j + "	=> <b>" + key + "</b> (active:" + _activeUid + ")");

				if (string.IsNullOrEmpty(key))
				{
					key = _activeUid;
				}
				else if (key != _activeUid)
				{
					_activeUid = key;
					cnt = 0;
				}

				key = _activeUid;
				cnt++; // UID index starts at 01

				if (cnt < 10) key = key + "-0" + cnt;
				else key = key + "-" + cnt;

				//lineFields[uidColumn] = key;

				//Debug.Log("  <b>autofilled</b> to : " + key);
			}

		}

		bool isEmptyLine(int lineIndex)
		{
			var lineFields = lines[lineIndex].cells;
			return lineFields.Count < 2;
		}

		/// <summary>
		/// empty UID but has values
		/// </summary>
		bool isLinePartOfABlock(int lineIndex, int column)
		{
			var line = lines[lineIndex];

			// empty line is end of block
			if (line.cells.Count < 2) return false;

			// https://learn.microsoft.com/en-us/dotnet/api/system.string.trim?view=net-9.0
			// Removes all leading and trailing white-space characters from the current string.
			var _uid = line.cells[column].Trim();

			// I have values but not UID => use UID of line above
			return string.IsNullOrEmpty(_uid);
		}

		bool isPartOfBlock(int lineIndex, string blockUid, int column)
		{
			var line = lines[lineIndex];

			// empty line is end of block
			if (line.cells.Count < 2) return false;

			// https://learn.microsoft.com/en-us/dotnet/api/system.string.trim?view=net-9.0
			// Removes all leading and trailing white-space characters from the current string.
			var _uid = line.cells[column].Trim();

			// I have values but not UID => use UID of line above
			if (string.IsNullOrEmpty(_uid)) return true;

			return _uid == blockUid;
		}

		/// <summary>
		/// check if next line is part of the same dialog
		/// either uid of next line is the same
		/// or empty uid field BUT as values in the line => not an empty line
		/// empty line = end of block
		/// </summary>
		bool isNextLinePartOfBlock(string currentLineUid, int uidColumn, int nextLineIndex)
		{
			if (nextLineIndex >= lines.Count)
				return false;

			var line = lines[nextLineIndex];

			// empty line is end of block
			if (line.cells.Count < 2) return false;

			// https://learn.microsoft.com/en-us/dotnet/api/system.string.trim?view=net-9.0
			// Removes all leading and trailing white-space characters from the current string.
			var _uid = line.cells[uidColumn].Trim();

			// has UID ? is it the same ?
			if (!string.IsNullOrEmpty(_uid))
			{
				//Debug.Log(_uid + " vs " + currentLineUid);
				return _uid == currentLineUid.Trim();
			}

			// auto fill
			return true;
		}

		/// <summary>
		/// crawl a string line
		/// catch when value is between quotes ""
		/// separate when catch ","
		/// </summary>
		string[] cellsSeparator(string lineRaw)
		{

			List<string> cells = new List<string>();

			StringBuilder sb = new StringBuilder();

			bool inValue = false;

			//browse the string
			for (int i = 0; i < lineRaw.Length; i++)
			{
				char cur = lineRaw[i];

				if (cur == ParserStatics.SPREAD_CELL_ESCAPE_VALUE) // "
				{
					inValue = !inValue;
				}

				if (cur == ParserStatics.SPREAD_CELL_SEPARATOR && !inValue) // ,
				{
					cells.Add(sb.ToString());
					sb.Clear();

					//Debug.Log("added value : " + sb);
				}
				else
				{
					//don't add separator "," symbol
					sb.Append(cur);
				}
			}

			cells.Add(sb.ToString());

			return cells.ToArray();
		}

		public string getCleanedCsvText()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < lines.Count; i++)
			{
				sb.AppendLine(lines[i].stringify());
			}

			return sb.ToString();
		}

		string presetupRaw(string raw)
		{

			//search & replace all LINE breaks
			// a line break char OUTSIDE of ""

			StringBuilder sb = new StringBuilder(raw);

			bool inCellValue = false;
			for (int i = 0; i < sb.Length; i++)
			{
				char cur = sb[i];

				if (cur == '"') inCellValue = !inCellValue;

				if (isCharLineBreak(cur.ToString()))
				{
					cur = inCellValue ? ParserStatics.CELL_LINE_BREAK : ParserStatics.SPREAD_LINE_BREAK;
				}

				sb[i] = cur;
			}

			//quand on tombe sur \r puis \n ça fait @@
			sb.Replace("@@", "@");
			sb.Replace("||", "|");

			raw = sb.ToString();

			return raw;
		}

		static bool isCharLineBreak(string cur)
		{
			if (cur == System.Environment.NewLine) return true;
			if (cur == "\r\n") return true;
			if (cur == "\r") return true;
			if (cur == "\n") return true;
			return false;
		}

		static CsvParser[] _parsers;

		static public void refreshCache()
		{
			//Debug.Log("refresh CSV cache");

			string[] csvPaths = Directory.GetFiles(
				LocalizationPaths.sysImports, "*." + CsvSerializer.parserExt);

			if (csvPaths.Length <= 0)
			{
				Debug.LogWarning("no csvs found @ " + LocalizationPaths.sysImports);
				return;
			}

			List<CsvParser> output = new List<CsvParser>();
			for (int i = 0; i < csvPaths.Length; i++)
			{
				var csv = CsvSerializer.load(csvPaths[i]);
				output.Add(csv);
			}

			_parsers = output.ToArray();
		}

		static public CsvParser[] loadParsers()
		{
			if (_parsers == null) refreshCache();
			return _parsers;
		}

		static public CsvParser getParser(DataSheetTab tab)
		{
			var ps = loadParsers();
			foreach (var p in ps)
			{
				if (p.tabUid == tab.tabUrlId)
				{
					return p;
				}
			}
			return null;
		}

	}

}
