using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;


namespace fwp.localizator.editor
{

	abstract public class WinEdLocaScaffold : EditorWindow
	{

		protected GUILayoutOption btnXS => GUILayout.Width(50f);
		protected GUILayoutOption btnS => GUILayout.Width(75f);
		protected GUILayoutOption btnM => GUILayout.Width(100f);

		protected WinHelpFilter filter = new();

		Dictionary<string, bool> edFoldout = new Dictionary<string, bool>();

		virtual protected void OnEnable()
		{
			generateManager();
		}

		virtual protected void OnFocus()
		{

		}

		/// <summary>
		/// to override orignal manager
		/// </summary>
		abstract protected void generateManager();

		protected bool drawFoldout(string label, string uid, bool isSection = false)
		{
			bool foldState = false;
			if (edFoldout.ContainsKey(uid))
			{
				foldState = edFoldout[uid];
			}

			bool _state;

			if (isSection)
			{
				_state = EditorGUILayout.Foldout(foldState, label, true, UtilStyles.FoldoutSection(15));
			}
			else
			{
				_state = EditorGUILayout.Foldout(foldState, label, true);
			}

			if (_state != foldState)
			{
				if (!edFoldout.ContainsKey(uid)) edFoldout.Add(uid, false);
				edFoldout[uid] = _state;
			}

			return _state;
		}

	}

}