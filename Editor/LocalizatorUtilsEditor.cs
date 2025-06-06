using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator.editor

{
    static public class LocalizatorUtilsEditor
	{
		static public DataSheetTab tab_fetch(string tabUid)
		{
			var sheets = getSheetsData();
			foreach (var ss in sheets)
			{
				foreach (var t in ss.tabs)
				{
					if (t.tabUrlId == tabUid)
						return t;
				}
			}

			Debug.LogWarning("could not locate data sheet tab :" + tabUid);
			return default(DataSheetTab);
		}

		static LocaDataSheet[] sheets; // data to fetch content online

		/// <summary>
		/// returns default, first one
		/// </summary>
		static public LocaDataSheet getSheetData(string filter, bool clearCache = false)
        {
            getSheetsData(clearCache);
            if(sheets != null && sheets.Length > 0)
            {
				foreach(var s in sheets)
				{
					if (s.name.Contains(filter)) return s;
				}
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
			var all = AssetDatabase.FindAssets("t:" + typ.Name);
			List<ScriptableObject> output = new();
			for (int i = 0; i < all.Length; i++)
			{
				Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), typ);
				var so = obj as ScriptableObject;
				if (so == null) continue;
				if (!string.IsNullOrEmpty(filter) && !so.name.Contains(filter)) continue;
				output.Add(so);
			}
			return output.ToArray();
		}

		static public T[] getScriptableObjectsInEditor<T>() where T : ScriptableObject
		{
			var ss = getScriptableObjectsInEditor(typeof(T));
			List<T> ret = new();
			foreach (var s in ss)
			{
				T cmp = s as T;
				if (cmp == null) continue;
				ret.Add(cmp);
			}
			return ret.ToArray();
		}
	}
}
