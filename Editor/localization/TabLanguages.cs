using UnityEngine;
using UnityEditor;

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
            if (fileSelect >= 0) drawFile(langs[fileSelect]);
        }

        void drawSummary()
        {
            GUILayout.Space(10f);

            var _langs = LocalizationMind.Sheets.lang_files;

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("langs files x" + _langs.Length);

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
                    fileRefresh(LFile);
                    fileSelect = i;
                }

                GUILayout.EndHorizontal();
            }
        }

        void drawFile(LocalizationFile file)
        {
            var lines = file.GetLines();
            for (int i = 0; i < lines.Length; i++)
            {
                var l = lines[i];
                GUILayout.Label("#" + i + " > " + l, GuiHelper.wrapped);
                GUILayout.Space(2);
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