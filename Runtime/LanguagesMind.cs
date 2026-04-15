using UnityEngine;
using System.Collections.Generic;

namespace fwp.localizator
{

	/// <summary>
	/// manager of languages
	/// </summary>
	public class LanguagesMind : LocalizationMind
	{
		public const string ppref_iso_language = "ppref_iso_language";

		public override string ToString() => base.ToString() + "|" + getIsoSafeLanguage() + "|x" + getSupportedLanguages().Length;

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
		/// raw system language
		/// additionnal rules to override sys language
		/// 	like: #if loca_en 
		/// </summary>
		virtual public SystemLanguage getApplicatonLanguageFiltered()
		{
			SystemLanguage langDefault = GetApplicatonLanguage();

#if loca_en
			langDefault = SystemLanguage.English;
#endif

#if loca_fr
			langDefault = SystemLanguage.French;
#endif
			return langDefault;
		}

		/// <summary>
		/// fallback is when trying to get a safe localization
		/// what language cannot fail ?
		/// </summary>
		virtual public IsoLanguages getIsoSafeLanguage() => IsoLanguages.en;

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

		/// <summary>
		/// bubble to reacts current saved language
		/// </summary>
		public void applySavedLanguage() => applyLanguage(getLanguage());

		/// <summary>
		/// when language changed, to bubble to reacts
		/// </summary>
		void applyLanguage(IsoLanguages newLang)
		{
			Debug.Log("<color=cyan>applyLanguage</color> to <b>" + newLang + "</b>!");

			if (!Application.isPlaying)
			{
				reacts.Clear();
				reacts.AddRange(fwp.appendix.AppendixUtils.getCandidates<iLanguageChangeReact>());
			}

			Debug.Log("applying new lang (" + newLang + ") to x" + reacts.Count + " reacts");

			for (int i = 0; i < reacts.Count; i++)
			{
				reacts[i].onLanguageChange(newLang.ToString());
			}

		}

		public void nextLanguage()
		{
			IsoLanguages cur = getLanguage();

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

			setLanguage(cur, true);
		}

		/// <summary>
		/// apply: bubble to reacts
		/// </summary>
		public void setLanguage(IsoLanguages iso, bool apply = false)
		{
			writeLanguage(iso);
			if (apply) applyLanguage(iso); // apply
		}

		/// <summary>
		/// </summary>
		public IsoLanguages getLanguage()
		{
			IsoLanguages iso = readLanguage();

			if (!isIsoLanguageSupported(iso))
			{
				logw($"{iso} not supported, fallback to system default", this);
				iso = SysToIso(getApplicatonLanguageFiltered());
				writeLanguage(iso);
			}

			return iso;
		}

		/// <summary>
		/// write language
		/// </summary>
		virtual protected void writeLanguage(IsoLanguages iso)
		{
			PlayerPrefs.SetInt(ppref_iso_language, (int)iso);
		}

		/// <summary>
		/// read language
		/// </summary>
		virtual protected IsoLanguages readLanguage()
		{
			return (IsoLanguages)PlayerPrefs.GetInt(ppref_iso_language, (int)LocalizatorMinds.Languages.getApplicatonLanguageFiltered());
		}

		/// <summary>
		/// raw unity detection
		/// best to don't use directly
		/// use filtered version instead
		/// </summary>
		static public SystemLanguage GetApplicatonLanguage() => Application.systemLanguage;

		/// <summary>
		/// https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.twoletterisolanguagename?view=net-5.0
		/// </summary>
		static IsoLanguages SysToIso(SystemLanguage sys)
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
			}

			Debug.LogWarning("[ISO]	sys language " + sys + " is not supported");
			return IsoLanguages.en; // fallback
		}

		public string StringifySupported
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