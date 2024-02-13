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

        public string url => googleSpreadsheetEditPrefix + tabUrlId;

        [Tooltip("to debug location")]
        public string cache; // relative to Assets/
        public string cacheResources // relative to Resources/
        {
            get
            {
                string path = cache.Substring("Resources/".Length);
                return path.Substring(0, path.LastIndexOf("."));
            }
        }

        public string cacheAsset => "Assets/" + cache; // relative to project

        public string displayName => tabName + "&" + tabUrlId;

        public bool compare(DataSheetTab other)
        {
            if (tabName != other.tabName) return false;
            if (tabUrlId != other.tabUrlId) return false;
            return true;
        }

    }

}
