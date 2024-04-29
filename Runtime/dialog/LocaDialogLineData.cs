using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.dialog
{
    /// <summary>
    /// don't forget to also flag children as serializable
    /// </summary>
    [System.Serializable]
    public class LocaDialogLineData
    {
        public string uid;

#if UNITY_EDITOR
        //FOR DEBUG ONLY
        public string[] previews;

        public void debugUpdatePreview(bool verbose = false)
        {
            List<string> tmp = new List<string>();

            if (verbose)
                Debug.Log("log debug previews @ " + uid);

            var sups = LocalizationManager.instance.getSupportedLanguages();
            foreach (IsoLanguages sup in sups)
            {
                var val = LocalizationManager.instance.getContent(uid, sup, true);
                tmp.Add(val);

                if (verbose)
                    Debug.Log(sup + " => " + val);
            }
            previews = tmp.ToArray();
        }
#endif

        /// <summary>
        /// THIS METHOD IS THE ONE THAT SHOULD PROVIDE LOCA
        /// </summary>
        /// <param name="useFallback"></param>
        /// <returns></returns>
        public string getSolvedLineByUID(bool useFallback = false)
        {
            if (useFallback) return LocalizationManager.instance.getContentSafe(uid);
            return LocalizationManager.instance.getContent(uid);
        }

        public string getSolvedLineByFUID() => LocalizationManager.instance.getContent(uid);

        public bool hasUID()
        {
            //for loading issues .......
            if (uid == null) return false;

            return uid.Length > 0;
        }

        virtual public string stringify()
        {
#if UNITY_EDITOR
            if (LocalizationManager.instance != null)
            {
                int idx = (int)LocalizationManager.instance.getSavedIsoLanguage();
                return previews[idx];
            }
#endif

            return string.Empty;
        }

        static public string getLocaByUID(string uid, bool fallbackIfMissing = false, bool emptyOnMissing = false)
        {
            string content = fallbackIfMissing ?
                LocalizationManager.instance.getContentSafe(uid) :
                LocalizationManager.instance.getContent(uid);
            return getFilteredLocaLine(content, emptyOnMissing);
        }

        static public string getFilteredLocaLine(string contentLocalized, bool emptyOnMissing = false)
        {
            // 2 parce qu'il y a deux charactères qui trainent dans l'export
            if (contentLocalized.Length <= 1)
            {
                contentLocalized = "[empty]";
                //Debug.LogWarning("no text given ?");
            }

            //missing ?
            if (contentLocalized.Length > 0)
            {
                if (contentLocalized[0] == '[')
                {
                    if (emptyOnMissing) contentLocalized = string.Empty;
                }
            }

            return contentLocalized;
        }

    }

}
