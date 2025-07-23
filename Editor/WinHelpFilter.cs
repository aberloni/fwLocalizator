using Codice.Client.BaseCommands.FastExport;
using UnityEngine;

namespace fwp.localizator
{

	public class WinHelpFilter
	{
		bool focused = false;

		string _filter = string.Empty;

		public string filter => _filter;
		public bool HasFilter => !string.IsNullOrEmpty(_filter);

		System.Action<string> onValueChange;

		public WinHelpFilter(System.Action<string> onChange)
		{
			onValueChange = onChange;
		}

		/// <summary>
		/// true : match filter
		/// value contains filter
		/// </summary>
		public bool MatchFilter(string value)
		{
			if (!HasFilter) return true; // inactive filter
			return value.Contains(filter);
		}

		public void drawFilterField()
		{
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();

			GUILayout.Label("filter", GUILayout.Width(50f));

			GUI.SetNextControlName("FilterArea");

			string val = GUILayout.TextArea(_filter);

			if (GUILayout.Button("clear", GUILayout.Width(50f)))
			{
				val = string.Empty;
			}

			bool hasFocus = GUI.GetNameOfFocusedControl() == "FilterArea";

			//Debug.Log(focused + "&" + hasFocus);
			if (focused && !hasFocus || val != _filter)
			{
				focused = hasFocus;

				_filter = val;

				//Debug.Log("filter = " + _filter);
				onValueChange?.Invoke(_filter);
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
		}

	}

}