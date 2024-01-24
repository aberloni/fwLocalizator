using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator
{
    static public class LocalizationMenuItems
    {

        [MenuItem(LocalizationManager._menu_item_path + "ppref / de")] public static void pprefDE() => editor_switchLanguage(IsoLanguages.de, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/en")] public static void pprefEN() => editor_switchLanguage(IsoLanguages.en, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/es")] public static void pprefES() => editor_switchLanguage(IsoLanguages.es, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/fr")] public static void pprefFR() => editor_switchLanguage(IsoLanguages.fr, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/it")] public static void pprefIT() => editor_switchLanguage(IsoLanguages.it, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/po")] public static void pprefPO() => editor_switchLanguage(IsoLanguages.po, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/ru")] public static void pprefRU() => editor_switchLanguage(IsoLanguages.ru, false);
        [MenuItem(LocalizationManager._menu_item_path + "ppref/cn")] public static void pprefZH() => editor_switchLanguage(IsoLanguages.zh, false);
                  
        [MenuItem(LocalizationManager._menu_item_path + "swap/deu")] public static void swapDE() => editor_switchLanguage(IsoLanguages.de);
        [MenuItem(LocalizationManager._menu_item_path + "swap/eng")] public static void swapEN() => editor_switchLanguage(IsoLanguages.en);
        [MenuItem(LocalizationManager._menu_item_path + "swap/esp")] public static void swapES() => editor_switchLanguage(IsoLanguages.es);
        [MenuItem(LocalizationManager._menu_item_path + "swap/fre")] public static void swapFR() => editor_switchLanguage(IsoLanguages.fr);
        [MenuItem(LocalizationManager._menu_item_path + "swap/ita")] public static void swapIT() => editor_switchLanguage(IsoLanguages.it);
        [MenuItem(LocalizationManager._menu_item_path + "swap/por")] public static void swapPO() => editor_switchLanguage(IsoLanguages.po);
        [MenuItem(LocalizationManager._menu_item_path + "swap/rus")] public static void swapRU() => editor_switchLanguage(IsoLanguages.ru);
        [MenuItem(LocalizationManager._menu_item_path + "swap/chi")] public static void swapZH() => editor_switchLanguage(IsoLanguages.zh);

        public static void editor_switchLanguage(IsoLanguages newLang, bool swap = true)
        {
            LocalizationManager.instance.setSavedLanguage(newLang, swap);
        }

    }

}
