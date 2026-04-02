using System.Collections.Generic;
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
	abstract public class Localization
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
		static public string getContentSafe(string id)
		{
			string output = GetContent(id);
			if (output.Length <= 0)
			{
				output = GetContent(id, LanguagesMind.GetLanguageFallback());
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
			=> GetContent(id, LocalizationMind.Languages.getIso());

		/// <summary>
		/// with specific iso given
		/// </summary>
		static public string GetContent(string id, IsoLanguages iso)
		{
			if (string.IsNullOrEmpty(id))
			{
				LocalizationMind.logw("empty id given to get content");
				return "[empty UID]";
			}

			if (LocalizationMind.Sheets == null)
			{
				LocalizationMind.logw("can't : no mind.sheets");
				return null;
			}

			LocalizationFile file = LocalizationMind.Sheets.getFileByLang(iso);
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
		static public bool HasKey(string key, bool ignoreDigits = true) => HasKey(key, LocalizationMind.Languages.getIso(), ignoreDigits);
		static public bool HasKey(string key, IsoLanguages iso, bool ignoreDigits = true)
		{
			LocalizationFile file = LocalizationMind.Sheets.getFileByLang(iso);
			return file.hasId(key, ignoreDigits);
		}

	}

}
