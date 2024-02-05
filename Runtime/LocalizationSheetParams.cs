using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ColumnLetter
{
    A, B, C, D, E, F, G, H, I, J, K, L
}

[System.Serializable]
public struct LocalizationSheetParams
{
    public ColumnLetter uidColumn;
    public int langLineIndex;
}
