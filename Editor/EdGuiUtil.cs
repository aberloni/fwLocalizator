using UnityEngine;

namespace fwp.localizator.editor
{

	static public class EdGuiUtil
	{

		static public GUIStyle genBackgroundStyle(Color col)
		{
			GUIStyle style = new GUIStyle();
			style.normal.background = MakeTex(600, 1, col);
			style.padding = new RectOffset(10, 10, 10, 10);
			return style;
		}

		static public Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}

	}

}