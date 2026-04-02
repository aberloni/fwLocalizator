using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.editor
{
	/// <summary>
	/// header => language info (selected, available)
	/// tabs
	/// 	=> sheets
	/// 	=> languages files content
	/// </summary>
	abstract public class WinEdImporter : WinEdLocaScaffold
	{
		protected override string getWindowTitle() => typeof(SheetsMind).ToString();

		LocaDataSheet[] Sheets => LocalizatorUtilsEditor.getSheetsData();

		public override iLocaTab[] GenerateTabs() => new iLocaTab[] { new TabSheets(), new TabLanguages(), };

		protected override void onFocus()
		{
			base.onFocus();

			// force refresh usage of sheets scriptables
			LocalizatorUtilsEditor.getSheetsData(true);
		}

		protected override void refresh(bool hard)
		{
			base.refresh(hard);
			LocalizatorUtilsEditor.getSheetsData(true);

			if (LocalizationMind.Verbose)
			{
				LocalizationMind.log("sheets x" + Sheets.Length);
				foreach (var s in Sheets) LocalizationMind.log(s.name + " tabs x" + s.tabs.Length);
			}
		}

		protected override void drawHeader()
		{
			base.drawHeader();
			drawLangSelector(LocalizationMind.Languages.getSupportedLanguages());
		}

		void drawLangSelector(IsoLanguages[] supported)
		{
			if(LocalizationMind.Verbose)
			{
				GUILayout.Label(LocalizationMind.Languages.stringifySupported);
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("SYS:" + Application.systemLanguage + " (" + LanguagesMind.SysToIso(Application.systemLanguage) + ")");

			if (LocalizationMind.Languages != null)
			{
				GUILayout.Label("SYS(filtered): " + LocalizationMind.Languages.getFilteredSystemLanguage());
				GUILayout.Label("USER: " + LocalizationMind.Languages.getIso());
			}
			GUILayout.EndHorizontal();

			if (LocalizationMind.Languages != null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("swap language");
				foreach (var s in supported)
				{
					if (GUILayout.Button(s.ToString()))
					{
						LocalizationMind.Languages.setIso(s, true);
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(10f);
		}

	}

}
