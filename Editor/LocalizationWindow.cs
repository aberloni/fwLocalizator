using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator
{
    using fwp.localizator.editor;

    /// <summary>
    /// base for a window editor dedicated to localizator
    /// </summary>
    abstract public class LocalizationWindow<Manager> : EditorWindow where Manager : LocalizationManager
    {
        GUIStyle foldHeaderTitle;
        GUIStyle foldTitle;
        GUILayoutOption btnW;

        /// <summary>
        /// in usage context
        /// return override localiz manager
        /// </summary>
        public Manager getManager() => LocalizationManager.instance as Manager;

        void checkStyles()
        {
            btnW = GUILayout.MaxWidth(150f);
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

            if(foldTitle != null)
            {
                //foldTitle.richText = true;
                foldTitle.fontSize = 20;
            }
        }

        private void OnEnable()
        {
            //checkStyles();
        }

        private void OnFocus()
        {
            if (LocalizationManager.instance == null)
                LocalizationManager.instance = System.Activator.CreateInstance<Manager>();

            //checkStyles();
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

        virtual protected void draw(Manager mgr)
        {
            GUILayout.Label(mgr.GetType().ToString());
            GUILayout.Label("active lang " + mgr.getSavedIsoLanguage().ToString().ToUpper());

            GUILayout.BeginHorizontal();
            var sups = mgr.getSupportedLanguages();
            foreach(var s in sups)
            {
                if(GUILayout.Button(s.ToString()))
                {
                    mgr.setSavedLanguage(s, true);
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("download & generate", GUILayout.Height(30f)))
            {
                var sheets = mgr.getSheets();
                ExportLocalisationToGoogleForm.ssheets_import(sheets);
                ExportLocalisationToGoogleForm.trad_files_generation();
            }

            drawSheetSection(mgr);
            drawLangFiles(mgr);
        }

        bool foldDownload;

        void drawSheetSection(Manager mgr)
        {
            var sheets = mgr.getSheets();

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            foldDownload = EditorGUILayout.BeginFoldoutHeaderGroup(foldDownload, "sheets x" + sheets.Length, foldHeaderTitle);
            if (EditorGUI.EndChangeCheck())
            {
                if (foldDownload)
                {
                    sheets = mgr.getSheets(true);
                }
            }

            if (foldDownload)
            {
                foreach (var sheet in sheets)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("URL    " + sheet.sheetUrlUid);

                    if (GUILayout.Button("browse", btnW)) OpenInFileBrowser.browseUrl(sheet.url);

                    if (GUILayout.Button("download SHEET", btnW))
                    {
                        string[] outputs = ExportLocalisationToGoogleForm.ssheet_import(sheet);
                        for (int i = 0; i < sheet.sheetTabsIds.Length; i++)
                        {
                            var tab = sheet.sheetTabsIds[i];
                            tab.cache = outputs[i];
                            sheet.sheetTabsIds[i] = tab;
                        }
                    }

                    GUILayout.EndHorizontal();

                    foreach (var tab in sheet.sheetTabsIds)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("TAB    " + tab.fieldId + "#" + tab.tabId);

                        if (GUILayout.Button("browse", btnW))
                        {
                            OpenInFileBrowser.browseUrl(sheet.url + tab.url);
                        }

                        if (GUILayout.Button("download TAB", btnW))
                        {
                            ExportLocalisationToGoogleForm.tab_import(sheet, tab);
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (!string.IsNullOrEmpty(tab.cache))
                        {
                            GUILayout.Label(tab.cache);

                            if (GUILayout.Button(" > ", btnW))
                                UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath(tab.cacheAsset, typeof(TextAsset));
                        }
                        GUILayout.EndHorizontal();
                    }
                }

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        bool foldLang;
        Vector2 scrollLang;

        void drawLangFiles(Manager mgr)
        {
            GUILayout.Space(10f);

            var langs = mgr.lang_files;

            EditorGUI.BeginChangeCheck();
            foldLang = EditorGUILayout.BeginFoldoutHeaderGroup(foldLang, "langs x" + langs.Length, foldHeaderTitle);
            if (EditorGUI.EndChangeCheck())
            {
                if (foldLang)
                {
                }
                else
                {
                    scrollLang = Vector2.zero;
                    foreach (var l in langs) l.editor_fold = false;
                }
            }

            if (foldLang)
            {
                if (GUILayout.Button("generate trad files"))
                {
                    ExportLocalisationToGoogleForm.trad_files_generation();
                }

                foreach (var l in langs)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(l.iso.ToString());
                    GUILayout.Label("char x"+l.textAsset.text.Length, btnW);

                    if (GUILayout.Button("generate", btnW))
                    {
                        LocalizationFile file = mgr.getFileByLang(l.iso);
                        ExportLocalisationToGoogleForm.trad_file_generate(l.iso);
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

        /// <summary>
        /// draw a label with speficic style
        /// </summary>
        static public void drawSectionTitle(string label, float spaceMargin = 20f, int leftMargin = 10)
        {
            if (spaceMargin > 0f)
                GUILayout.Space(spaceMargin);

            GUILayout.Label(label, getSectionTitle(15, TextAnchor.UpperLeft, leftMargin));
        }

        static private GUIStyle gSectionTitle;
        static public GUIStyle getSectionTitle(int size = 15, TextAnchor anchor = TextAnchor.MiddleCenter, int leftMargin = 10)
        {
            if (gSectionTitle == null)
            {
                gSectionTitle = new GUIStyle();

                gSectionTitle.richText = true;
                gSectionTitle.alignment = anchor;
                gSectionTitle.normal.textColor = Color.white;

                gSectionTitle.fontStyle = FontStyle.Bold;
                gSectionTitle.margin = new RectOffset(leftMargin, 10, 10, 10);
                //gWinTitle.padding = new RectOffset(30, 30, 30, 30);

            }

            gSectionTitle.fontSize = size;

            return gSectionTitle;
        }
    }

}
