using UnityEditor;
using UnityEngine;

public class UtilStyles
{

	static GUIStyle fontBold;
	static public GUIStyle FontBold()
	{
		if (fontBold == null)
		{
			fontBold = new GUIStyle();
			//sectionTitle.normal.textColor = Color.gray;
			//sectionTitle.richText = true;
			fontBold.fontStyle = FontStyle.Bold;
		}
		return fontBold;
	}

	static private GUIStyle foldoutSection;
	static public GUIStyle FoldoutSection(int size = 15)
	{
		if (foldoutSection == null)
		{
			foldoutSection = new GUIStyle(EditorStyles.foldout);

			foldoutSection.richText = true;
			foldoutSection.normal.textColor = Color.white;

			foldoutSection.fontStyle = FontStyle.Bold;
		}

		foldoutSection.fontSize = size;

		return foldoutSection;
	}

	static private GUIStyle sectionTitle;
	static public GUIStyle SectionTitle(int size = 15, TextAnchor anchor = TextAnchor.MiddleCenter, 
		int verticalMargin = 10,
		int horizontalMargin = 10)
	{
		if (sectionTitle == null)
		{
			sectionTitle = new GUIStyle();

			sectionTitle.richText = true;
			sectionTitle.alignment = anchor;
			sectionTitle.normal.textColor = Color.white;

			sectionTitle.fontStyle = FontStyle.Bold;
			
			sectionTitle.margin = new RectOffset(
				horizontalMargin, horizontalMargin, 
				verticalMargin, verticalMargin);
		}

		sectionTitle.fontSize = size;

		return sectionTitle;
	}

	static GUIStyle foldHeaderTitle;
	static public GUIStyle FoldHeaderTitle()
	{
		if (foldHeaderTitle == null)
		{
			foldHeaderTitle = new GUIStyle(EditorStyles.foldoutHeader);
			foldHeaderTitle.fontStyle = FontStyle.Bold;

			foldHeaderTitle.normal.textColor = Color.white;

			foldHeaderTitle.onFocused.textColor = Color.gray;
			foldHeaderTitle.focused.textColor = Color.gray;

			//foldTitle.onActive.textColor = Color.red;
			//foldTitle.active.textColor = Color.red;

			foldHeaderTitle.fontSize = 20;
			//foldHeaderTitle.richText = true;
			//foldHeaderTitle.alignment = TextAnchor.MiddleCenter;

			//foldTitle.padding = new RectOffset(0, 0, 100, 100);
			//foldHeaderTitle.margin = new RectOffset(20,0,0,0);
		}
		return foldHeaderTitle;
	}

	static GUIStyle foldTitle;
	static public GUIStyle FoldTitle()
	{
		if (foldTitle == null)
		{
			foldTitle = new GUIStyle(EditorStyles.foldout);

			//foldTitle.richText = true;
			foldTitle.fontSize = 20;
		}
		return foldTitle;
	}



	/// <summary>
	/// draw a label with speficic style
	/// </summary>
	static public bool drawSectionTitle(string label, int vSpace = 10, int hSpace = 10)
	{
		return GUILayout.Button(label, SectionTitle(15, TextAnchor.UpperLeft, vSpace, hSpace));
	}
}
