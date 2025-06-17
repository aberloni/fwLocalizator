using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localizator.dialog
{
	/// <summary>
	/// 
	/// dialog UID in spreadsheet must match name of scriptable
	/// 
	/// </summary>
	//[CreateAssetMenu(menuName = LocalizationManager._asset_menu_path + "create dialog data",fileName = "DialogData_", order = LocalizationManager._asset_menu_order)]
	[System.Serializable]
	abstract public class LocaDialogData : ScriptableObject
	{
		public const string dialog_line_number_separator = "-";

		/// <summary>
		/// max lines per scriptable
		/// </summary>
		const int max_fetch_lines = 50;

		public string getDialogUid() => name;

		virtual public bool match(string uid)
		{
			return getDialogUid() == uid;
		}

		/// <summary>
		/// what type is used for lines in this dialogs
		/// for auto generation routine
		/// </summary>
		abstract public System.Type getLineDataType();

		abstract public iDialogLine[] getLines();

		virtual public iDialogLine getNextLine(iDialogLine line)
		{
			var ls = getLines();
			for (int i = 0; i < ls.Length - 1; i++)
			{
				if (ls[i].getLineUid() == line.getLineUid())
				{
					return ls[i + 1];
				}
			}
			return null;
		}

#if UNITY_EDITOR
		virtual public void edUpdateContent()
		{
			edFillDialogLines();

			EditorUtility.SetDirty(this);
		}

		/// <summary>
		/// will generate a list of lines to play for this dialog
		/// it will check if any localization exists for number from 1 to MAX
		/// if there is any : add this UID
		/// </summary>
		public void edFillDialogLines()
		{
			string uid = getDialogUid();

			List<iDialogLine> tmp = new();

			int index = 1;

			var t = getLineDataType();

			Debug.Log("<b>autofill</b> " + name + " (up to : " + max_fetch_lines + ")");

			IsoLanguages iso = IsoLanguages.en;

			while (index < max_fetch_lines)
			{
				// generate a potential UID
				// to check if that key exists in loca file
				string fullId = uid + dialog_line_number_separator + ((index < 10) ? "0" + index : index.ToString());
				Debug.Log($"({iso})?" + fullId);

				if (!LocalizationManager.instance.hasKey(fullId, iso, false))
				{
					// stop
					index = max_fetch_lines;
					Debug.Log("not present in localization files : <b>" + fullId + "</b>");
				}
				else
				{
					Debug.Log("     +<" + t.Name + ">	fid ? " + fullId);

					object line = System.Activator.CreateInstance(t, new object[] { fullId });

					tmp.Add(line as iDialogLine);
				}

				index++;
			}

			edInjectDialogLines(tmp.ToArray());
		}

		/// <summary>
		/// all generated lines
		/// how to inject them
		/// </summary>
		abstract protected void edInjectDialogLines(iDialogLine[] newLines);

		bool dUnfold;
		public void drawLines()
		{
			dUnfold = EditorGUILayout.Foldout(dUnfold, "dialog#" + getDialogUid(), true);
			if (!dUnfold) return;

			var lines = getLines();
			if (lines == null) GUILayout.Label("null lines[]");
			else
			{
				foreach (var line in lines)
				{
					GUILayout.Label(line.getContent());
				}
			}

			if (GUILayout.Button(">", GUILayout.Width(70f)))
			{
				UnityEditor.Selection.activeObject = this;
			}
		}
#endif
	}

}
