using UnityEngine;
using UnityEditor;

namespace fwp.localizator.editor
{

    public class TabSheets : iLocaTab
    {
        // https://www.alanwood.net/unicode/arrows.html
        const string symbol_download = "⬇"; // ⇓
        const string symbol_cloud = "☁︎";

        public void Refresh(bool hard)
        { }

        public string GetTabName() => "Sheets";
        public void Draw(LocalizationManager LManager)
        {

            GUILayout.Label("spreadsheet params", UtilStyles.SectionTitle());

            LocaDataSheet[] sheets;

            if (GUILayout.Button(symbol_download + " all (download > parse > trads)", GUILayout.Height(30f)))
            {
                sheets = LocalizatorUtilsEditor.getSheetsData(clearCache: true);
                ImportSheetUtils.ssheets_import(sheets);
                GenerateSheetUtils.csvs_generate(sheets);
                GenerateSheetUtils.trads_generate();
            }
            else
            {
                sheets = LocalizatorUtilsEditor.getSheetsData();
            }

            var iso = LManager.getSavedIsoLanguage();

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("sheets x" + sheets.Length);

            foreach (var sheet in sheets)
            {
                GUILayout.Space(5f);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("?", GuiHelper.btnXS)) UnityEditor.Selection.activeObject = sheet;

                EditorGUILayout.ObjectField(sheet, sheet.GetType(), true);

                if (GUILayout.Button(symbol_download + " tabs", GuiHelper.btnS))
                {
                    ImportSheetUtils.ssheet_import(sheet);
                    GenerateSheetUtils.csv_generate(sheet);
                    GenerateSheetUtils.trads_generate();
                }

                GUILayout.EndHorizontal();

                bool _fold = GuiHelper.DrawFoldout("Show all tabs", "tab" + sheet.sheetUrlUid);
                if (!_fold) continue;

                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(symbol_cloud, GuiHelper.btnXS)) OpenInFileBrowser.browseUrl(sheet.url);
                GUILayout.Label("URL : " + sheet.sheetUrlUid);
                GUILayout.EndHorizontal();

                foreach (var tab in sheet.tabs)
                {
                    using (new GUILayout.HorizontalScope())
                    {

                        if (GUILayout.Button(symbol_cloud, GuiHelper.btnXS))
                            OpenInFileBrowser.browseUrl(sheet.url + tab.Url);

                        GUILayout.Label(tab.TxtFileName + " (" + tab.parseType + ")");

                        if (GUILayout.Button(symbol_download, GuiHelper.btnXS))
                        {
                            // import tab
                            ImportSheetUtils.tab_import(sheet, tab);

                            // make sure csv are up to date
                            GenerateSheetUtils.csv_file_generate(tab);

                            // all remake all localiz files
                            GenerateSheetUtils.trads_generate();
                        }

                        if (!string.IsNullOrEmpty(tab.Cache))
                        {
                            if (GUILayout.Button("txt", GuiHelper.btnS))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheTxt, typeof(TextAsset));
                            }

                            if (GUILayout.Button("csv", GuiHelper.btnS))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + tab.CacheCsv, typeof(UnityEngine.Object));
                            }

                            if (GUILayout.Button("log:raw", GuiHelper.btnS))
                            {
                                CsvParser.getParser(tab)?.logRaw();
                            }
                            if (GUILayout.Button("log:loca", GuiHelper.btnS))
                            {
                                CsvParser.getParser(tab)?.logLocalized(iso);
                            }
                            if (GUILayout.Button("log.missing", GuiHelper.btnS))
                            {
                                CsvParser.getParser(tab)?.logMissing(iso);
                            }
                        }
                    }
                }
            }
        }

    }

}