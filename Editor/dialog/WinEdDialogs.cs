using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using fwp.localizator.editor;

namespace fwp.localizator.dialog.editor
{
	public class WinEdDialogs : WinEdLocaScaffold
	{
		[MenuItem("Window/Localizator/(win) dialogs")]
		static void init() => EditorWindow.GetWindow(typeof(WinEdDialogs));

		/// <summary>
		/// scriptable objects
		/// </summary>
		public LocaDialogData[] dialogs = null;

		/// <summary>
		/// all possible dialogs from localization
		/// </summary>
		public string[] dialogsUids = null;

		Vector2 scrollTabDialogs;

		override protected void generateManager() => new DialogManager();

		protected override void OnEnable()
		{
			base.OnEnable();
			refresh();
		}

		protected override void OnFocus()
		{
			base.OnFocus();
			findDialogsUids();
		}

		protected override string getTitle() => DialogManager.instance.GetType().Name;

		protected override void draw()
		{
			base.draw();

			if (DialogManager.instance == null)
			{
				GUILayout.Label("no manager");
				return;
			}

			filter.drawFilterField();

			scrollTabDialogs = GUILayout.BeginScrollView(scrollTabDialogs);

			drawFoldLocalizationFiles(filter.filter);
			drawFoldScriptableFiles(filter.filter);

			GUILayout.EndScrollView();
		}

		override protected void refresh(bool verbose = false)
		{
			base.refresh(verbose);
			dialogs = fetchDialogObjects();

			if (verbose) Debug.Log($"      -> solved x{dialogs.Length} scriptable dialogs");
			findDialogsUids();
		}

		public void findDialogsUids()
		{
			var mgr = LocalizationManager.instance;
			if (mgr == null)
				return;

			var _iso = mgr.getSavedIsoLanguage();

			// get french (default)
			var file = mgr.getFileByLang(_iso);
			if (file == null)
			{
				Debug.LogWarning($"no {_iso} file ?");
				return;
			}

			// fetching all possible UIDs (from trad file)

			List<string> tmp = new List<string>();
			var lines = file.getLines();

			foreach (var l in lines)
			{
				// split UID=VAL
				var uid = l.Split("=")[0];

				// only keep uid with autofill numbering
				if (!uid.Contains("-")) continue;

				// split UID-{NUM}
				uid = uid.Substring(0, uid.LastIndexOf("-"));

				if (!tmp.Contains(uid))
				{
					if (DialogManager.verbose) Debug.Log("+" + uid);
					tmp.Add(uid);
				}
			}

			dialogsUids = tmp.ToArray();

			if (DialogManager.verbose) Debug.Log($"      -> solved x{dialogsUids.Length} uids dialogs");
		}

		public bool hasDialogInstance(string dialogUid)
			=> getDialog(dialogUid) != null;

		public LocaDialogData getDialog(string uid)
		{
			if (dialogs == null) return null;

			foreach (var d in dialogs)
			{
				if (d == null) continue;
				if (d.match(uid)) return d as LocaDialogData;
			}
			return null;
		}

		public void drawFoldScriptableFiles(string filter)
		{
			if (dialogs == null)
				return;

			bool fold = drawFoldout("in :   scriptables x" + dialogs.Length, "scriptables");
			if (!fold) return;

			foreach (var d in dialogs)
			{
				if (d == null) continue;
				if (!string.IsNullOrEmpty(filter) && !d.getDialogUid().Contains(filter)) continue;

				d.drawLines();
			}
		}

		public void drawFoldLocalizationFiles(string filter)
		{
			if (dialogsUids == null)
				return;

			bool fold = drawFoldout("in :   loca dialogs UIDs x" + dialogsUids.Length, "loca");
			if (!fold) return;

			drawGlobalDialogButtons();

			bool dirty = false;
			foreach (var d in dialogsUids)
			{
				if (d == null) continue;
				if (!string.IsNullOrEmpty(filter) && !d.Contains(filter)) continue;

				GUILayout.BeginHorizontal();
				GUILayout.Label(d);
				var dial = getDialog(d);

				if (dial == null)
				{
					if (GUILayout.Button("create", btnM))
					{
						dial = DialogManager.instance.createDialog(d);

						dirty = true;
					}
				}

				if (dial != null)
				{
					if (GUILayout.Button("update", btnM))
					{
						Debug.Log("dialog.update " + dial.name, dial);

						dial.edUpdateContent();
						UnityEditor.Selection.activeObject = dial;
					}
					if (GUILayout.Button(" > ", btnS))
					{
						UnityEditor.Selection.activeObject = dial;
					}
				}

				GUILayout.EndHorizontal();
			}

			if (dirty) refresh();
		}

		void drawGlobalDialogButtons()
		{
			bool _generate = false;
			bool _update = false;

			bool dirty = false;

			if (GUILayout.Button("generate all missing dialogs")) _generate = true;
			if (GUILayout.Button("update all dialogs")) _update = true;

			if (!_generate && !_update)
				return;

			EditorUtility.DisplayProgressBar("process", "fetching...", 0f);

			for (int i = 0; i < dialogsUids.Length; i++)
			{
				var d = dialogsUids[i];

				if (EditorUtility.DisplayCancelableProgressBar("processing x" + dialogsUids.Length, "#" + i + ":" + d + "...", (i * 1f) / (dialogsUids.Length * 1f)))
				{
					i = dialogsUids.Length;
					continue;
				}

				if (hasDialogInstance(d))
				{
					if (_update)
					{
						var dial = getDialog(d);
						if (dial != null) dial.edUpdateContent();

						dirty = true;
					}
				}
				else if (_generate)
				{
					DialogManager.instance.createDialog(d);
					dirty = true;
				}
			}

			EditorUtility.ClearProgressBar();

			if (!dirty)
			{
				Debug.Log("dialog process : nothing was done");
			}

			refresh();
		}

#if UNITY_EDITOR
		static LocaDialogData[] fetchScriptablesEditor(string filter = null)
		{
			var type = typeof(LocaDialogData);

			var all = UnityEditor.AssetDatabase.FindAssets("t:" + type.Name);
			List<LocaDialogData> output = new();
			for (int i = 0; i < all.Length; i++)
			{
				Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(
					UnityEditor.AssetDatabase.GUIDToAssetPath(all[i]), type);
				var so = obj as LocaDialogData;
				if (so == null) continue;
				if (!string.IsNullOrEmpty(filter) && !so.name.Contains(filter)) continue;
				output.Add(so);
			}
			return output.ToArray();
		}
#endif

		static public LocaDialogData[] fetchDialogObjects(string filter = null)
		{
			LocaDialogData[] ss = null;

#if UNITY_EDITOR
			ss = fetchScriptablesEditor(filter);
#endif

			return ss;
		}
	}

}