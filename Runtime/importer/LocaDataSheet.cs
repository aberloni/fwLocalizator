using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace fwp.localizator
{
    
    /// <summary>
    /// base structure to get a scriptble with uid/tabid couples
    /// - url
    /// - tabs @ spreadsheet
    /// </summary>

    [CreateAssetMenu(menuName = LocalizationManager._asset_menu_path + "create sheet", order = LocalizationManager._asset_menu_order)]
    public class LocaDataSheet : ScriptableObject
    {
        // https://docs.google.com/spreadsheets/d/[my-uid]/edit#gid=[tab-uid]

        public const string googleSpreadsheetBaseUrl = "https://docs.google.com/spreadsheets/d/";

        [Tooltip("string to identify spreadsheet")]
        public string sheetUrlUid;

        [Tooltip("all tabs in that spreadsheet")]
        public DataSheetTab[] tabs;

        public string url => googleSpreadsheetBaseUrl + sheetUrlUid;

        public int getTabIndex(DataSheetTab tab)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].compare(tab))
                    return i;
            }
            return -1;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (sheetUrlUid.StartsWith(GoogleSpreadsheetBridge.sheetUrlPrefix))
            {
                sheetUrlUid = sheetUrlUid.Replace(GoogleSpreadsheetBridge.sheetUrlPrefix, string.Empty);
            }
        }

        public string getMatchingLabel(string tabId) => getTabIdOfField(tabId).ToString().ToLower();

        public string getTabIdOfField(string field)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].tabName == field) return tabs[i].tabUrlId;
            }
            return string.Empty;
        }

    }

}
