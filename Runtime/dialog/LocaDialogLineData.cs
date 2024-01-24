using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator
{
    [System.Serializable]
    public class LocaDialogLineData
    {
        public string uid;

        //FOR DEBUG ONLY
        public string[] previews;

        [ContextMenu("debug update preview")]
        public void debugUpdatePreview()
        {
            previews = new string[System.Enum.GetValues(typeof(IsoLanguages)).Length];

            Debug.Log(uid);

            var sups = LocalizationManager.instance.getSupportedLanguages();
            foreach (IsoLanguages sup in sups)
            {
                Debug.Log(sup);
                previews[(int)sup] = LocalizationManager.instance.getContent(uid, sup, true);
            }
        }

        //THIS IS WHAT SHOULD PROVIDE LOCA
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
