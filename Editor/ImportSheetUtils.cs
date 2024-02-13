using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// entry points for import calls
/// </summary>
namespace fwp.localizator.editor
{
    using fwp.localizator;
    using System.IO;
    using UnityEditor;

    public class ImportSheetUtils
    {
        static public bool verbose => LocalizationManager.verbose;

        /// <summary>
        /// import and save multiple spreadsheets
        /// </summary>
        static public void ssheets_import(LocaDataSheet[] sheets)
        {
            if (verbose) Debug.Log("import x" + sheets.Length + " sheets");
                 
            for (int i = 0; i < sheets.Length; i++)
            {
                ssheet_import(sheets[i]);
            }
        }

        /// <summary>
        /// import and save a single spreadsheet
        /// returns : path to generated files
        /// </summary>
        static public string[] ssheet_import(LocaDataSheet sheet)
        {
            if (sheet == null)
            {
                Debug.LogError("no scriptable with tabs ids ?");
                return new string[0];
            }

            DataSheetTab[] tabs = sheet.tabs;

            if (verbose) Debug.Log("import x" + tabs.Length + " tabs");

            EditorUtility.DisplayProgressBar("importing loca " + sheet.url, "fetching...", 0f);

            List<string> output = new List<string>();
            for (int i = 0; i < sheet.tabs.Length; i++)
            {
                var tab = sheet.tabs[i];

                EditorUtility.DisplayProgressBar("importing tab" + tab.displayName, "fetching...", (1f * i) / (1f * tabs.Length));
                output.Add(tab_import(sheet, tab));
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();

            return output.ToArray();
        }

        /// <summary>
        /// specific tab of a spreadsheet
        /// </summary>
        static public string tab_import(LocaDataSheet sheet, DataSheetTab tab)
        {
            int idx = sheet.getTabIndex(tab);

            //EditorUtility.DisplayProgressBar("importing tab "+tab.displayName, "fetching...", 0f);

            tab.cache = importAndSaveSheetTab(sheet.sheetUrlUid, tab);
            tab.cache = tab.cache.Substring(Application.dataPath.Length + 1); // relative to project

            sheet.tabs[idx] = tab;

            //EditorUtility.ClearProgressBar();

            return tab.cache;
        }

        /// <summary>
        /// where the download and treatment of original CSV is done
        /// return : complete file path
        /// </summary>
        static protected string importAndSaveSheetTab(string sheetUrl, DataSheetTab dt)
        {
            //fileContent is raw downloadHandler text
            string fileContent = GoogleSpreadsheetBridge.ssheet_import(sheetUrl, dt.tabUrlId);

            // path/to/sheets.ext
            string _folder = LocalizationPaths.sysImports;

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

            //string fileName = getTabIdFileName(tabId);
            string fileName = dt.tabName + "_" + dt.tabUrlId;

            string filePath = Path.Combine(_folder, fileName + LocalizationPaths.langExtDot);
            File.WriteAllText(filePath, fileContent);

            //FileStream stream = File.OpenRead(filePath);
            //stream.Close();

            Debug.Log("  saved : <b>" + fileName + "</b> ; chars saved in file : " + fileContent.Length);

            return filePath;
        }

        static protected string getTabIdFileName(string tabId)
        {
            LocaDataSheet data = LocalizationStatics.getScriptableObjectInEditor<LocaDataSheet>();

            if (data == null) return string.Empty;

            return data.getMatchingLabel(tabId);
        }

    }
}