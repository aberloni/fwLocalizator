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
                drawFile(langs[fileSelect]);
            }
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
                    fileRefresh(LFile);
                    fileSelect = i;

                    assoc_selected.Clear();
                    assoc_user.Clear();
                    if (LocalizationMind.Languages.getIso() == LFile.iso)
                    {
                        // just display lines content
                        // each key of selected file
                        foreach (var l in LFile.GetLines())
                        {
                            assoc_selected.Add(l.key, new BuffAssoc()
                            {
                                loca = Localization.GetContent(l.key, LFile.iso)
                            });
                        }
                    }
                    else
                    {
                        // each keys of user lang file
                        var _file = LocalizationMind.Sheets.getFileByLang(LocalizationMind.Languages.getIso());
                        foreach (var l in _file.GetLines())
                        {
                            assoc_user.Add(l.key, new BuffAssoc()
                            {
                                loca = Localization.GetContent(l.key, LFile.iso),
                                locaUser = Localization.GetContent(l.key, _file.iso),
                            });
                        }
                    }

                }

                GUILayout.EndHorizontal();
            }
        }

        void drawFile(LocalizationFile file)
        {
            GUILayout.Label("debug." + file);

            var curr_lang = "[" + file.iso + "]";
            var user_lang = "[" + LocalizationMind.Languages.getIso() + "]";

            if (assoc_user.Count <= 0)
            {
                var lines = file.GetLines();
                for (int i = 0; i < lines.Length; i++)
                {
                    GUILayout.Label("#" + i + " " + lines[i], GuiHelper.wrapped);
                }
            }
            else
            {
                GUILayout.Label(curr_lang + " VS " + user_lang);

                foreach (var a in assoc_user)
                {
                    GUILayout.Label(a.Key + "|" + a.Value.locaUser);

                    string value = a.Value.loca;

                    if (a.Value.loca.Contains("missing")) value = "<color=red>" + value + "</color>";
                    else value = "<color=green>" + value + "</color>";
                    
                    GUILayout.Label(value, GuiHelper.wrapped);
                }

            }

        }

        struct BuffAssoc
        {
            public string loca; // loca in file language
            public string locaUser; // loca in user language
        }

        Dictionary<string, BuffAssoc> assoc_selected = new();
        Dictionary<string, BuffAssoc> assoc_user = new();

        void fileRefresh(LocalizationFile LFile)
        {
            CsvParser.refreshCache();
            //var sheet = mgr.getSheets()[0];
            //LocalizationFile file = mgr.getFileByLang(l.iso);
            GenerateSheetUtils.trad_file_generate(LFile.iso);
        }

    }

}