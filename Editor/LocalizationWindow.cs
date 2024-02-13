using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator
{
    using fwp.localizator.editor;
    using fwp.localizator.dialog;

    /// <summary>
    /// base for a window editor dedicated to localizator
    /// </summary>
    abstract public class LocalizationWindow<Manager, LineData> : EditorWindow
        where Manager : LocalizationManager
        where LineData : LocaDialogLineData
    {

        GUIStyle foldHeaderTitle;
        GUIStyle foldTitle;
        GUILayoutOption btnSW = GUILayout.MaxWidth(70f);
        GUILayoutOption btnW = GUILayout.MaxWidth(150f);
        GUILayoutOption btnH = GUILayout.Height(30f);

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

        void checkStyles()
        {
            foldHeaderTitle = new GUIStyle(EditorStyles.foldoutHeader);
            foldTitle = new GUIStyle(EditorStyles.foldout);

            if (foldHeaderTitle != null)
            {
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

            if (foldTitle != null)
            {
                //foldTitle.richText = true;
                foldTitle.fontSize = 20;
            }
        }

        string[] tabs = new string[] { "localization", "dialogs" };
        int selectedTab = 0;

        DialogManager<LineData> mgrDialog;

        private void OnFocus()
        {
            if (LocalizationManager.instance == null)
                LocalizationManager.instance = System.Activator.CreateInstance<Manager>();

            if (mgrDialog == null)
            {
                mgrDialog = DialogManager<LineData>.instance;
                if (mgrDialog == null)
                {
                    mgrDialog = System.Activator.CreateInstance<DialogManager<LineData>>();
                }
            }
            else mgrDialog.refresh();
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

            draw(mgr);
        }

        /// <summary>
        /// generate the instance
        /// </summary>
        abstract protected LocaDialogData<LineData> createDialogInstance(string nm);

        virtual protected void draw(Manager mgr)
        {
            LocalizationWindowUtils.drawSectionTitle(mgr.GetType().ToString());

            LocalizationManager.verbose = EditorGUILayout.Toggle("verbose", LocalizationManager.verbose);

            drawLangSelector(mgr);

            GUILayout.Space(20f);

            selectedTab = GUILayout.Toolbar((int)selectedTab, tabs, "LargeButton");
            switch (selectedTab)
            {
                case 0:
                    drawLocalization(mgr);
                    break;
                case 1:
                    if (DialogManager<LineData>.instance == null) GUILayout.Label("no dialog manager");
                    else
                    {
                        drawDialogs();
                    }
                    break;
            }
        }

        Vector2 scrollDialsContent;
        Vector2 scrollDialsScriptables;
        void drawDialogs()
        {
            if (mgrDialog == null) return;

            GUILayout.Label("in :   loca files x" + mgrDialog.dialogsUids.Length,
                LocalizationWindowUtils.getSectionTitle());

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

            var dialogs = mgrDialog.dialogs;

            GUILayout.Label("in :   scriptables x" + dialogs.Length, LocalizationWindowUtils.getSectionTitle());

            scrollDialsScriptables = GUILayout.BeginScrollView(scrollDialsScriptables);

            foreach (var d in dialogs)
            {
                if (d == null)
                    continue;

                d.winEdFold = EditorGUILayout.Foldout(d.winEdFold, "dialog#" + d.name, true);
                if (d.winEdFold)
                {
                    if (d.lines == null) GUILayout.Label("null lines[]");
                    else
                    {
                        foreach (var line in d.lines)
                        {
                            GUILayout.Label("       " + line.previews[(int)IsoLanguages.fr]);
                        }
                    }
                }

            }

            GUILayout.EndScrollView();
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

            if (GUILayout.Button("download & generate", GUILayout.Height(30f)))
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

            GUILayout.Label("active lang " + mgr.getSavedIsoLanguage().ToString().ToUpper());

            GUILayout.BeginHorizontal();
            var sups = mgr.getSupportedLanguages();
            foreach (var s in sups)
            {
                if (GUILayout.Button(s.ToString()))
                {
                    mgr.setSavedLanguage(s, true);
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

                    GUILayout.Label("URL    " + sheet.sheetUrlUid);

                    if (GUILayout.Button("browse", btnW)) OpenInFileBrowser.browseUrl(sheet.url);

                    if (GUILayout.Button("download TABS", btnW))
                    {
                        string[] outputs = ImportSheetUtils.ssheet_import(sheet);

                        // update all tabs cache paths
                        for (int i = 0; i < sheet.tabs.Length; i++)
                        {
                            var tab = sheet.tabs[i];
                            tab.cache = outputs[i];
                            sheet.tabs[i] = tab;
                        }
                    }

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

                            if (GUILayout.Button(" > ", btnSW))
                                UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath(tab.cacheAsset, typeof(TextAsset));
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
