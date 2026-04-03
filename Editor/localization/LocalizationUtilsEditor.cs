using UnityEngine;
using System.Collections.Generic;

namespace fwp.localizator.editor
{
#if UNITY_EDITOR
    static public class LocalizationUtilsEditor
    {

        static public ScriptableObject[] getScriptablesEditor(string filter = null)
        {
            var type = typeof(ScriptableObject);
            var all = UnityEditor.AssetDatabase.FindAssets("t:" + type.Name);
            List<ScriptableObject> output = new();
            for (int i = 0; i < all.Length; i++)
            {
                Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(all[i]), type);
                var so = obj as ScriptableObject;
                if (so == null) continue;
                if (!string.IsNullOrEmpty(filter) && !so.name.Contains(filter)) continue;
                output.Add(so);
            }
            return output.ToArray();
        }
        
		static public T[] getDialogObjects<T>(string filter = null) where T : class
		{
			ScriptableObject[] ss = null;
			
			getScriptablesEditor(filter);

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

#endif
}