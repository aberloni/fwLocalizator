using UnityEngine;
using System.Collections.Generic;

namespace fwp.localizator
{

	/// <summary>
	/// loca files
	/// </summary>
	public class SheetsMind : LocalizationMind
	{

		public LocalizationFile[] lang_files;

		public SheetsMind()
		{
			LocalizatorMinds.Sheets = this;
			loadFiles();
		}

		public void reloadFiles()
		{
			loadFiles();
		}

		protected void loadFiles()
		{

			List<LocalizationFile> tmp = new List<LocalizationFile>();
			var sups = LocalizatorMinds.Languages.getSupportedLanguages();
			for (int i = 0; i < sups.Length; i++)
			{
				LocalizationFile file = new LocalizationFile(sups[i]);
				if (file != null && file.IsLoaded) tmp.Add(file);
			}
			lang_files = tmp.ToArray();

		}

#if UNITY_EDITOR
		protected void checkIntegrity()
		{
			for (int i = 0; i < lang_files.Length; i++)
			{
				for (int j = 0; j < lang_files.Length; j++)
				{
					if (lang_files[i].edCompareKeys(lang_files[j]))
					{
						Debug.LogError("Issue comparing " + lang_files[i].iso + " VS " + lang_files[j].iso);
					}
				}
			}
		}
#endif

		public LocalizationFile getFileByLang(IsoLanguages lang)
		{
			for (int i = 0; i < lang_files.Length; i++)
			{
				//debug, NOT runtime, to be sure content is updated
				if (!Application.isPlaying) lang_files[i].edRefresh();

				if (lang_files[i].iso == lang)
				{
					return lang_files[i];
				}
			}
			return null;
		}

		protected LocalizationFile getLangFileByLangLabel(string label)
		{
			for (int i = 0; i < lang_files.Length; i++)
			{
				if (lang_files[i].iso.ToString() == label) return lang_files[i];
			}
			Debug.LogWarning(" !!! <color=red>no file</color> for current lang : " + label);
			return null;
		}

		public LocalizationFile getCurrentLangFile()
		{
#if UNITY_EDITOR
			loadFiles();
#endif

			string lang = LocalizatorMinds.Languages.getLanguage().ToString();
			LocalizationFile file = getLangFileByLangLabel(lang);

			return file;
		}

	}

}