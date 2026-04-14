using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

namespace fwp.localizator.editor
{
	public interface iLocaTab
	{
		public string GetTabName();
		public void Refresh(bool hard);
		public void Draw();
	}

	/// <summary>
	/// base structure of localization window
	/// 	=> spreadsheets import
	/// 	=> dialogs
	/// 
	/// </summary>
	abstract public class WinEdLocaScaffold : EditorWindow
	{
		protected WinHelpFilter filter = null;

		int tabSelected = 0;
		iLocaTab[] tabs;
		GUIContent[] guiTabs;

		/// <summary>
		/// replace default minds
		/// </summary>
		virtual protected void ReplaceMinds()
		{ }

		/// <summary>
		/// sealed
		/// </summary>
		void OnEnable()
		{
			LocalizatorMinds.InitMinds();
			ReplaceMinds();
			onEnable();
		}

		/// <summary>
		/// enable
		/// </summary>
		virtual protected void onEnable()
		{
			if (filter == null)
			{
				filter = new((filter) =>
				{
					Debug.Log("filter >> " + filter);
					refresh();
				});
			}

			tabs = GenerateTabs();
			if(tabs != null)
			{

				guiTabs = new GUIContent[tabs.Length];
				for (int i = 0; i < tabs.Length; i++)
				{
					guiTabs[i] = new GUIContent(tabs[i].GetTabName());
				}
			}
		}

		abstract public iLocaTab[] GenerateTabs();

		/// <summary>
		/// sealed
		/// </summary>
		void OnFocus() => onFocus();

		virtual protected void onFocus()
		{
			refresh();
		}

		virtual protected string getWindowTitle() => "Localization";

		Vector2 globalScroll;
		private void OnGUI()
		{
			drawHeader();
			GUILayout.Space(10);

			drawTabs();
			GUILayout.Space(10);

			globalScroll = GUILayout.BeginScrollView(globalScroll);
			draw();
			GUILayout.EndScrollView();
		}

		virtual protected void refresh(bool hard = false)
		{ }

		virtual protected void drawHeader()
		{
			if (UtilStyles.drawSectionTitle(getWindowTitle(), 15, 15))
			{
				Debug.Log("> title.refresh");
				refresh(true);
			}

			LocalizationMind.Verbose = EditorGUILayout.Toggle("verbose", LocalizationMind.Verbose);
			if (LocalizationMind.Verbose)
			{
				GUILayout.Label("Languages:" + LocalizatorMinds.Languages);
				GUILayout.Label("Sheets:" + LocalizatorMinds.Sheets);
				GUILayout.Label("Dialogs:" + LocalizatorMinds.Dialogs);
			}

		}

		void drawTabs()
		{
			if (guiTabs == null || guiTabs.Length <= 0) return;

			int _tab = GUILayout.Toolbar(tabSelected, guiTabs);
			if (_tab != tabSelected)
			{
				tabSelected = _tab;
			}
		}

		virtual protected void draw()
		{
			if (tabs == null || tabs.Length <= 0) return;
			if (tabSelected >= 0) tabs[tabSelected].Draw();
		}


	}

}