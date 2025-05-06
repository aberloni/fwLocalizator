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

        WinHelpFilter filter = new();

        GUIStyle sectionTitle;
        GUIStyle foldHeaderTitle;
        GUIStyle foldTitle;
        GUILayoutOption btnSW = GUILayout.MaxWidth(70f);
        GUILayoutOption btnW = GUILayout.MaxWidth(150f);
        GUILayoutOption btnH = GUILayout.Height(30f);

        Dictionary<string, bool> edFoldout = new Dictionary<string, bool>();

        LocalizationSheetParams sheetParams = new LocalizationSheetParams()
        {
            uidColumn = ColumnLetter.D,
            langLineIndex = 3
        };

        /// <summary>
        /// in usage context
        /// return override localiz manager
        /// </summary>
        public Manager getManager() => LocalizationManager.instance as Manager;

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
                case 0:
                    drawLocalization(mgr);
                    break;
                case 1:

                    if (mgrDialog == null) GUILayout.Label("no dialog manager ?");
                    else
                    {
						filter.drawFilterField();

						drawFoldLocalizationFiles();
                        drawFoldScriptableFiles();
                    }
                    break;
            }
        }

        Vector2 scrollDialsScriptables;
        void drawFoldScriptableFiles()
        {
            if (mgrDialog == null)
                return;

            var dialogs = mgrDialog.dialogs;

            if (dialogs == null)
                return;

            //GUILayout.Label("in :   scriptables x" + dialogs.Length, LocalizationWindowUtils.getSectionTitle());
            bool unfold = drawFoldout("in :   scriptables x" + dialogs.Length, "scriptables", true);

            //GUILayout.Label("in :   scriptables x" + dialogs.Length, LocalizationWindowUtils.getSectionTitle());

            if (unfold)
            {
                IsoLanguages iso = getManager().getSavedIsoLanguage();

                scrollDialsScriptables = GUILayout.BeginScrollView(scrollDialsScriptables);

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
                    }

                }

                GUILayout.EndScrollView();
            }

        }

        Vector2 scrollDialsContent;
        void drawFoldLocalizationFiles()
        {
            //GUILayout.Label("in :   loca files x" + mgrDialog.dialogsUids.Length, LocalizationWindowUtils.getSectionTitle());
            if (mgrDialog == null)
                return;

            bool unfold = drawFoldout("in :   loca dialogs UIDs x" + mgrDialog.dialogsUids.Length, "loca", true);

            if (unfold)
            {
                if (GUILayout.Button("generate all missing dialogs"))
                {
                    foreach (var d in mgrDialog.dialogsUids)
                    {
                        var dial = mgrDialog.getDialogInstance(d);
                        if (dial == null) createDialog(d);
                    }
                }

                scrollDialsContent = GUILayout.BeginScrollView(scrollDialsContent);

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

                GUILayout.EndScrollView();

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

        void drawLocalization(Manager mgr)
        {
            GUILayout.Label("spreadsheet params", LocalizationWindowUtils.getSectionTitle());

            GUILayout.BeginHorizontal();
            GUILayout.Label("uid @ " + sheetParams.uidColumn);
            GUILayout.Label("langs @ " + sheetParams.langLineIndex);
            GUILayout.EndHorizontal();

            GUILayout.Label("spreadsheet import", LocalizationWindowUtils.getSectionTitle());

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("download all", btnH))
            {
                var sheets = LocalizatorUtils.getSheetsData();
                ImportSheetUtils.ssheets_import(sheets);
            }
            if (GUILayout.Button("generate CSVs", btnH))
            {
                GenerateSheetUtils.csv_file_generate(sheetParams);
            }
            if (GUILayout.Button("generate trads", btnH))
            {
                GenerateSheetUtils.trad_files_generation(sheetParams);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("download > CSV > trads", GUILayout.Height(30f)))
            {
                var sheets = LocalizatorUtils.getSheetsData();
                ImportSheetUtils.ssheets_import(sheets);
                GenerateSheetUtils.csv_file_generate(sheetParams);
                GenerateSheetUtils.trad_files_generation(sheetParams);
            }

            drawSheetSection(mgr);
            drawLangFiles(mgr);
        }

        void drawLangSelector(Manager mgr)
        {
            GUILayout.Space(10f);
            GUILayout.Label("sys lang       : " + Application.systemLanguage + " (iso : " + LocalizationManager.sysToIso(Application.systemLanguage) + ")");
            GUILayout.Label("saved lang     : " + mgr.getSavedIsoLanguage());
            GUILayout.Label("build iso      : " + LocalizationManager.getSystemLanguageToIso());
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
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

        void drawSheetSection(Manager mgr)
        {
            var sheets = LocalizatorUtils.getSheetsData();

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            foldDownload = EditorGUILayout.BeginFoldoutHeaderGroup(foldDownload, "sheets x" + sheets.Length, foldHeaderTitle);
            if (EditorGUI.EndChangeCheck())
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

                    GUILayout.Label("URL", btnW);
                    GUILayout.Label(sheet.sheetUrlUid);

                    if (GUILayout.Button("browse", btnW)) OpenInFileBrowser.browseUrl(sheet.url);

                    GUILayout.EndHorizontal();

                    foreach (var tab in sheet.tabs)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("TAB    " + tab.tabName + "#" + tab.tabUrlId);

                        if (GUILayout.Button("browse", btnW))
                        {
                            OpenInFileBrowser.browseUrl(sheet.url + tab.url);
                        }

                        if (GUILayout.Button("download TAB", btnW))
                        {
                            // import tab
                            ImportSheetUtils.tab_import(sheet, tab);

                            // make sure csv are up to date
                            GenerateSheetUtils.csv_file_generate(sheetParams);
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (!string.IsNullOrEmpty(tab.cache))
                        {
                            GUILayout.Label(tab.cache);

                            if (GUILayout.Button("txt", btnSW))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.cacheTxt, typeof(TextAsset));
                            }

                            if (GUILayout.Button("csv", btnSW))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.cacheCsv, typeof(UnityEngine.Object));
                            }

                            if (GUILayout.Button("log", btnSW))
                            {
                                var parser = CsvParser.load("Assets/" + tab.cacheCsv);

                                foreach (var l in parser.lines)
                                {
                                    Debug.Log("parser line : " + l.stringify());
                                    foreach (var c in l.cell)
                                    {
                                        Debug.Log("    >> " + c);
                                    }
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                    }
                }

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        bool foldLang;
        void drawLangFiles(Manager mgr)
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
                    GenerateSheetUtils.trad_files_generation(sheetParams);
                }

                foreach (var l in langs)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(l.iso.ToString());
                    GUILayout.Label("char x" + l.textAsset.text.Length, btnW);

                    if (GUILayout.Button("update", btnW))
                    {
                        //var sheet = mgr.getSheets()[0];
                        //LocalizationFile file = mgr.getFileByLang(l.iso);
                        GenerateSheetUtils.trad_file_generate(l.iso, sheetParams);
                    }

                    if (GUILayout.Button(" > ", btnW))
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
