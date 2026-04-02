using UnityEngine;
using System.Collections.Generic;

namespace fwp.localizator
{

	/// <summary>
	/// manager of languages
	/// </summary>
	public class LanguagesMind : LocalizationMind
	{
		public const string ppref_language = "ppref_language";

		public override string ToString() => base.ToString() + "|" + getLanguageFallback() + "|x" + getSupportedLanguages().Length;

		/// <summary>
		/// list of reactor candidates to lang change
		/// interface should sub/unsub to this to get reaction event
		/// </summary>
		static public List<iLanguageChangeReact> reacts = new List<iLanguageChangeReact>();

		public LanguagesMind()
		{
			ReplaceMind<LanguagesMind>(this);
		}

		/// <summary>
		/// fallback is when trying to get a safe localization
		/// what language cannot fail ?
		/// </summary>
		virtual public IsoLanguages getLanguageFallback() => IsoLanguages.en;

		public bool isIsoLanguageSupported(IsoLanguages iso)
		{
			var sups = getSupportedLanguages();
			for (int i = 0; i < sups.Length; i++)
			{
				if (sups[i] == iso) return true;
			}
			return false;
		}

		virtual public IsoLanguages[] getSupportedLanguages()
		{
			List<IsoLanguages> isos = new();
			for (int i = 0; i < System.Enum.GetValues(typeof(IsoLanguages)).Length; i++)
			{
				isos.Add((IsoLanguages)i);
			}
			return isos.ToArray();
		}

		public void applySavedLanguage() => applyLanguage(getIso());

		/// <summary>
		/// A apl quand on change la lang
		/// </summary>
		public void applyLanguage(IsoLanguages newLang)
		{
			Debug.Log("<color=cyan>applyLanguage</color> to <b>" + newLang + "</b>!");

			IsoLanguages iso = getIso();

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

		public void nextLanguage()
		{
			IsoLanguages cur = getIso();

			int supportIndex = -1;
			var sups = getSupportedLanguages();
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

			setIso(cur, true);
		}

		/// <summary>
		/// apply lang where it should be localy stored
		/// </summary>
		virtual protected void setLanguage(IsoLanguages iso)
		{
			if (Application.isEditor)
			{
#if UNITY_EDITOR
				UnityEditor.EditorPrefs.SetInt(ppref_language, (int)iso);
#endif
			}
			else
			{
				PlayerPrefs.SetInt(ppref_language, (int)iso);
			}
		}

		/// <summary>
		/// extract lang from where it's stored
		/// </summary>
		virtual protected IsoLanguages getLanguage()
		{
			int idx = (int)getLanguageFallback();
			if (Application.isEditor)
			{
#if UNITY_EDITOR
				idx = UnityEditor.EditorPrefs.GetInt(ppref_language, -1);
#endif
			}
			else
			{
				idx = PlayerPrefs.GetInt(ppref_language, (int)Languages.getLanguageFiltered());
			}

			return (IsoLanguages)idx;
		}

		public void setIso(IsoLanguages iso, bool applySwap = false)
		{
			setLanguage(iso);
			if (applySwap) applyLanguage(iso); // apply
		}

		/// <summary>
		/// uses sys language as default
		/// </summary>
		public IsoLanguages getIso()
		{
			//default value
			IsoLanguages lang = getLanguage();

			//how to load
			//IsoLanguages lang = (IsoLanguages)LabySaveManager.getStream().getOption(LANG_PREFIX, (int)sysToIso(langDefault));

			if (!isIsoLanguageSupported(lang))
			{
				logw($"{lang} not supported, fallback to system", this);

				lang = getRawSystemLanguageToIso(); // sys OR fallback if sys is not supported
			}

			return lang;
		}

		/// <summary>
		/// on SWITCH platform there is a specific setup for this to work
		/// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
		/// none defined language in player settings won't work
		/// </summary>
		static public IsoLanguages getRawSystemLanguageToIso() => SysToIso(getRawSystemLanguage());

		/// <summary>
		/// raw unity detection
		/// </summary>
		static public SystemLanguage getRawSystemLanguage() => Application.systemLanguage;

		/// <summary>
		/// potential additionnal rules to override sys language
		/// #if loca_en 
		/// </summary>
		virtual public SystemLanguage getLanguageFiltered()
		{
			SystemLanguage langDefault = getRawSystemLanguage();

#if loca_en
			langDefault = SystemLanguage.English;
#endif

			return langDefault;
		}

		/// <summary>
		/// https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.twoletterisolanguagename?view=net-5.0
		/// </summary>
		public static IsoLanguages SysToIso(SystemLanguage sys)
		{
			switch (sys)
			{
				case SystemLanguage.English: return IsoLanguages.en;
				case SystemLanguage.French: return IsoLanguages.fr;
				case SystemLanguage.German: return IsoLanguages.de;
				case SystemLanguage.Italian: return IsoLanguages.it;

				case SystemLanguage.Chinese:
				case SystemLanguage.ChineseSimplified:
					return IsoLanguages.zh_hans;

				case SystemLanguage.ChineseTraditional: return IsoLanguages.zh_hant;

				case SystemLanguage.Portuguese: return IsoLanguages.pt;
				case SystemLanguage.Spanish: return IsoLanguages.es;
				default:
					Debug.LogWarning("language " + sys + " is not supported ; returning system");
					break;
			}

			return getRawSystemLanguageToIso();
		}

		public string stringifySupported
		{
			get
			{
				string ret = "[supported]";
				foreach (var sup in getSupportedLanguages())
				{
					ret += sup + ",";
				}
				ret = ret.Substring(0, ret.Length - 1);
				return ret;
			}
		}
	}

}