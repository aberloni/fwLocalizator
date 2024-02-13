using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationPaths : MonoBehaviour
{
    static public string langExt = "txt";
    static public string langExtDot = "." + langExt;

    static public string folderLocalization = "localization/";
    static public string folderLangs = "languages/";

    static public string exportPathBase = "Resources/" + folderLocalization;

    static public string pathImports = exportPathBase + "imports/";
    static public string pathCsvs = exportPathBase + "csvs/";

    static public string pathLangs = exportPathBase + folderLangs;

    static public string sysImports => Path.Combine(Application.dataPath, pathImports);
    static public string sysCsvs => Path.Combine(Application.dataPath, pathCsvs);
    static public string sysLangs => Path.Combine(Application.dataPath, pathLangs);
}

[System.Serializable]
public class LocalizationPathOverride
{

    public string exportPathBase = LocalizationPaths.exportPathBase;
    public string pathImports = LocalizationPaths.pathImports;
    public string pathCsvs = LocalizationPaths.pathCsvs;
    public string pathLangs = LocalizationPaths.pathLangs;

}