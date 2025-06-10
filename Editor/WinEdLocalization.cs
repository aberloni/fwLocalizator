using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator.editor
{
	using fwp.localizator.dialog;

	public class WinEdLocalization : WinEdLocaScaffold
	{
		[MenuItem("Window/Localizator/(win) loca")]
		static void init() => EditorWindow.GetWindow(typeof(WinEdLocalization));

		const string button_browse = "open URL";

		LocaDataSheet[] Sheets => LocalizatorUtilsEditor.getSheetsData();

		public DialogManager mDialog => DialogManager.instance;

		protected override void generateManager() => new LocalizationManager();

		protected override void OnFocus()
		{
			base.OnFocus();

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

		protected override string getTitle() => LocalizationManager.instance.GetType().Name;

		override protected void draw()
		{
			base.draw();

			LocalizationManager.verbose = EditorGUILayout.Toggle("verbose", LocalizationManager.verbose);

			drawLangSelector(LocalizationManager.instance.getSupportedLanguages());

			GUILayout.Space(20f);

			drawTabLocalization();
		}

		Vector2 scrollTabLocaliz;
		void drawTabLocalization()
		{
			GUILayout.Label("spreadsheet params", UtilStyles.SectionTitle());

			if (GUILayout.Button("process all (download > parse > trads)", GUILayout.Height(30f)))
			{
				var sheets = LocalizatorUtilsEditor.getSheetsData(true);
				ImportSheetUtils.ssheets_import(sheets);
				GenerateSheetUtils.csvs_generate(sheets);
				GenerateSheetUtils.trads_generate();
			}

			scrollTabLocaliz = GUILayout.BeginScrollView(scrollTabLocaliz);

			drawFoldSheetSection();
			drawFoldLangFiles();

			GUILayout.EndScrollView();
		}

		void drawLangSelector(IsoLanguages[] supported)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("APP (unity)   : " + Application.systemLanguage + " (iso : " + LocalizationManager.sysToIso(Application.systemLanguage) + ")");
			if (LocalizationManager.instance != null)
			{
				GUILayout.Label("SYS           : " + LocalizationManager.instance.getSystemLanguage());
				GUILayout.Label("BUILD (#if)   : " + LocalizationManager.instance.getFilteredSystemLanguage());
				GUILayout.Label("USER (ppref)  : " + LocalizationManager.instance.getSavedIsoLanguage());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("swap language (ppref)");
			foreach (var s in supported)
			{
				if (GUILayout.Button(s.ToString()))
				{
					LocalizationManager.instance.setSavedLanguage(s, true);
				}
			}
			GUILayout.EndHorizontal();
		}

		bool foldDownload;
		void drawFoldSheetSection()
		{
			var sheets = LocalizatorUtilsEditor.getSheetsData();

			GUILayout.Space(10f);

			EditorGUI.BeginChangeCheck();
			foldDownload = EditorGUILayout.BeginFoldoutHeaderGroup(foldDownload,
				"sheets x" + sheets.Length, UtilStyles.FoldHeaderTitle());

			if (EditorGUI.EndChangeCheck()) // unfold
			{
				if (foldDownload)
				{
					sheets = LocalizatorUtilsEditor.getSheetsData(true);
				}
			}

			if (foldDownload)
			{
				var iso = LocalizationManager.instance.getSavedIsoLanguage();

				foreach (var sheet in sheets)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("?", btnXS)) UnityEditor.Selection.activeObject = sheet;
					EditorGUILayout.ObjectField(sheet, sheet.GetType(), true);
					GUILayout.Label("URL : " + sheet.sheetUrlUid);
					if (GUILayout.Button(button_browse, btnM)) OpenInFileBrowser.browseUrl(sheet.url);
					GUILayout.EndHorizontal();

					bool _fold = drawFoldout("Show all tabs", "tab" + sheet.sheetUrlUid);
					if (!_fold) continue;

					EditorGUI.indentLevel++;
					foreach (var tab in sheet.tabs)
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(20f);
							GUILayout.Label(tab.tabName + "#" + tab.tabUrlId + " (" + tab.parseType + ")");

							if (GUILayout.Button(button_browse, btnS))
							{
								OpenInFileBrowser.browseUrl(sheet.url + tab.Url);
							}

							if (GUILayout.Button("download", btnM))
							{
								// import tab
								ImportSheetUtils.tab_import(sheet, tab);

								// make sure csv are up to date
								GenerateSheetUtils.csv_file_generate(tab);
							}

							if (!string.IsNullOrEmpty(tab.Cache))
							{
								GUILayout.Label("cache");

								if (GUILayout.Button("txt", btnS))
								{
									Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheTxt, typeof(TextAsset));
								}

								if (GUILayout.Button("csv", btnS))
								{
									Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheCsv, typeof(UnityEngine.Object));
								}

								if (GUILayout.Button("log raw", btnS))
								{
									var p = CsvParser.getParser(tab);
									p?.logRaw();
								}
								if (GUILayout.Button("log loca", btnS))
								{
									var p = CsvParser.getParser(tab);
									p?.logLocalized(iso);
								}
							}
						}
					}
				}

			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		bool foldLang;
		void drawFoldLangFiles()
		{
			GUILayout.Space(10f);

			var langs = LocalizationManager.instance.lang_files;

			EditorGUI.BeginChangeCheck();
			foldLang = EditorGUILayout.BeginFoldoutHeaderGroup(foldLang,
				"langs files x" + langs.Length, UtilStyles.FoldHeaderTitle());

			if (EditorGUI.EndChangeCheck())
			{
				if (!foldLang)
				{
					foreach (var l in langs) l.editor_fold = false;
				}
			}

			if (foldLang)
			{
				if (GUILayout.Button("generate trad files"))
				{
					LocalizationManager.instance.reloadFiles();
					GenerateSheetUtils.trads_generate();
				}

				foreach (var l in langs)
				{
					GUILayout.BeginHorizontal();

					GUILayout.Label(l.iso.ToString());
					GUILayout.Label("char x" + l.textAsset.text.Length, btnM);

					if (GUILayout.Button("update", btnS))
					{
						//var sheet = mgr.getSheets()[0];
						//LocalizationFile file = mgr.getFileByLang(l.iso);
						GenerateSheetUtils.trad_file_generate(l.iso);
					}

					if (GUILayout.Button(" > ", btnS))
					{
						UnityEditor.Selection.activeObject = l.textAsset;
					}

					GUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

	}

}
