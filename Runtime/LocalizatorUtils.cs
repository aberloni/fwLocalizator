using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using fwp.localizator;

namespace fwp.localizator
{
    static public class LocalizatorUtils
    {
#if UNITY_EDITOR

        static LocaDataSheet[] sheets; // data to fetch content online

        static public LocaDataSheet getSheetData(bool clearCache = false)
        {
            getSheetsData(clearCache);
            if(sheets != null && sheets.Length > 0)
            {
                return sheets[0];
            }
            return null;
        }

        static public LocaDataSheet[] getSheetsData(bool clearCache = false)
        {
            if (clearCache || sheets == null)
                sheets = getScriptableObjectsInEditor<LocaDataSheet>();
            return sheets;
        }

		static public ScriptableObject[] getScriptableObjectsInEditor(System.Type typ, string filter = null)
		{
			string[] all = AssetDatabase.FindAssets("t:" + typ.Name);
			List<ScriptableObject> output = new List<ScriptableObject>();
			for (int i = 0; i < all.Length; i++)
			{
				Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), typ);
				ScriptableObject so = obj as ScriptableObject;
				if (so == null) continue;

                if (!string.IsNullOrEmpty(filter) && !so.name.Contains(filter)) continue;

				output.Add(so);
			}
			return output.ToArray();
		}

		static public T[] getScriptableObjectsInEditor<T>() where T : ScriptableObject
			=> (T[])getScriptableObjectsInEditor(typeof(T));

#endif
	}
}
