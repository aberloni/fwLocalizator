using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

public class LocalizationWindowUtils
{

    /// <summary>
    /// draw a label with speficic style
    /// </summary>
    static public void drawSectionTitle(string label, float spaceMargin = 20f, int leftMargin = 10)
    {
        if (spaceMargin > 0f)
            GUILayout.Space(spaceMargin);

        GUILayout.Label(label, getSectionTitle(15, TextAnchor.UpperLeft, leftMargin));
    }

    static private GUIStyle gFoldoutSection;
    static public GUIStyle getFoldoutSection(int size = 15)
    {
        if(gFoldoutSection == null)
        {
            gFoldoutSection = new GUIStyle(EditorStyles.foldout);

            gFoldoutSection.richText = true;
            gFoldoutSection.normal.textColor = Color.white;

            gFoldoutSection.fontStyle = FontStyle.Bold;
        }

        gFoldoutSection.fontSize = size;

        return gFoldoutSection;
    }
    static private GUIStyle gSectionTitle;
    static public GUIStyle getSectionTitle(int size = 15, TextAnchor anchor = TextAnchor.MiddleCenter, int leftMargin = 10)
    {
        if (gSectionTitle == null)
        {
            gSectionTitle = new GUIStyle();

            gSectionTitle.richText = true;
            gSectionTitle.alignment = anchor;
            gSectionTitle.normal.textColor = Color.white;

            gSectionTitle.fontStyle = FontStyle.Bold;
            gSectionTitle.margin = new RectOffset(leftMargin, 10, 10, 10);
            //gWinTitle.padding = new RectOffset(30, 30, 30, 30);

        }

        gSectionTitle.fontSize = size;

        return gSectionTitle;
    }

}
