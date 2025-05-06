using Codice.Client.BaseCommands.FastExport;
using UnityEngine;

namespace fwp.localizator
{

	public class WinHelpFilter
	{
		string _filter = string.Empty;

		public string filter => _filter;
		public bool HasFilter => !string.IsNullOrEmpty(_filter);

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
			_filter = GUILayout.TextArea(_filter);

			if (GUILayout.Button("clear", GUILayout.Width(50f)))
			{
				_filter = string.Empty;
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
		}

	}

}