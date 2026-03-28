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

        public void Draw(LocalizationManager LManager)
        {
            if (langs == null) langs = LManager.lang_files;

            drawSummary(LManager);
            if (fileSelect >= 0) drawFile(langs[fileSelect]);
        }

        void drawSummary(LocalizationManager LManager)
        {
            GUILayout.Space(10f);

            var langs = LManager.lang_files;

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("langs files x" + langs.Length);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("clear csv cache(s)")) CsvParser.refreshCache();

            if (GUILayout.Button("(re)generate trad files"))
            {
                CsvParser.refreshCache();
                LManager.reloadFiles();
                GenerateSheetUtils.trads_generate();
            }
            GUILayout.EndHorizontal();

            for (int i = 0; i < langs.Length; i++)
            {
                var LFile = langs[i];

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(" ? ", GuiHelper.btnXS))
                {
                    UnityEditor.Selection.activeObject = LFile.textAsset;
                }

                GUILayout.Label(LFile.iso.ToString());
                GUILayout.Label("lines x" + LFile.GetLinesCount(), GuiHelper.btnM);
                GUILayout.Label("char x" + LFile.textAsset.text.Length, GuiHelper.btnM);

                if (GUILayout.Button("refresh", GuiHelper.btnS))
                {
                    fileRefresh(LFile);
                }

                if (GUILayout.Button("debug", GuiHelper.btnS))
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