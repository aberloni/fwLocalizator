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
                    // fileRefresh(LFile);
                    fileSelect = i; // focus this file (LFile getter)

                    var iso = LocalizationMind.Languages.getIso();

                    assoc_selected.Clear();
                    assoc_user.Clear();
                    if (iso == LFile.iso)
                    {
                        // just display lines content
                        // each key of selected file
                        foreach (var l in LFile.GetLines())
                        {
                            var assoc = new BuffAssoc();
                            assoc.assign(l.key, LFile.iso);
                            assoc_selected.Add(l.key, assoc);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("language comparison: " + iso + " & " + LFile.iso);

                        // each keys of user lang file
                        var _file = LocalizationMind.Sheets.getFileByLang(iso);
                        foreach (var l in _file.GetLines())
                        {
                            BuffAssoc assoc;

                            if (assoc_user.ContainsKey(l.key))
                            {
                                Debug.LogWarning("duplicate : " + l.key + " => " + LFile.iso);

                                assoc = assoc_user[l.key];
                                assoc.duplicate = true;
                                assoc_user[l.key] = assoc;

                                continue;
                            }

                            assoc = new BuffAssoc();
                            assoc.assign(l.key, LFile.iso, _file.iso);
                            assoc_user.Add(l.key, assoc);
                        }
                    }

                }

                GUILayout.EndHorizontal();
            }
        }

        const string pattern_missing = "missing";
        bool missing = true;
        void drawFileDebug(LocalizationFile file)
        {
            GUILayout.Label("debug." + file);

            missing = GUILayout.Toggle(missing, "missing only");

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

                int cnt = 0;
                foreach (var a in assoc_user)
                {
                    string value = a.Key + "=" + a.Value.locaFile;

                    if (a.Value.duplicate) value += " <color=red>[DUPLICATE=" + a.Key + "]</color>";

                    if (missing == a.Value.file_missing)
                    {
                        value = " <color=red>" + value + "</color>";
                    }
                    else
                    {
                        value = " <color=green>" + value + "</color>";
                    }

                    GUILayout.Label(value, GuiHelper.wrapped);
                    cnt++;
                }
                GUILayout.Label("qty x" + cnt);
            }

        }

        struct BuffAssoc
        {
            public bool duplicate;

            public string locaFile; // loca in file language
            public string locaUser; // loca in user language

            public bool file_missing;
            public bool user_missing;

            public void assign(string key, IsoLanguages isoFrom, IsoLanguages isoTo = IsoLanguages.en)
            {
                locaFile = Localization.GetContent(key, isoFrom);
                file_missing = locaFile.Contains(pattern_missing);

                if (isoTo != isoFrom)
                {
                    locaUser = Localization.GetContent(key, isoTo);
                    user_missing = locaUser.Contains(pattern_missing);
                }
            }
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