using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.localizator.editor
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

		public string Url => googleSpreadsheetEditPrefix + tabUrlId;

		public string DisplayName => tabName + "&" + tabUrlId;
        public string TxtFileName => tabName + "_" + tabUrlId;

        /// <summary>
        /// relative to Assets/
        /// </summary>
        [Tooltip("to debug location")]
        public string Cache => LocalizationPaths.pathImports + "/" + DisplayName;

		// getter

		public string CacheTxt => Cache + ".txt";
		public string CacheCsv => Cache + ".parser";

		public bool compare(DataSheetTab other)
        {
            if (tabName != other.tabName) return false;
            if (tabUrlId != other.tabUrlId) return false;
            return true;
        }

    }

}
