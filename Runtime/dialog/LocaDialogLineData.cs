using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.dialog
{
	/// <summary>
	/// don't forget to also flag children as serializable
	/// </summary>
	[System.Serializable]
	public class LocaDialogLineData : iDialogLine
	{
		/// <summary>
		/// uid of line to be played
		/// uid to ask localization for the localized string
		/// 
		/// dial-01
		/// dial-02
		/// ...
		/// </summary>
		public string uid;
		public string getLineUid() => uid;

		public LocaDialogLineData(string uid)
		{
			this.uid = uid;
		}

		public string getContent()
		{
			return LocalizationManager.instance.getContent(uid);
		}

#if UNITY_EDITOR
		//FOR DEBUG ONLY
		public string[] previews;

		public void debugUpdatePreview(bool verbose = false)
		{
			List<string> tmp = new List<string>();

			if (verbose)
				Debug.Log("log debug previews @ " + uid);

			var sups = LocalizationManager.instance.getSupportedLanguages();
			foreach (IsoLanguages sup in sups)
			{
				var val = LocalizationManager.instance.getContent(uid, sup);
				tmp.Add(val);

				if (verbose)
					Debug.Log(sup + " => " + val);
			}
			previews = tmp.ToArray();
		}
#endif

	}

}
