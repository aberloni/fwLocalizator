using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localizator.editor
{
    using fwp.localizator;

    public class ExportLocalisationToGoogleForm
    {
        static public bool verbose = false;

        static public string outputFolder => Path.Combine(Application.dataPath, LocalizationManager.path_resource_localization, "import");

        static public void ssheets_import(LocaDataSheet[] sheets)
        {
            for (int i = 0; i < sheets.Length; i++)
            {
                ssheet_import(sheets[i]);
            }
        }

        /// <summary>
        /// returns : path to generated files
        /// </summary>
        static public string[] ssheet_import(LocaDataSheet sheet)
        {
            if (sheet == null)
            {
                Debug.LogError("no scriptable with tabs ids ?");
                return new string[0];
            }

            DataSheetLabel[] tabs = sheet.sheetTabsIds;

            EditorUtility.DisplayProgressBar("importing loca " + sheet.url, "fetching...", 0f);

            List<string> output = new List<string>();
            for (int i = 0; i < sheet.sheetTabsIds.Length; i++)
            {
                var tab = sheet.sheetTabsIds[i];

                EditorUtility.DisplayProgressBar("importing tab" + tab.displayName, "fetching...", (1f * i) / (1f * tabs.Length));
                output.Add(tab_import(sheet, tab));
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();

            return output.ToArray();
        }

        static public string tab_import(LocaDataSheet sheet, DataSheetLabel tab)
        {
            int idx = sheet.getTabIndex(tab);

            //EditorUtility.DisplayProgressBar("importing tab "+tab.displayName, "fetching...", 0f);

            tab.cache = importAndSaveSheetTab(sheet.sheetUrlUid, tab);
            tab.cache = tab.cache.Substring(Application.dataPath.Length + 1);

            sheet.sheetTabsIds[idx] = tab;

            //EditorUtility.ClearProgressBar();

            return tab.cache;
        }

        static public void trad_files_generation(LocalizationSheetParams param)
        {
            var sups = LocalizationManager.instance.getSupportedLanguages();

            if(verbose)
                Debug.Log("generating for x" + sups.Length + " languages");

            EditorUtility.DisplayProgressBar("converting loca", "loading...", 0f);

            for (int i = 0; i < sups.Length; i++)
            {
                IsoLanguages lang = sups[i];

                EditorUtility.DisplayProgressBar("converting loca", "langage : " + lang.ToString(), (1f * i) / (1f * sups.Length));

                trad_file_generate(lang, (int)param.uidColumn);
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// merge all tabs into a single file for given language
        /// </summary>
        /// <param name="lang"></param>
        static public void trad_file_generate(IsoLanguages lang, int uidColumn)
        {
            string importPath = Path.Combine(Application.dataPath,
                LocalizationManager.path_resource_localization, "import");

            // all tabs txt
            string[] tabsFiles = Directory.GetFiles(importPath, "*.txt");

            if(verbose)
                Debug.Log(" generating trad file for : <b>" + lang.ToString().ToUpper() + "</b>");

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < tabsFiles.Length; i++)
            {
                string tabOutput = solveTabAutoId(tabsFiles[i], lang, uidColumn);
                output.AppendLine(tabOutput);
            }

            //save

            string outputPath = Path.Combine(Application.dataPath,
                LocalizationManager.path_resource_localization, "lang_" + lang + ".txt");

            if (verbose)
                Debug.Log("saving : " + outputPath + " (" + output.Length + " char)");

            File.WriteAllText(outputPath, output.ToString());
        }

        /// <summary>
        /// solving whatever was saved in raw files
        /// </summary>
        static string solveTab(string filePath, IsoLanguages lang, int uidColumn)
        {
            string tabContent = File.ReadAllText(filePath);

            if (verbose)
                Debug.Log("parsing csv file : " + filePath);

            CsvParser csv = CsvParser.parse(tabContent);

            if (verbose)
                Debug.Log("  solved x" + csv.lines.Count + " lines after CSV parser");

            //for (int j = 0; j < lines.Length; j++) { Debug.Log("#" + j); Debug.Log(lines[j]); }

            //  search for mathing language column

            //after CSV treatment on import, header is removed, language are on first line
            int LANGUAGE_LINE_INDEX = 0; // languages are stored in column 5
            string[] langs = csv.lines[LANGUAGE_LINE_INDEX].cell.ToArray(); // each lang of cur line

            int langColumnIndex = -1;
            for (int j = 0; j < langs.Length; j++)
            {
                // is the right language ?
                if (langs[j].Trim().ToLower() == lang.ToString().ToLower())
                {
                    langColumnIndex = j;
                    //Debug.Log("found lang "+lang+" at column #"+ langColumnIndex);
                }
            }

            if (langColumnIndex < 0)
            {
                Debug.LogWarning("sheet import : <b>no column</b> for lang : <b>" + lang.ToString().ToUpper() + "</b> | out of x" + langs.Length);

                if (verbose)
                {
                    Debug.LogWarning(csv.lines[LANGUAGE_LINE_INDEX].raw);
                    for (int i = 0; i < langs.Length; i++)
                    {
                        Debug.LogWarning(langs[i]);
                    }
                }

                return string.Empty;
            }

            int cntNotTranslation = 0;

            StringBuilder output = new StringBuilder();
            
            //first line is languages
            for (int j = 1; j < csv.lines.Count; j++)
            {
                string[] datas = csv.lines[j].cell.ToArray();

                //Debug.Log(j + " => x" + datas.Length);
                //Debug.Log(line);

                // here filter everything NOT grab from excel file
                if (datas.Length < 2) continue; // empty line

                // uid key
                var key = datas[uidColumn];
                if (key.Length < 1)
                {
                    Debug.LogWarning("empty key @ line " + j);
                    continue; // key empty
                }
                else if (key.Contains(" ")) // pas d'espace dans les ids
                {
                    Debug.LogWarning("skipping keys with spaces : " + key);
                    continue; // skip line with spaces
                }

                // ligne vide, pas assez de colonnes dedans ?
                if (langColumnIndex >= datas.Length)
                {
                    Debug.LogError("line #" + j + " => line has not enought cells for a lang in column #" + langColumnIndex + " / out of x" + datas.Length + " columns");
                    Debug.Log(csv.lines[j].raw);
                    continue;
                }

                //https://www.c-sharpcorner.com/uploadfile/mahesh/trim-string-in-C-Sharp/
                // remove white spaces on sides
                string langValue = datas[langColumnIndex].Trim();
                
                //remove "" around escaped values
                langValue = langValue.Replace(ParserStatics.SPREAD_CELL_ESCAPE_VALUE.ToString(), string.Empty);

                // note : line return IN cells should be replaced by | here
                output.AppendLine(key + "=" + langValue);
            }

            if (cntNotTranslation > 0)
            {
                Debug.LogWarning("x" + cntNotTranslation + " lines have NO translation in " + lang);
            }

            return output.ToString();
        }


        /// <summary>
        /// solving whatever was saved in raw files
        /// </summary>
        static string solveTabAutoId(string filePath, IsoLanguages lang, int uidColumn)
        {
            string tabContent = File.ReadAllText(filePath);
            CsvParser csv = CsvParser.parse(tabContent);

            int langColumnIndex = getLangColumnIndex(csv, lang); //  search for matching language column
            if (langColumnIndex < 0) return string.Empty;

            StringBuilder output = new StringBuilder();

            string _uid = string.Empty;
            int cnt = 0;

            //Debug.Log("UID column #" + uidColumn);

            //first line is languages
            for (int j = 1; j < csv.lines.Count; j++)
            {
                string[] datas = csv.lines[j].cell.ToArray();

                if (datas.Length < 2)
                {
                    _uid = string.Empty;
                    cnt = 0;
                    continue; // skip empty lines
                }

                // lang column overflow provided datas
                if (langColumnIndex >= datas.Length) continue;

                
                var key = datas[uidColumn]; // uid key

                if (key.Length < 3)
                {
                    Debug.Assert(_uid.Length > 0, "need uid here");
                    key = _uid; // empty key = last known
                }
                else
                {
                    if (key != _uid)
                    {
                        _uid = key;
                        cnt = 0;
                    }
                }

                cnt++;
                if (cnt < 10) key = key + "-0" + cnt;
                else key = key + "-" + cnt;

                string langValue = sanitizeValue(datas[langColumnIndex]);

                if(langValue.Length <= 2)
                {
                    Debug.LogWarning("empty value @ " + key);
                    continue;
                }

                output.AppendLine(key + "=" + langValue);
            }

            return output.ToString();
        }

        static string sanitizeValue(string val)
        {
            val = val.Trim();

            //remove "" around escaped values
            val = val.Replace(ParserStatics.SPREAD_CELL_ESCAPE_VALUE.ToString(), string.Empty);

            return val;
        }

        static int getLangColumnIndex(CsvParser csv, IsoLanguages lang)
        {

            //after CSV treatment on import, header is removed, language are on first line
            int LANGUAGE_LINE_INDEX = 0; // languages are stored in column 5
            string[] langs = csv.lines[LANGUAGE_LINE_INDEX].cell.ToArray(); // each lang of cur line

            int langColumnIndex = -1;
            for (int j = 0; j < langs.Length; j++)
            {
                // is the right language ?
                if (langs[j].Trim().ToLower() == lang.ToString().ToLower())
                {
                    langColumnIndex = j;
                    //Debug.Log("found lang "+lang+" at column #"+ langColumnIndex);
                }
            }

            if (langColumnIndex < 0)
            {
                Debug.LogWarning("sheet import : <b>no column</b> for lang : <b>" + lang.ToString().ToUpper() + "</b> | out of x" + langs.Length);

                if (verbose)
                {
                    Debug.LogWarning(csv.lines[LANGUAGE_LINE_INDEX].raw);
                    for (int i = 0; i < langs.Length; i++)
                    {
                        Debug.LogWarning(langs[i]);
                    }
                }

                return -1;
            }

            return langColumnIndex;
        }

        /// <summary>
        /// where the download and treatment of original CSV is done
        /// return : complete file path
        /// </summary>
        static protected string importAndSaveSheetTab(string sheetUrl, DataSheetLabel dt)
        {
            //fileContent is raw downloadHandler text
            string fileContent = LocaSpreadsheetBridge.ssheet_import(sheetUrl, dt.tabId);

            string _folder = outputFolder;

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

            //string fileName = getTabIdFileName(tabId);
            string fileName = dt.fieldId + "_" + dt.tabId;

            string filePath = Path.Combine(outputFolder, fileName + ".txt");
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
