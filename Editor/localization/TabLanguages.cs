using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace fwp.localizator.editor
{

    public class TabLanguages : iLocaTab
    {
        public string GetTabName() => "Keys";

        int fileSelect = -1;
        LocalizationFile[] langs;

        public void Refresh(bool hard)
        { }

        public void Draw()
        {
            if (langs == null) langs = LocalizationMind.Sheets.lang_files;

            drawSummary();
            if (fileSelect >= 0)
            {
                drawFileDebug(langs[fileSelect]);
            }
        }

        void drawSummary()
        {
            GUILayout.Space(10f);

            var _langs = LocalizationMind.Sheets.lang_files;

            GUILayout.Label("langs files x" + _langs.Length);

            if (GUILayout.Button("sanity"))
            {
                Debug.LogWarning("SANITY");
                GenerateSheetUtils.sanity_duplicates();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("clear csv cache(s)")) CsvParser.refreshCache();

            if (GUILayout.Button("(re)generate trad files"))
            {
                CsvParser.refreshCache();
                LocalizationMind.Sheets.reloadFiles();
                GenerateSheetUtils.trads_generate();
            }
            GUILayout.EndHorizontal();

            for (int i = 0; i < _langs.Length; i++)
            {
                var LFile = _langs[i];
                if (LFile == null) continue;

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(" ? ", GuiHelper.wXS))
                {
                    UnityEditor.Selection.activeObject = LFile.textAsset;
                }

                GUILayout.Label(LFile.iso.ToString());
                GUILayout.Label("lines x" + LFile.GetLinesCount(), GuiHelper.wM);
                GUILayout.Label("char x" + LFile.textAsset.text.Length, GuiHelper.wM);

                if (GUILayout.Button("refresh", GuiHelper.wS))
                {
                    fileRefresh(LFile);
                }

                if (GUILayout.Button("debug", GuiHelper.wS))
                {
                    // fileRefresh(LFile);
                    fileSelect = i; // focus this file (LFile getter)

                    var iso = LocalizationMind.Languages.getIso();

                    if (iso != LFile.iso) Debug.LogWarning("language comparison: " + iso + " & " + LFile.iso);
                    LFile.generateComparison(iso);

                }

                GUILayout.EndHorizontal();
            }
        }


        const string line_missing = "<color=red>missing</color>:";
        void drawFileDebug(LocalizationFile file)
        {
            var user = LocalizationMind.Languages.getIso();

            GUILayout.Label("debug." + file.iso + " VS " + user);

            if (file.iso == user)
            {
                GUILayout.Label("debug same file as user lang");
            }
            else
            {
                // var curr_lang = "[" + file.iso + "]";
                // var user_lang = "[" + user + "]";
                var buff = file.GetDiffs(user);
                foreach (var b in buff)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(b.key, GUILayout.Width(150f));
                    GUILayout.Label(b.local != null ? b.local.value : line_missing + file.iso, GuiHelper.wrapped);
                    GUILayout.Label(b.user != null ? b.user.value : line_missing + file.iso, GuiHelper.wrapped);
                    GUILayout.EndHorizontal();
                }
            }
        }

        void fileRefresh(LocalizationFile LFile)
        {
            CsvParser.refreshCache();
            //var sheet = mgr.getSheets()[0];
            //LocalizationFile file = mgr.getFileByLang(l.iso);

            GenerateSheetUtils.trad_file_generate(LFile.iso);
        }

    }

}