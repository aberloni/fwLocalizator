using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

static public class GuiHelper
{
    static public void Separator() => GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
    static readonly public GUILayoutOption wXS = GUILayout.Width(50f);
    static readonly public GUILayoutOption wS = GUILayout.Width(75f);
    static readonly public GUILayoutOption wM = GUILayout.Width(100f);
    static readonly public GUILayoutOption wL = GUILayout.Width(150f);
    static readonly public GUILayoutOption wXL = GUILayout.Width(200f);

    static readonly public GUIStyle wrapped = new GUIStyle()
    {
        richText = true,
        fontStyle = FontStyle.Normal,
        normal = new GUIStyleState()
        {
            textColor = Color.ghostWhite,
        },
        wordWrap = true,
        alignment = TextAnchor.MiddleLeft,
    };

    static Dictionary<string, bool> edFoldout = new();
    static public bool DrawFoldout(string label, string uid, bool isSection = false)
    {
        bool foldState = false;
        if (edFoldout.ContainsKey(uid))
        {
            foldState = edFoldout[uid];
        }

        bool _state;

        if (isSection)
        {
            _state = EditorGUILayout.Foldout(foldState, label, true, UtilStyles.FoldoutSection(15));
        }
        else
        {
            _state = EditorGUILayout.Foldout(foldState, label, true);
        }

        if (_state != foldState)
        {
            if (!edFoldout.ContainsKey(uid)) edFoldout.Add(uid, false);
            edFoldout[uid] = _state;
        }

        return _state;
    }
}
