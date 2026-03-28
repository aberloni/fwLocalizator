using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator.editor
{
	using fwp.localizator.dialog;

	/// <summary>
	/// header => language info (selected, available)
	/// tabs
	/// 	=> sheets
	/// 	=> languages files content
	/// </summary>
	public class WinEdLocalization : WinEdLocaScaffold<LocalizationManager>
	{

		[MenuItem("Window/Localizator/+panel:localization")]
		static void init() => EditorWindow.GetWindow(typeof(WinEdLocalization));

		protected override string getWindowTitle() 
			=> LocalizationManager.Instance != null ? LocalizationManager.Instance.GetType().Name : "Localization";

		LocaDataSheet[] Sheets => LocalizatorUtilsEditor.getSheetsData();

		public DialogManager mDialog => DialogManager.instance;

		public override iLocaTab[] GenerateTabs() => new iLocaTab[]{ new TabSheets(), new TabLanguages(), };

        public override LocalizationManager GenerateManager() => new LocalizationManager();

		protected override void onFocus()
		{
			base.onFocus();

			// force refresh usage of sheets scriptables
			LocalizatorUtilsEditor.getSheetsData(true);
		}

		protected override void refresh(bool verbose)
		{
			base.refresh(verbose);
			LocalizatorUtilsEditor.getSheetsData(true);
			if (verbose)
			{
				Debug.Log("sheets x" + Sheets.Length);
				foreach (var s in Sheets) Debug.Log(s.name + " tabs x" + s.tabs.Length);
			}
		}

        protected override void drawHeader()
        {
            base.drawHeader();
			drawLangSelector(LManager.getSupportedLanguages());
        }

		void drawLangSelector(IsoLanguages[] supported)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("APP (unity)   : " + Application.systemLanguage + " (iso : " + LocalizationManager.sysToIso(Application.systemLanguage) + ")");
			if (LManager != null)
			{
				GUILayout.Label("SYS           : " + LManager.getSystemLanguage());
				GUILayout.Label("BUILD (#if)   : " + LManager.getFilteredSystemLanguage());
				GUILayout.Label("USER (ppref)  : " + LManager.getSavedIsoLanguage());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("swap language (ppref)");
			foreach (var s in supported)
			{
				if (GUILayout.Button(s.ToString()))
				{
					LManager.setSavedLanguage(s, true);
				}
			}
			GUILayout.EndHorizontal();

		}

	}

}
