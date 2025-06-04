using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.localizator
{
    [Serializable]
    public struct DataSheetTab
    {
        public const string googleSpreadsheetEditPrefix = "/edit#gid=";

        [Tooltip("name of tab ; used to name file")]
        public string tabName; // tab name in sheet

        [Tooltip("id of tab in url")]
        public string tabUrlId; // ssheet url uid

        public SheetParseType parseType;

        public LocalizationSheetParams tabParams;

        [Tooltip("to debug location")]
        public string cache; // relative to Assets/
        
        // getter

        public string CacheTxt => cache + ".txt";
        public string CacheCsv => cache + ".parser";

		public string Url => googleSpreadsheetEditPrefix + tabUrlId;

		public string DisplayName => tabName + "&" + tabUrlId;

        public bool compare(DataSheetTab other)
        {
            if (tabName != other.tabName) return false;
            if (tabUrlId != other.tabUrlId) return false;
            return true;
        }

    }

}
