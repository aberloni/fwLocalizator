using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.localizator
{
	using fwp.localizator.editor;
	using fwp.localizator.dialog;

	/// <summary>
	/// base for a window editor dedicated to localizator
	/// </summary>
	abstract public class LocalizationWindow<Manager, LineData> : EditorWindow
		where Manager : LocalizationManager // extended Manager
		where LineData : LocaDialogLineData // extended base LineData
	{

		const string button_browse = "open URL";

		WinHelpFilter filter = new();

		GUIStyle sectionTitle;
		GUIStyle foldHeaderTitle;
		GUIStyle foldTitle;
		GUILayoutOption btnSW = GUILayout.MaxWidth(70f);
		GUILayoutOption btnW = GUILayout.MaxWidth(150f);
		GUILayoutOption btnH = GUILayout.Height(30f);

		Dictionary<string, bool> edFoldout = new Dictionary<string, bool>();

		/// <summary>
		/// in usage context
		/// return override localiz manager
		/// </summary>
		public Manager getManager() => LocalizationManager.instance as Manager;

		LocaDataSheet[] Sheets => LocalizatorUtils.getSheetsData();

		void checkStyles(bool force = false)
		{

			if (sectionTitle == null || force)
			{
				sectionTitle = new GUIStyle();
				//sectionTitle.normal.textColor = Color.gray;
				//sectionTitle.richText = true;
				sectionTitle.fontStyle = FontStyle.Bold;
			}

			if (foldHeaderTitle == null || force)
			{
				foldHeaderTitle = new GUIStyle(EditorStyles.foldoutHeader);
				foldHeaderTitle.fontStyle = FontStyle.Bold;

				foldHeaderTitle.normal.textColor = Color.white;

				foldHeaderTitle.onFocused.textColor = Color.gray;
				foldHeaderTitle.focused.textColor = Color.gray;

				//foldTitle.onActive.textColor = Color.red;
				//foldTitle.active.textColor = Color.red;

				foldHeaderTitle.fontSize = 20;
				//foldHeaderTitle.richText = true;
				//foldHeaderTitle.alignment = TextAnchor.MiddleCenter;

				//foldTitle.padding = new RectOffset(0, 0, 100, 100);
				//foldHeaderTitle.margin = new RectOffset(20,0,0,0);
			}

			if (foldTitle == null || force)
			{
				foldTitle = new GUIStyle(EditorStyles.foldout);

				//foldTitle.richText = true;
				foldTitle.fontSize = 20;
			}
		}

		string[] tabs = new string[] { "localization", "dialogs" };
		int selectedTab = 0;

		DialogManager<LineData> mgrDialog;

		private void OnEnable()
		{
			//checkStyles(true);
		}

		private void OnFocus()
		{
			Type t;

			if (LocalizationManager.instance == null)
			{
				t = typeof(Manager);
				if (!t.IsAbstract)
				{
					LocalizationManager.instance = System.Activator.CreateInstance<Manager>();
				}
			}

			if (mgrDialog == null)
			{
				t = typeof(LineData);

				if (!t.IsAbstract)
				{
					mgrDialog = DialogManager<LineData>.instance;
					if (mgrDialog == null)
					{
						mgrDialog = System.Activator.CreateInstance<DialogManager<LineData>>();
					}
				}
			}

			// force refresh usage of sheets scriptables
			LocalizatorUtils.getSheetsData(true);

			mgrDialog?.refresh();
		}

		private void OnGUI()
		{
			checkStyles();

			Manager mgr = getManager();
			if (mgr == null)
			{
				GUILayout.Label("no manager <" + typeof(Manager) + "> ?");
				return;
			}

			LocalizationWindowUtils.drawSectionTitle(mgr.GetType().ToString());

			draw(mgr);
		}

		/// <summary>
		/// generate the instance
		/// </summary>
		abstract protected LocaDialogData<LineData> createDialogInstance(string nm);

		virtual protected void draw(Manager mgr)
		{
			LocalizationManager.verbose = EditorGUILayout.Toggle("verbose", LocalizationManager.verbose);

			drawLangSelector(mgr);

			GUILayout.Space(20f);

			if (mgrDialog == null)
				return;

			int _selectedTab = GUILayout.Toolbar((int)selectedTab, tabs, "LargeButton");
			if (_selectedTab != selectedTab)
			{
				selectedTab = _selectedTab;

				if (mgrDialog != null) mgrDialog.refresh();
			}

			switch (selectedTab)
			{
				case 0: // localization
					drawTabLocalization(mgr);
					break;
				case 1: // dialogs
					drawTabDialogs();
					break;
			}
		}

		Vector2 scrollTabDialogs;
		void drawTabDialogs()
		{
			if (mgrDialog == null)
			{
				GUILayout.Label("no dialog manager ?");
				return;
			}
			filter.drawFilterField();

			scrollTabDialogs = GUILayout.BeginScrollView(scrollTabDialogs);

			drawFoldLocalizationFiles();
			drawFoldScriptableFiles();

			GUILayout.EndScrollView();
		}

		void drawFoldScriptableFiles()
		{
			if (mgrDialog == null)
				return;

			var dialogs = mgrDialog.dialogs;

			if (dialogs == null)
				return;

			//GUILayout.Label("in :   scriptables x" + dialogs.Length, LocalizationWindowUtils.getSectionTitle());
			bool unfold = drawFoldout("in :   scriptables x" + dialogs.Length, "scriptables", true);
			if (!unfold) return;

			foreach (var d in dialogs)
			{
				if (d == null) continue;
				if (!filter.MatchFilter(d.name)) continue;

				bool dUnfold = drawFoldout("dialog#" + d.name, d.name);
				//d.winEdFold = EditorGUILayout.Foldout(d.winEdFold, "dialog#" + d.name, true);
				if (dUnfold)
				{
					if (d.lines == null) GUILayout.Label("null lines[]");
					else
					{
						foreach (var line in d.lines)
						{
							GUILayout.Label(line.stringify());
						}
					}
					
					if (GUILayout.Button(">", btnSW))
					{
						UnityEditor.Selection.activeObject = d;
					}

				}
			}
		}

		void drawFoldLocalizationFiles()
		{
			//GUILayout.Label("in :   loca files x" + mgrDialog.dialogsUids.Length, LocalizationWindowUtils.getSectionTitle());
			if (mgrDialog == null)
				return;

			bool unfold = drawFoldout("in :   loca dialogs UIDs x" + mgrDialog.dialogsUids.Length, "loca", true);
			if (!unfold) return;

			if (GUILayout.Button("generate all missing dialogs"))
			{
				foreach (var d in mgrDialog.dialogsUids)
				{
					var dial = mgrDialog.getDialogInstance(d);
					if (dial == null) createDialog(d);
				}
			}

			foreach (var d in mgrDialog.dialogsUids)
			{
				if (d == null) continue;
				if (!filter.MatchFilter(d)) continue;

				GUILayout.BeginHorizontal();
				GUILayout.Label(d);
				var dial = mgrDialog.getDialogInstance(d);

				if (dial == null)
				{
					if (GUILayout.Button("create", btnW))
						createDialog(d);
				}
				else
				{
					if (GUILayout.Button("update", btnW))
					{
						dial.solveContent();
						EditorUtility.SetDirty(dial);

						UnityEditor.Selection.activeObject = dial;
					}
					if (GUILayout.Button(" > ", btnSW))
					{
						UnityEditor.Selection.activeObject = dial;
					}
				}

				GUILayout.EndHorizontal();
			}

		}

		bool drawFoldout(string label, string uid, bool isSection = false)
		{
			bool foldState = false;
			if (edFoldout.ContainsKey(uid))
			{
				foldState = edFoldout[uid];
			}

			bool _state;

			if (isSection)
			{
				_state = EditorGUILayout.Foldout(foldState, label, true, LocalizationWindowUtils.getFoldoutSection(15));
			}
			else
			{
				_state = EditorGUILayout.Foldout(foldState, label, true);
			}

			if (_state != foldState)
			{
				if (!edFoldout.ContainsKey(uid)) edFoldout.Add(uid, false);
				edFoldout[uid] = _state;
			}

			return _state;
		}

		void createDialog(string uid)
		{

			var inst = createDialogInstance(uid);
			Debug.Assert(inst != null, "could not create scriptable dialog");

			//string path = DialogManager.sysDialogs;
			string path = DialogManager<LineData>.assetDialogs;

			// make sure folder exists
			path = generateExportPath(path);

			// add asset at end of path
			path += uid + ".asset";

			Debug.Log("asset path @ " + path);

			AssetDatabase.CreateAsset(inst, path);
			AssetDatabase.Refresh();

			Debug.Log("solving content of " + inst);

			inst.solveContent();
			EditorUtility.SetDirty(inst);

			mgrDialog.refresh();
			UnityEditor.Selection.activeObject = inst;
		}

		/// <summary>
		/// in : (Assets/) some/path (/)
		/// out : filled path
		/// </summary>
		string generateExportPath(string path)
		{
			const string asset_path = "Assets";

			if (!path.StartsWith(asset_path))
				path = System.IO.Path.Combine(asset_path, path);

			// remove last  /
			if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

			var split = path.Split("/");

			Debug.Log(path + " => " + split.Length);

			string progressivePath = asset_path + "/";
			for (int i = 1; i < split.Length; i++)
			{
				string tarPath = progressivePath + "/" + split[i];
				if (!AssetDatabase.IsValidFolder(tarPath)) // Assets/Data/Dialogs
				{
					Debug.LogWarning("creating : " + tarPath);
					var guid = AssetDatabase.CreateFolder(progressivePath, split[i]); // Assets/Data & Dialogs
					Debug.Log(guid);
				}
				else Debug.Log("OK : " + tarPath);


				progressivePath = tarPath;
			}

			return path + "/";
		}

		Vector2 scrollTabLocaliz;
		void drawTabLocalization(Manager mgr)
		{
			GUILayout.Label("spreadsheet params", LocalizationWindowUtils.getSectionTitle());

			if (GUILayout.Button("process all (download > parse > trads)", GUILayout.Height(30f)))
			{
				var sheets = LocalizatorUtils.getSheetsData(true);
				ImportSheetUtils.ssheets_import(sheets);
				GenerateSheetUtils.csvs_generate(sheets);
				GenerateSheetUtils.trads_generate();
			}

			scrollTabLocaliz = GUILayout.BeginScrollView(scrollTabLocaliz);

			drawFoldSheetSection(mgr);
			drawFoldLangFiles(mgr);

			GUILayout.EndScrollView();
		}

		void drawLangSelector(Manager mgr)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("APP (unity)   : " + Application.systemLanguage + " (iso : " + LocalizationManager.sysToIso(Application.systemLanguage) + ")");
			GUILayout.Label("BUILD (#if)   : " + LocalizationManager.getSystemLanguageToIso());
			GUILayout.Label("USER (ppref)  : " + mgr.getSavedIsoLanguage());
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("swap language (ppref)");
			var sups = mgr.getSupportedLanguages();
			foreach (var s in sups)
			{
				if (GUILayout.Button(s.ToString()))
				{
					mgr.setSavedLanguage(s, true);
					mgrDialog?.refresh();
				}
			}
			GUILayout.EndHorizontal();
		}

		bool foldDownload;
		void drawFoldSheetSection(LocalizationManager mgr)
		{
			var sheets = LocalizatorUtils.getSheetsData();

			GUILayout.Space(10f);

			EditorGUI.BeginChangeCheck();
			foldDownload = EditorGUILayout.BeginFoldoutHeaderGroup(foldDownload, "sheets x" + sheets.Length, foldHeaderTitle);

			if (EditorGUI.EndChangeCheck()) // unfold
			{
				if (foldDownload)
				{
					sheets = LocalizatorUtils.getSheetsData(true);
				}
			}

			if (foldDownload)
			{
				foreach (var sheet in sheets)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("URL : " + sheet.sheetUrlUid);
					if (GUILayout.Button(button_browse, btnW)) OpenInFileBrowser.browseUrl(sheet.url);
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

							if (GUILayout.Button(button_browse, btnSW))
							{
								OpenInFileBrowser.browseUrl(sheet.url + tab.Url);
							}

							if (GUILayout.Button("download", btnW))
							{
								// import tab
								ImportSheetUtils.tab_import(sheet, tab);

								// make sure csv are up to date
								GenerateSheetUtils.csv_file_generate(tab);
							}

							if (!string.IsNullOrEmpty(tab.Cache))
							{
								GUILayout.Label("cache");

								if (GUILayout.Button("txt", btnSW))
								{
									Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheTxt, typeof(TextAsset));
								}

								if (GUILayout.Button("csv", btnSW))
								{
									Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheCsv, typeof(UnityEngine.Object));
								}

								if (GUILayout.Button("log raw", btnSW))
								{
									var p = CsvParser.getParser(tab);
									p?.logRaw();
								}
								if (GUILayout.Button("log loca", btnSW))
								{
									var p = CsvParser.getParser(tab);
									p?.logLocalized(mgr.getSavedIsoLanguage());
								}
							}
						}
					}
				}

			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		bool foldLang;
		void drawFoldLangFiles(Manager mgr)
		{
			GUILayout.Space(10f);

			var langs = mgr.lang_files;

			EditorGUI.BeginChangeCheck();
			foldLang = EditorGUILayout.BeginFoldoutHeaderGroup(foldLang, "langs files x" + langs.Length, foldHeaderTitle);
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
					mgr.reloadFiles();
					GenerateSheetUtils.trads_generate();
				}

				foreach (var l in langs)
				{
					GUILayout.BeginHorizontal();

					GUILayout.Label(l.iso.ToString());
					GUILayout.Label("char x" + l.textAsset.text.Length, btnW);

					if (GUILayout.Button("update", btnSW))
					{
						//var sheet = mgr.getSheets()[0];
						//LocalizationFile file = mgr.getFileByLang(l.iso);
						GenerateSheetUtils.trad_file_generate(l.iso);
					}

					if (GUILayout.Button(" > ", btnSW))
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
