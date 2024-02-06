using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// all tools to clean and generate stuff
/// from imported content
/// </summary>
namespace fwp.localizator.editor
{
    using fwp.localizator;
    using UnityEditor;
    using System.IO;
    using System.Text;

    public class GenerateSheetUtils
    {
        static public bool verbose => LocalizationManager.verbose;

        static public void trad_files_generation(LocalizationSheetParams param)
        {
            var sups = LocalizationManager.instance.getSupportedLanguages();

            if (verbose)
                Debug.Log("generating for x" + sups.Length + " languages");

            EditorUtility.DisplayProgressBar("converting loca", "loading...", 0f);

            for (int i = 0; i < sups.Length; i++)
            {
                IsoLanguages lang = sups[i];

                EditorUtility.DisplayProgressBar("converting loca", "langage : " + lang.ToString(), (1f * i) / (1f * sups.Length));

                trad_file_generate(lang, param);
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        static public void csv_file_generate(LocalizationSheetParams param)
        {
            // fetching all tabs txts
            string[] tabsFiles = Directory.GetFiles(LocalizationManager.sys_localization_import, "*.txt");

            if (tabsFiles.Length <= 0)
            {
                Debug.LogWarning("no tabs txt @ " + LocalizationManager.sys_localization_import);
                return;
            }

            // all tabs
            for (int i = 0; i < tabsFiles.Length; i++)
            {
                Debug.Log(tabsFiles[i]);

                // read and create csv
                var csv = new CsvParser().generateFromPath(tabsFiles[i], param.langLineIndex);
                csv.fillAutoUid(param.langLineIndex);
                csv.save();
            }
        }

        /// <summary>
        /// merge all tabs into a single file for given language
        /// </summary>
        /// <param name="lang"></param>
        static public void trad_file_generate(IsoLanguages lang, LocalizationSheetParams param)
        {
            var parsers = CsvParser.loadParsers();

            StringBuilder output = new StringBuilder();

            // all tabs
            for (int i = 0; i < parsers.Length; i++)
            {
                var csv = parsers[i];

                // generate file
                string tabOutput = generateLangFile(csv, lang, param);

                output.AppendLine(tabOutput);
            }

            //save

            string outputPath = Path.Combine(Application.dataPath,
                LocalizationManager.sys_localization, "lang_" + lang + ".txt");

            if (verbose)
                Debug.Log("saving : " + outputPath + " (" + output.Length + " char)");

            File.WriteAllText(outputPath, output.ToString());
        }

        /// <summary>
        /// generate the list of key=value
        /// for localization per lang
        /// </summary>
        static string generateLangFile(CsvParser csv, IsoLanguages lang, LocalizationSheetParams param)
        {
            //string tabContent = File.ReadAllText(filePath);
            //CsvParser csv = CsvParser.parse(tabContent, param.langLineIndex);

            int langColumnIndex = getLangColumnIndex(csv, lang); //  search for matching language column
            if (langColumnIndex < 0) return string.Empty;

            StringBuilder output = new StringBuilder();

            Debug.Log($"solveTabAutoId <b>lang : {lang}</b> , ssheet UIDs column <b>#{param.uidColumn}</b> , lines x" + csv.lines.Count);

            //first line is languages
            for (int j = 1; j < csv.lines.Count; j++)
            {
                var values = csv.lines[j].cell;

                if (values.Count < 2)
                {
                    if (verbose) Debug.LogWarning("empty line #" + j);
                    continue; // skip empty lines
                }

                // lang column overflow provided datas
                if (langColumnIndex >= values.Count)
                {
                    if (verbose) Debug.LogWarning("lang overflow #" + j);
                    continue;
                }

                var key = values[(int)param.uidColumn]; // uid key

                if (verbose) Debug.Log("  checking key=" + key);

                string langValue = sanitizeValue(values[langColumnIndex]);

                // skipping empty values
                if (langValue.Length <= 2)
                {
                    if (verbose) Debug.LogWarning("empty value (key=" + key + ") #" + j);
                    continue;
                }

                string line = key + "=" + langValue;

                if (verbose)
                    Debug.Log("     append : " + line);

                output.AppendLine(line);
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
            Debug.Assert(csv != null, "null csv ?");

            if (csv.lines.Count <= 0)
            {
                Debug.Assert(csv.lines.Count > 0, "no lines in csv ?");
                return -1;
            }

            // after CSV treatment on import, header is removed,
            // language are on first line
            string[] langs = csv.lines[0].cell.ToArray(); // each lang of cur line

            string langStr = lang.ToString().ToLower();

            int langColumnIndex = -1;
            for (int j = 0; j < langs.Length; j++)
            {
                string cellLang = langs[j].Trim().ToLower();

                if (cellLang.Length < 2) // empty
                    continue;

                //if(verbose) Debug.Log(cellLang + " (x" + langs.Length + ") vs " + langStr);

                // is the right language ?
                if (cellLang == langStr)
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
                    Debug.LogWarning("raw : " + csv.lines[0].raw);
                    for (int i = 0; i < langs.Length; i++)
                    {
                        Debug.LogWarning(langs[i]);
                    }
                }

                return -1;
            }

            return langColumnIndex;
        }
    }
}