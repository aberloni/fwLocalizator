using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fwp.localizator
{

	[System.Serializable]
	public class CsvLineRaw
	{
		public string raw = string.Empty;

		public List<string> cells = new();

		public CsvLineRaw(string raw)
		{
			this.raw = raw;
		}

		public bool hasAnyLocalization(int uidCol)
		{
			for (int i = uidCol + 1; i < cells.Count; i++)
			{
				if (cells[i].Length > 0) return true;
			}
			return false;
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

		public bool hasLocalization(IsoLanguages lang)
		{
			if (localized.Count <= (int)lang) return false;
			return !string.IsNullOrEmpty(localized[(int)lang]);
		}

		public CsvLineLang(string key)
		{
			this.key = key;
			Debug.Assert(!string.IsNullOrEmpty(key), "no key given ?");
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


}