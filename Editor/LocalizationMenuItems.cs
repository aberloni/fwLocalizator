using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator
{
    static public class LocalizationMenuItems
    {

        [MenuItem(LocalizationManager._menu_item_path + "ppref/de")] public static void pprefDE() => editor_switchLanguage(IsoLanguages.de);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/en")] public static void pprefEN() => editor_switchLanguage(IsoLanguages.en);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/es")] public static void pprefES() => editor_switchLanguage(IsoLanguages.es);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/fr")] public static void pprefFR() => editor_switchLanguage(IsoLanguages.fr);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/it")] public static void pprefIT() => editor_switchLanguage(IsoLanguages.it);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/pt")] public static void pprefPO() => editor_switchLanguage(IsoLanguages.pt);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/ru")] public static void pprefRU() => editor_switchLanguage(IsoLanguages.ru);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/zht")] public static void pprefZHT() => editor_switchLanguage(IsoLanguages.zh_hant);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/zhs")] public static void pprefZHS() => editor_switchLanguage(IsoLanguages.zh_hans);

        /// <summary>
        /// swap:trigger reaction to language swap
        /// </summary>
        public static void editor_switchLanguage(IsoLanguages newLang, bool swap = true)
        {
            if (LocalizationManager.instance == null)
            {
                Debug.LogWarning("only at runtime");
                Debug.LogWarning("can't set language: no instance of localization");
                return;
            }

            LocalizationManager.instance.setSavedLanguage(newLang, swap);
        }

    }

}
