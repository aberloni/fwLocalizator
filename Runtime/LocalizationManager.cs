using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localizator
{
	using System.IO;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Manager qui s'occupe de la loca au editor/runtime
	/// 
	/// # NiN info
	/// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
	/// 
	/// pour les espaces insécables : Alt+0160 pour l'écrire dans excel mais \u00A0 dans TMPro.
	/// https://forum.unity.com/threads/why-there-is-no-setting-for-textmesh-pro-ugui-to-count-whitespace-at-the-end.676897/
	/// </summary>
	abstract public class LocalizationManager
	{
		static public bool verbose = false;

		/// for [MenuItem]
		public const string _asset_menu_path = "Localizator/";
		public const string _menu_item_path = "Window/Localizator/";
		public const int _asset_menu_order = 100;

		public const string ppref_language = "ppref_language";

		static public LocalizationManager instance;

		/*
        static public LocalizationManager instance
        {
            get
            {
                if (_instance == null) _instance = new LocalizationManager();
                return _instance;
            }
        }
        */

		/// <summary>
		/// list of reactor candidates to lang change
		/// </summary>
		static public List<iLanguageChangeReact> reacts = new List<iLanguageChangeReact>();

		public const IsoLanguages languageFallback = IsoLanguages.en; // si la langue du system est pas supportée

		public LocalizationFile[] lang_files;

		public LocalizationManager()
		{
			instance = this;

			//CultureInfo.GetCultureInfo(CultureTypes.NeutralCultures)

			IsoLanguages iso = getSavedIsoLanguage(); //au cas ou, set default (fr)

			if (Application.isPlaying) Debug.Log("~Language~ starting language is <b>" + iso + "</b>");

			loadFiles();

			//Debug.Log("loaded " + lang_files.Length + " files");

			//remonte les erreurs
			//checkIntegrity();
		}

		virtual public IsoLanguages[] getSupportedLanguages()
		{
			return new IsoLanguages[]{
				IsoLanguages.en, IsoLanguages.fr, IsoLanguages.de, IsoLanguages.es, IsoLanguages.it
			};
		}

		public bool isIsoLanguageSupported(IsoLanguages iso)
		{
			var sups = getSupportedLanguages();
			for (int i = 0; i < sups.Length; i++)
			{
				if (sups[i] == iso) return true;
			}
			return false;
		}

		public void reloadFiles()
		{
			loadFiles();
		}

		protected void loadFiles()
		{

			List<LocalizationFile> tmp = new List<LocalizationFile>();
			var sups = getSupportedLanguages();
			for (int i = 0; i < sups.Length; i++)
			{
				LocalizationFile file = new LocalizationFile(sups[i]);
				if (file != null && file.isLoaded()) tmp.Add(file);
			}
			lang_files = tmp.ToArray();

		}

		public void applySavedLanguage() => applyLanguage(getSavedIsoLanguage());

		/// <summary>
		/// A apl quand on change la lang
		/// </summary>
		public void applyLanguage(IsoLanguages newLang)
		{
			Debug.Log("<color=cyan>applyLanguage</color> to <b>" + newLang + "</b>!");

			IsoLanguages iso = getSavedIsoLanguage();

			if (!Application.isPlaying)
			{
				reacts.Clear();
				reacts.AddRange(fwp.appendix.AppendixUtils.getCandidates<iLanguageChangeReact>());
			}

			Debug.Log("applying new lang (" + iso + ") to x" + reacts.Count + " reacts");

			for (int i = 0; i < reacts.Count; i++)
			{
				reacts[i].onLanguageChange(iso.ToString());
			}

		}

		public LocalizationFile getFileByLang(IsoLanguages lang)
		{
			for (int i = 0; i < lang_files.Length; i++)
			{
				//debug, NOT runtime, to be sure content is updated
				if (!Application.isPlaying) lang_files[i].debugRefresh();

				if (lang_files[i].iso == lang)
				{
					return lang_files[i];
				}
			}
			return null;
		}

		protected void checkIntegrity()
		{

			for (int i = 0; i < lang_files.Length; i++)
			{
				for (int j = 0; j < lang_files.Length; j++)
				{
					if (lang_files[i].compare(lang_files[j]))
					{
						Debug.LogError("Issue comparing " + lang_files[i].iso + " VS " + lang_files[j].iso);
					}
				}
			}

		}

		public LocalizationFile getCurrentLangFile()
		{
#if UNITY_EDITOR
			loadFiles();
#endif

			string lang = getSavedIsoLanguage().ToString();
			LocalizationFile file = getLangFileByLangLabel(lang);

			if (file == null)
			{
				Debug.LogWarning(" !!! <color=red>no file</color> for current lang : " + lang);
				Debug.LogWarning(" !!! <color=red>this needs to be fixed before release</color> !!! ");

				IsoLanguages iso = getSystemLanguageToIso();
				Debug.LogWarning(" DEBUG <b>force</b> switching lang to '" + iso + "'");
				setSavedLanguage(iso);

				file = getLangFileByLangLabel(lang);
			}

			Debug.Assert(file != null, "file  " + lang + " should be assigned at this point ...");

			return file;
		}

		protected LocalizationFile getLangFileByLangLabel(string label)
		{
			for (int i = 0; i < lang_files.Length; i++)
			{
				if (lang_files[i].iso.ToString() == label) return lang_files[i];
			}
			Debug.LogWarning(" !!! <color=red>no file</color> for current lang : " + label);
			return null;
		}

		/// <summary>
		/// tries a getContent
		/// if fails return default value (en ?)
		/// </summary>
		public string getContentSafe(string id, bool warning = false)
			=> getContentSafe(id, getSavedIsoLanguage(), warning);

		public string getContentSafe(string id, IsoLanguages lang, bool warning = false)
		{
			string output = getContent(id);
			if (output.Length <= 0)
			{
				output = getContent(id, languageFallback, warning);
			}

			if (output.Length <= 0)
			{
				Debug.LogError("could not safe get content : " + id);
				return string.Empty;
			}

			return output;
		}

		/// <summary>
		/// natural flow (using ppref ios)
		/// </summary>
		public string getContent(string id, bool warning = false)
		{
			return getContent(id, getSavedIsoLanguage(), warning);
		}

		/// <summary>
		/// with specific iso given
		/// </summary>
		public string getContent(string id, IsoLanguages iso, bool warning = false)
		{
			if(string.IsNullOrEmpty(id))
			{
				log("empty id given to get content");
				return "[empty UID]";
			}

			LocalizationFile file = instance.getFileByLang(iso);
			Debug.Assert(file != null, "no file found for language : " + iso);

			return file.getContentById(id, warning);
		}

		public void nextLanguage()
		{
			IsoLanguages cur = getSavedIsoLanguage();

			int supportIndex = -1;
			var sups = instance.getSupportedLanguages();
			for (int i = 0; i < sups.Length; i++)
			{
				if (cur == sups[i])
				{
					supportIndex = i;
				}
			}

			Debug.Assert(supportIndex > -1, "unsupported language ?");

			supportIndex++;

			if (supportIndex >= sups.Length)
			{
				supportIndex = 0;
			}

			cur = sups[supportIndex];

			log("next language is : " + cur + " / " + sups.Length, this);

			setSavedLanguage(cur, true);
		}


		static IsoLanguages stringToIso(string lang)
		{
			string[] nms = Enum.GetNames(typeof(IsoLanguages));
			for (int i = 0; i < nms.Length; i++)
			{
				if (nms[i] == lang) return (IsoLanguages)i;
			}

			Debug.LogError("nope ; using fallback language");

			return languageFallback;
		}

		/// <summary>
		/// in : language
		/// out : label of language
		/// </summary>
		public string isoToLabel(IsoLanguages lang)
		{
			return getContent("menu_" + lang.ToString());
		}

		public void setSavedLanguage(IsoLanguages iso, bool applySwap = false)
		{
			setLanguage(iso);

			if (applySwap) applyLanguage(iso); // apply
		}

		protected void setLanguage(IsoLanguages iso)
		{

#if UNITY_EDITOR
			EditorPrefs.SetInt(ppref_language, (int)iso);
#endif
			//LabySaveManager.getStream().setOption(LANG_PREFIX, (float)iso); // save

			//how to save
			//...

		}

		protected IsoLanguages loadLanguage()
		{
			int idx = -1;

#if UNITY_EDITOR
			idx = EditorPrefs.GetInt(ppref_language, -1);
#endif

			if (idx >= 0) return (IsoLanguages)idx;
			return getSystemLanguageToIso();
		}

		/// <summary>
		/// on SWITCH platform there is a specific setup for this to work
		/// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
		/// none defined language in player settings won't work
		/// </summary>
		static public IsoLanguages getSystemLanguageToIso()
		{
			SystemLanguage langDefault = Application.systemLanguage;

#if loca_en
    langDefault = SystemLanguage.English;
#elif loca_fr
    langDefault = SystemLanguage.French;
#elif local_zh
    langDefault = SystemLanguage.Chinese;
#endif

			IsoLanguages lang = sysToIso(langDefault);
			return lang;
		}

		/// <summary>
		/// uses sys language as default
		/// </summary>
		public IsoLanguages getSavedIsoLanguage()
		{

			//default value
			IsoLanguages lang = loadLanguage();

			//how to load
			//IsoLanguages lang = (IsoLanguages)LabySaveManager.getStream().getOption(LANG_PREFIX, (int)sysToIso(langDefault));

			if (!isIsoLanguageSupported(lang))
			{
				logw($"{lang} not supported, fallback to system", this);

				lang = getSystemLanguageToIso(); // sys OR fallback if sys is not supported
			}

			return lang;
		}

		/// <summary>
		/// https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.twoletterisolanguagename?view=net-5.0
		/// </summary>
		public static IsoLanguages sysToIso(SystemLanguage sys)
		{
			switch (sys)
			{
				case SystemLanguage.English: return IsoLanguages.en;
				case SystemLanguage.French: return IsoLanguages.fr;
				case SystemLanguage.German: return IsoLanguages.de;
				case SystemLanguage.Italian: return IsoLanguages.it;
				case SystemLanguage.Chinese: return IsoLanguages.zh;
				case SystemLanguage.Portuguese: return IsoLanguages.po;
				case SystemLanguage.Spanish: return IsoLanguages.es;
				default:
					Debug.LogWarning("language " + sys + " is not supported ; returning system");
					break;
			}

			return languageFallback;
		}

		static public void log(string content, object target = null)
		{
			if (!verbose)
				return;

			Debug.Log("(Loca) "
				+ ((target != null) ? target.GetType().ToString() : string.Empty)
				+ "     "
				+ content, target as UnityEngine.Object);
		}

		static public void logw(string content, object target)
		{
			Debug.LogWarning("(Loca) "
				+ ((target != null) ? target.GetType().ToString() : string.Empty)
				+ "     "
				+ content, target as UnityEngine.Object);
		}
	}

}
