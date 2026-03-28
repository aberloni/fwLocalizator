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
		public void Draw(LocalizationManager LManager);
	}

	/// <summary>
	/// base structure of localization window
	/// 	=> spreadsheets import
	/// 	=> dialogs
	/// 
	/// </summary>
	abstract public class WinEdLocaScaffold<TManager> : EditorWindow where TManager : LocalizationManager
	{
		protected TManager LManager;

		protected WinHelpFilter filter = null;

		int tabSelected = 0;
		iLocaTab[] tabs;
		GUIContent[] guiTabs;

		/// <summary>
		/// sealed
		/// </summary>
		void OnEnable()
		{
			LManager = GenerateManager();
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
			guiTabs = new GUIContent[tabs.Length];
			for (int i = 0; i < tabs.Length; i++)
			{
				guiTabs[i] = new GUIContent(tabs[i].GetTabName());
			}
		}

		abstract public iLocaTab[] GenerateTabs();

		/// <summary>
		/// select type of loca manager to generate
		/// </summary>
		abstract public TManager GenerateManager();

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
			LocalizationManager.Verbose = EditorGUILayout.Toggle("verbose", LocalizationManager.Verbose);
			drawHeader();
			drawTabs();
			globalScroll = GUILayout.BeginScrollView(globalScroll);
			draw();
			GUILayout.EndScrollView();
		}

		virtual protected void refresh(bool forced = false)
		{ }

		virtual protected void drawHeader()
		{
			if (UtilStyles.drawSectionTitle(getWindowTitle(), 15, 15))
			{
				Debug.Log("> title.refresh");
				refresh(true);
			}
		}

		void drawTabs()
		{
			if (guiTabs.Length <= 0) return;

			int _tab = GUILayout.Toolbar(tabSelected, guiTabs);
			if (_tab != tabSelected)
			{
				tabSelected = _tab;
			}
		}

		virtual protected void draw()
		{
			if(tabSelected >= 0) tabs[tabSelected].Draw(LManager);
		}


	}

}