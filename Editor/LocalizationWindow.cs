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
        /*
        [MenuItem("Localization/viewer")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(LocalizationWindow));
        }
        */

        string output = string.Empty;

        /// <summary>
        /// in usage context
        /// return override localiz manager
        /// </summary>
        public Manager getManager() => LocalizationManager.instance as Manager;

        private void OnFocus()
        {
            if (LocalizationManager.instance == null)
                LocalizationManager.instance = System.Activator.CreateInstance<Manager>();
        }

        private void OnGUI()
        {
            Manager mgr = getManager();
            if (mgr == null)
            {
                GUILayout.Label("no manager <" + typeof(Manager) + "> ?");
                return;
            }

            GUILayout.Label(mgr.GetType().ToString());

            draw(mgr);
        }

        virtual protected void draw(Manager mgr)
        {
            drawSheetSection(mgr);

            if (GUILayout.Button("generate trad files"))
            {
                ExportLocalisationToGoogleForm.trad_files_generation();
            }

            if (GUILayout.Button("solve chinese"))
            {
                LocalizationFile file = mgr.getFileByLang(IsoLanguages.zh.ToString());

                output = string.Empty;

                var lines = file.getLines();
                foreach (var line in lines)
                {
                    output += line + "\n";
                }
            }

        }

        bool foldDownload;

        void drawSheetSection(Manager mgr)
        {
            var sheets = mgr.getSheets();

            EditorGUI.BeginChangeCheck();
            foldDownload = EditorGUILayout.Foldout(foldDownload, "sheets x" + sheets.Length);
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

                    if (GUILayout.Button("open")) openUrl(sheet.url);

                    if (GUILayout.Button("download SHEET"))
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

                        if (GUILayout.Button("open"))
                        {
                            openUrl(sheet.url + tab.url);
                        }

                        if (GUILayout.Button("download TAB"))
                        {
                            ExportLocalisationToGoogleForm.tab_import(sheet, tab);
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (!string.IsNullOrEmpty(tab.cache))
                        {
                            GUILayout.Label(tab.cache);

                            if (GUILayout.Button("select"))
                                UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath(tab.cacheAsset, typeof(TextAsset));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.TextArea(output);
        }

        static public void openUrl(string url)
        {

            Debug.Log(url);
            //System.Diagnostics.Process.Start("explorer.exe", url);
            System.Diagnostics.Process.Start(url);
        }
    }

}
