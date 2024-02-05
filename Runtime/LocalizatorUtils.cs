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

        static public LocaDataSheet getSheetData() => getScriptableObjectInEditor<LocaDataSheet>();
        static public LocaDataSheet[] getSheetsData() => getScriptableObjectsInEditor<LocaDataSheet>();

        static T getScriptableObjectInEditor<T>(string nameContains = "") where T : ScriptableObject
        {
            string[] all = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            for (int i = 0; i < all.Length; i++)
            {
                Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), typeof(T));
                T data = obj as T;

                if (data == null) continue;
                if (nameContains.Length > 0)
                {
                    if (!data.name.Contains(nameContains)) continue;
                }

                return data;
            }
            Debug.LogWarning("can't locate scriptable of type " + typeof(T).Name + " (filter name ? " + nameContains + ")");
            return null;
        }


        /// <summary>
        /// can't cast to T[]
        /// </summary>
        static T[] getScriptableObjectsInEditor<T>() where T : ScriptableObject
        {
            var typ = typeof(T);
            string[] all = AssetDatabase.FindAssets("t:" + typ.Name);

            List<T> output = new List<T>();
            for (int i = 0; i < all.Length; i++)
            {
                Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), typ);
                T so = obj as T;
                if (so == null) continue;
                output.Add(so);
            }

            //Debug.Log(scriptableType + " x"+output.Count+" / x" + all.Length);

            return output.ToArray();
        }


#endif
    }
}
