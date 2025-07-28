using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using fwp.localizator.editor;

namespace fwp.localizator.dialog.editor
{
	abstract public class WinEdDialogs : WinEdLocaScaffold
	{
		//[MenuItem("Window/Localizator/(win) dialogs")]
		//static void init() => EditorWindow.GetWindow(typeof(WinEdDialogs));

		public class LocaAssoc
		{
			public LocaDialogData dialog;
			//public string duid;
		}

		/// <summary>
		/// all duid matching a dialog
		/// </summary>
		public Dictionary<string, LocaAssoc> dialogs = null;

		/// <summary>
		/// dialogs without matching duid
		/// </summary>
		public LocaDialogData[] dialogsIssues = null;

		Vector2 scrollTabDialogs;

		protected override string getTitle() => DialogManager.instance.GetType().Name;

		IsoLanguages Iso => LocalizationManager.instance != null ? LocalizationManager.instance.getSavedIsoLanguage() : IsoLanguages.en;

		protected override void draw()
		{
			base.draw();

			if (DialogManager.instance == null)
			{
				GUILayout.Label("no manager");
				return;
			}

			filter.drawFilterField();

			if (LocalizationManager.instance == null)
			{
				GUILayout.Label("no loca manager?");
				return;
			}

			GUILayout.Label($"using lang : {Iso}");

			scrollTabDialogs = GUILayout.BeginScrollView(scrollTabDialogs);

			drawGlobalDialogButtons();

			// all possible dialogs (from localiz)
			drawFoldLocalizationFiles();

			GUILayout.Space(10f);
			// all existing scriptables
			drawFoldScriptableFiles();

			GUILayout.Space(10f);
			// all existing scriptables without matching uids from loca
			drawIssues();

			GUILayout.EndScrollView();
		}

		override protected void refresh(bool verbose = false)
		{
			base.refresh(verbose);

			findDialogsUids(); // refresh
		}

		/// <summary>
		/// regen all data
		/// </summary>
		void findDialogsUids()
		{
			var mgr = LocalizationManager.instance;
			if (mgr == null) return;

			string _filter = filter != null && filter.HasFilter ? filter.filter : null;

			// get french (default)
			var file = mgr.getFileByLang(Iso);
			if (file == null)
			{
				Debug.LogWarning($"no {Iso} file ?");
				return;
			}

			// fetching all possible UIDs (from trad file)

			List<string> tmp = new List<string>();
			var lines = file.getLines(); // lines of target translation file

			foreach (var l in lines)
			{
				// split UID=VAL
				var uid = l.Split("=")[0];

				// only keep uid with autofill numbering
				if (!uid.Contains("-")) continue;

				// split UID-{NUM}
				uid = uid.Substring(0, uid.LastIndexOf("-"));

				if (!string.IsNullOrEmpty(_filter) && !uid.Contains(_filter)) continue;

				if (!tmp.Contains(uid))
				{
					if (DialogManager.verbose) Debug.Log("+" + uid);
					tmp.Add(uid);
				}
			}

			string[] dialogsUids = tmp.ToArray();
			if (DialogManager.verbose) Debug.Log($"      -> solved x{dialogsUids.Length} uids dialogs");

			if (dialogs == null) dialogs = new(); // regen dico
			else dialogs.Clear();

			foreach (var d in dialogsUids)
			{
				dialogs.Add(d, new LocaAssoc());
			}

			LocaDialogData[] scriptables = fetchScriptablesEditor(_filter);
			List<LocaDialogData> issues = new();
			foreach (var s in scriptables)
			{
				if (dialogs.ContainsKey(s.name))
				{
					dialogs[s.name].dialog = s;
				}
				else
				{
					issues.Add(s);
				}
			}
			dialogsIssues = issues.ToArray();
		}

		public bool hasDialogInstance(string dialogUid) => getDialog(dialogUid) != null;

		/// <summary>
		/// don't call often when there are a lot of dialogs
		/// </summary>
		public LocaDialogData getDialog(string uid)
		{
			if (dialogs.ContainsKey(uid)) return dialogs[uid].dialog;
			return null;
		}

		void drawIssues()
		{
			bool fold = drawFoldout("issues", "dialog_issues");
			if (!fold) return;

			GUILayout.Label("all dialogs scriptable without matching DUID in translation file");

			if (dialogsIssues == null || dialogsIssues.Length <= 0)
				return;

			foreach (var d in dialogsIssues)
			{
				if (d == null) continue;

				d.drawLines();
			}
		}

		void drawFoldScriptableFiles()
		{
			if (dialogs == null || dialogs.Count <= 0)
				return;

			bool fold = drawFoldout("generated scriptables", "dialog_scriptables");
			if (!fold) return;

			GUILayout.Label("all dialogs scriptable existing in resources");

			foreach (var d in dialogs)
			{
				if (d.Value.dialog == null) continue;
				d.Value.dialog.drawLines();
			}
		}

		public void drawFoldLocalizationFiles()
		{
			if (dialogs == null || dialogs.Count <= 0)
				return;

			bool fold = drawFoldout("loca dialogs UIDs x" + dialogs.Count, "dialog_locas");
			if (!fold) return;

			GUILayout.Label($"all DUID found in translation file {LocalizationManager.instance.getSavedIsoLanguage()}");

			bool dirty = false;
			foreach (var kp in dialogs)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(kp.Key);

				if (kp.Value.dialog == null)
				{
					if (GUILayout.Button("create", btnM))
					{
						kp.Value.dialog = DialogManager.instance.createDialog(kp.Key);

						dirty = true;
					}
				}

				if (kp.Value.dialog != null)
				{
					if (GUILayout.Button("update", btnM))
					{
						Debug.Log("dialog.update " + kp.Value.dialog.name, kp.Value.dialog);

						kp.Value.dialog.edUpdateContent();
						UnityEditor.Selection.activeObject = kp.Value.dialog;
					}
					if (GUILayout.Button(" > ", btnS))
					{
						UnityEditor.Selection.activeObject = kp.Value.dialog;
					}
				}

				GUILayout.EndHorizontal();
			}

			if (dirty) refresh();
		}

		/// <summary>
		/// global routines
		/// </summary>
		void drawGlobalDialogButtons()
		{
			bool _generate = false;
			bool _update = false;

			bool dirty = false;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("generate missing dialogs")) _generate = true;
			if (GUILayout.Button("update dialogs")) _update = true;
			GUILayout.EndHorizontal();

			if (!_generate && !_update)
				return;

			EditorUtility.DisplayProgressBar("process", "fetching...", 0f);

			int cnt = 0;
			foreach (var kp in dialogs)
			{
				var d = kp.Value.dialog;

				if (EditorUtility.DisplayCancelableProgressBar("processing x" + dialogs.Count, "#" + cnt + ":" + d + "...", (cnt * 1f) / (dialogs.Count * 1f)))
				{
					break;
				}

				if (d != null)
				{
					if (_update)
					{
						d.edUpdateContent();
						dirty = true;
					}
				}
				else if (_generate)
				{
					d = DialogManager.instance.createDialog(kp.Key);
					dirty = true;
				}

				cnt++;
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

	}

}