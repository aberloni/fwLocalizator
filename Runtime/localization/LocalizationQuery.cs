using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

namespace fwp.localizator
{
	/// <summary>
	/// Manager qui s'occupe de la loca au editor/runtime
	/// 
	/// # NiN info
	/// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
	/// 
	/// pour les espaces insécables : Alt+0160 pour l'écrire dans excel mais \u00A0 dans TMPro.
	/// https://forum.unity.com/threads/why-there-is-no-setting-for-textmesh-pro-ugui-to-count-whitespace-at-the-end.676897/
	/// </summary>
	abstract public class LocalizationQuery
	{
		/// for [MenuItem]
		public const string _asset_menu_path = "Localizator/";
		public const string _menu_item_path = "Window/Localizator/";
		public const int _asset_menu_order = 100;

		/// <summary>
		/// tries a getContent
		/// fail : return content using fallback language
		/// fallback fail : error
		/// </summary>
		static public string GetContentSafe(string id)
		{
			if (!LocalizatorMinds.CanLocalize()) return null;

			// normal get
			string output = GetContent(id);

			// failed, try again with fallback
			if (output.Length <= 0)
			{
				output = GetContent(id, LocalizatorMinds.Languages.getIsoSafeLanguage());
			}

			if (output.Length <= 0)
			{
				Debug.LogError("could not SAFE get content : " + id);
				return string.Empty;
			}

			return output;
		}

		/// <summary>
		/// natural flow (using ppref ios)
		/// </summary>
		static public string GetContent(string id)
		{
			if (!LocalizatorMinds.CanLocalize()) return null;

			return GetContent(id, LocalizatorMinds.Languages.getLanguage());
		}

		/// <summary>
		/// with specific iso given
		/// </summary>
		static public string GetContent(string id, IsoLanguages iso)
		{
			if (!LocalizatorMinds.CanLocalize()) return null;

			if (string.IsNullOrEmpty(id))
			{
				LocalizationMind.logw("empty id given to get content");
				return "[empty UID]";
			}

			if (LocalizatorMinds.Sheets == null)
			{
				LocalizationMind.logw("can't : no mind.sheets");
				return null;
			}

			LocalizationFile file = LocalizatorMinds.Sheets.getFileByLang(iso);
			if (file == null)
			{
				LocalizationMind.logw("can't : no file for iso:" + iso);
				return null;
			}

			string ret = file.getContentById(id);
			return ret;
		}

		/// <summary>
		/// check if localization file has matching key (strict comparison)
		/// </summary>
		static public bool HasKey(string key, bool ignoreDigits = true)
		{
			if (!LocalizatorMinds.CanLocalize()) return false;

			if (LocalizatorMinds.Languages == null)
			{
				LocalizationMind.logw("can't : no mind.lang");
				return false;
			}
			return HasKeyByLanguage(key, LocalizatorMinds.Languages.getLanguage(), ignoreDigits);
		}

		static public bool HasKeyByLanguage(string key, IsoLanguages iso, bool ignoreDigits = true)
		{
			if (!LocalizatorMinds.CanLocalize()) return false;
			LocalizationFile file = LocalizatorMinds.Sheets.getFileByLang(iso);
			return file.hasId(key, ignoreDigits);
		}

	}

}
