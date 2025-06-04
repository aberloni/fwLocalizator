using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ColumnLetter
{
    A, B, C, D, E, F, G, H, I, J, K, L
}

[System.Serializable]
public class LocalizationSheetParams
{
    /// <summary>
    /// what column in the spreadsheet is UID of a line located
    /// </summary>
    public ColumnLetter uidColumn = ColumnLetter.D;

    /// <summary>
    /// quantity of lines to ignore from top of ssheet
    /// </summary>
    public int langLineIndex = 3;
}
