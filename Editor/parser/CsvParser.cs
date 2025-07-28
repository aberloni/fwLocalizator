using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.editor
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
	public class CsvParser
	{
		/// <summary>
		/// parent tab
		/// assigned during generation
		/// </summary>
		public DataSheetTab tab;

		/// <summary>
		/// [tabName]_[ssheetGUID]
		/// </summary>
		public string ParserFileName => tab.TxtFileName;

		//public DataSheetTab Tab => LocalizatorUtilsEditor.tab_fetch(tabUid);

		/// <summary>
		/// contains header of column
		/// to find column of each lang
		/// </summary>
		public CsvLineRaw Header => lines[0];

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
			Debug.Log("parser.raw	<b>" + tab.DisplayName + "</b>	lines x" + lines.Count);

			for (int i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				Debug.Log("	#" + i + " cells x" + line.cells.Count);
				foreach (var c in line.cells) Debug.Log("		> " + c);
			}
		}

		public void logLocalized(IsoLanguages iso)
		{
			Debug.Log("parser.loca	<b>" + tab.DisplayName + "</b>	iso:" + iso + " x" + localizes.Count);
			foreach (var l in localizes)
			{
				if (!l.hasLocalization(iso)) Debug.Log(l.key + "=<color=red>[missing " + iso + "]</color>");
				else Debug.Log(l.key + "	= " + l.localized[(int)iso]);
			}
		}

		public void logMissing(IsoLanguages iso)
		{
			Debug.Log("parser.loca.missing	<b>" + tab.DisplayName + "</b>	iso:" + iso + " x" + localizes.Count);
			foreach (var l in localizes)
			{
				if (!l.hasLocalization(iso)) Debug.Log(l.key + "=<color=red>[missing " + iso + "]</color>");
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
			string[] langs = Header.cells.ToArray(); // each lang of cur line

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
			LocalizationPaths.sysImports + ParserFileName + CsvSerializer.parserExtDot);

		void generateFromPath(DataSheetTab tab, string path)
		{
			string raw = File.ReadAllText(path);

			this.tab = tab;

			// some cleaning
			raw = presetupRaw(raw);

			// this will split lines endings & remove empty lines
			string[] rawLines = raw.Split(new char[] { ParserStatics.SPREAD_LINE_BREAK }, System.StringSplitOptions.RemoveEmptyEntries);

			int originalCount = rawLines.Length;

			// line just under header of langs
			int firstContentLine = tab.tabParams.langLineIndex;
			for (int i = firstContentLine; i < rawLines.Length; i++)
			{
				string lineStr = rawLines[i];

				// don't keep empty lines (if any)
				if (string.IsNullOrEmpty(lineStr)) continue;

				string[] split = cellsSeparator(lineStr);

				// it's spreadsheet cells
				// need to add event the empty ones to keep index coherent
				CsvLineRaw line = new CsvLineRaw(lineStr);
				line.cells.AddRange(split);

				if (line.hasAnyLocalization((int)tab.tabParams.uidColumn))
				{
					lines.Add(line);
					//Debug.Log("+	" + lineStr);
				}
				//else Debug.LogWarning("skipped : " + lineStr);

			}

		}

		public void generateLocalization()
		{
			Debug.Log("		generate <b>localization</b> out of lines x" + lines.Count);

			int _col = (int)tab.tabParams.uidColumn;

			foreach (var line in lines)
			{
				Debug.Assert(line != null);
				string key = line.cells[_col];

				if (string.IsNullOrEmpty(key))
				{
					Debug.LogWarning($"{tab.tabName} :	a key is null");
					Debug.LogWarning("line is : " + line.raw);
					continue;
				}

				CsvLineLang llang = new(key);
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
		/// append 01,02,03 to keys
		/// </summary>
		public void fillAutoUid(int uidColumn)
		{
			Debug.Log("		autofill lines x" + lines.Count);
			string _activeUid = string.Empty;
			int cnt = 0;

			// search for first valid UID line
			/*
			int startIndex = 1;
			while (_activeUid.Length <= 0)
			{
				var lineFields = lines[startIndex].cells;
				if (lineFields.Count < 2) continue;
				var uid = lineFields[uidColumn].Trim();
				if (!string.IsNullOrEmpty(uid)) _activeUid = uid;
				else startIndex++;
			}
			*/

			//first line is languages header
			for (int j = 0; j < lines.Count; j++)
			{
				Debug.Assert(!isEmptyLine(j), "autofill an empty line ?");

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

				Debug.Assert(!string.IsNullOrEmpty(key), "no key to inject ?");

				lines[j].cells[uidColumn] = key;

				Debug.Log("  <b>autofilled</b> key : " + key);
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

			// parse the string
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
					// append current character to cell value
					sb.Append(cur);
				}
			}

			// append last active
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

		/// <summary>
		/// key=value
		/// key=value
		/// </summary>
		public string getLangFileContent(IsoLanguages lang)
		{
			StringBuilder output = new StringBuilder();

			// first is header
			for (int i = 1; i < localizes.Count; i++)
			{
				var line = localizes[i];
				if (line.hasLocalization(lang))
				{
					string val = line.localized[(int)lang];
					val = sanitizeValue(val);
					output.AppendLine(line.key + "=" + val);
				}
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

			List<CsvParser> ret = new();

			foreach (var parser in ps)
			{
				if (parser.tab.compare(tab))
				{
					ret.Add(parser);
				}
			}

			if (ret.Count > 1)
			{
				Debug.LogWarning("found multiple instance of tab" + tab.tabUrlId);
				return null;
			}

			if (ret.Count <= 0)
			{
				Debug.LogWarning("missing parser : " + tab.tabUrlId);
				return null;
			}

			return ret[0];
		}

		static public string getCellValue(string lineUid, int cell)
		{
			var csvs = CsvParser.loadParsers();

			foreach (var csv in csvs)
			{
				// search for line
				foreach (var l in csv.lines)
				{
					// search for cell with uid
					foreach (var val in l.cells)
					{
						if (val.Contains(lineUid))
						{
							if (LocalizationManager.verbose)
							{
								Debug.Log("found " + lineUid + " cell in CSV:" + csv.tab.DisplayName + " => returning column #" + cell);
							}

							return l.cells[cell];
						}
					}
				}
			}

			if (LocalizationManager.verbose)
			{
				Debug.LogWarning("could not find a cell value for uid : " + lineUid);
			}

			return string.Empty;
		}

	}

}
