using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEditor;

namespace fwp.localizator
{
    [Serializable]
    public struct DataSheetLabel
    {
        public const string googleSpreadsheetEditPrefix = "/edit#gid=";

        public string fieldId; // tab name in sheet
        public string tabId; // ssheet url uid

        public string url => googleSpreadsheetEditPrefix + tabId;

        public string cache;
        public string cacheAsset => "Assets/" + cache;

        public string displayName => fieldId + "&" + tabId;

        public bool compare(DataSheetLabel other)
        {
            if (fieldId != other.fieldId) return false;
            if (tabId != other.tabId) return false;
            return true;
        }
    }

    /// <summary>
    /// base structure to get a scriptble with uid/tabid couples
    /// - url
    /// - tabs @ spreadsheet
    /// </summary>

    [CreateAssetMenu(menuName = "Localizator/create sheet", order = 100)]
    public class LocaDataSheet : ScriptableObject
    {
        // https://docs.google.com/spreadsheets/d/[my-uid]/edit#gid=[tab-uid]

        public const string googleSpreadsheetBaseUrl = "https://docs.google.com/spreadsheets/d/";

        [Tooltip("string to identify spreadsheet")]
        public string sheetUrlUid;

        [Tooltip("all tabs in that spreadsheet")]
        public DataSheetLabel[] sheetTabsIds;

        public string url => googleSpreadsheetBaseUrl + sheetUrlUid;

        public int getTabIndex(DataSheetLabel tab)
        {
            for (int i = 0; i < sheetTabsIds.Length; i++)
            {
                if (sheetTabsIds[i].compare(tab))
                    return i;
            }
            return -1;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if(sheetUrlUid.StartsWith(LocaSpreadsheetBridge.sheetUrlPrefix))
            {
                sheetUrlUid = sheetUrlUid.Replace(LocaSpreadsheetBridge.sheetUrlPrefix, string.Empty);
            }
        }

        public string getMatchingLabel(string tabId) => getTabIdOfField(tabId).ToString().ToLower();

        public string getTabIdOfField(string field)
        {
            for (int i = 0; i < sheetTabsIds.Length; i++)
            {
                if (sheetTabsIds[i].fieldId == field) return sheetTabsIds[i].tabId;
            }
            return string.Empty;
        }

    }

}
