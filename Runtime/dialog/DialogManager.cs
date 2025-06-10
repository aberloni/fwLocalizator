using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.dialog
{
	/// <summary>
	/// manage the list of available dialogs
	/// editor : everything
	/// runtime : will load from Resources/
	/// </summary>
	public class DialogManager
	{
		static public bool verbose;

		static public DialogManager instance;

		static public string folderDialogs = "dialogs/";
		static public string resourcesDialogs = "Resources/" + folderDialogs;

		static public string assetDialogs = System.IO.Path.Combine(
			"Assets/", resourcesDialogs);
		static public string sysDialogs = System.IO.Path.Combine(
			Application.dataPath, resourcesDialogs);

		/// <summary>
		/// scriptable objects
		/// </summary>
		public List<LocaDialogData> dialogs = new();

		public DialogManager()
		{
			instance = this;
		}

		public LocaDialogData getDialogInstance(string uid)
		{
			foreach (var d in dialogs)
			{
				if (d == null)
					continue;

				if (d.getDialogUid() == uid)
					return d;
			}
			return null;
		}

		public bool hasDialogInstance(string uid)
		{
			foreach (var d in dialogs)
			{
				if (d.getDialogUid() == uid)
					return true;
			}
			return false;
		}

		public LocaDialogData getDialog(string uid)
		{
			//... resource load ...
			return null;
		}

		virtual protected System.Type getDialogType() => typeof(LocaDialogData);

#if UNITY_EDITOR
		/// <summary>
		/// generate the scriptable object instance
		/// </summary>
		public LocaDialogData createDialog(string uid)
		{
			var inst = ScriptableObject.CreateInstance(getDialogType());

			Debug.Assert(inst != null, "could not create scriptable dialog");

			//string path = DialogManager.sysDialogs;
			string path = DialogManager.assetDialogs;

			// make sure folder exists
			path = generateExportPath(path);

			// add asset at end of path
			path += uid + ".asset";

			Debug.Log("asset path @ " + path);

			UnityEditor.AssetDatabase.CreateAsset(inst, path);
			UnityEditor.AssetDatabase.Refresh();

			Debug.Log("solving content of " + inst);
			LocaDialogData data = inst as LocaDialogData;

			Debug.Assert(data != null, data.GetType() + " is not a dialog data ?");
			reactDialogCreation(data);

			UnityEditor.EditorUtility.SetDirty(inst);

			//mgrDialog.refresh();
			UnityEditor.Selection.activeObject = inst;

			return data;
		}

		virtual protected void reactDialogCreation(LocaDialogData data)
		{
			data.edFillDialogLines();
		}

		/// <summary>
		/// in : (Assets/) some/path (/)
		/// out : filled path
		/// </summary>
		string generateExportPath(string path)
		{
			const string asset_path = "Assets";

			if (!path.StartsWith(asset_path))
				path = System.IO.Path.Combine(asset_path, path);

			// remove last  /
			if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

			var split = path.Split("/");

			Debug.Log(path + " => " + split.Length);

			string progressivePath = asset_path + "/";
			for (int i = 1; i < split.Length; i++)
			{
				string tarPath = progressivePath + "/" + split[i];
				if (!UnityEditor.AssetDatabase.IsValidFolder(tarPath)) // Assets/Data/Dialogs
				{
					Debug.LogWarning("creating : " + tarPath);
					var guid = UnityEditor.AssetDatabase.CreateFolder(progressivePath, split[i]); // Assets/Data & Dialogs
					Debug.Log(guid);
				}
				else Debug.Log("OK : " + tarPath);


				progressivePath = tarPath;
			}

			return path + "/";
		}
#endif

	}
}
