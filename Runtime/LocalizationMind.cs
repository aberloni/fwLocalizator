using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace fwp.localizator
{

    /// <summary>
    /// scaffold for loca managers
    /// </summary>
    abstract public class LocalizationMind
    {
        static public LanguagesMind Languages
        {
            private set;
            get;
        }

        static public SheetsMind Sheets
        {
            private set;
            get;
        }

        static public DialogsMind Dialogs
        {
            private set;
            get;
        }

        static public void InitMinds()
        {
            if (Languages == null) Languages = new LanguagesMind();
            if (Sheets == null) Sheets = new SheetsMind();
            if (Dialogs == null) Dialogs = new DialogsMind();
        }

        static public void ReplaceMind<T>(T mind) where T : LocalizationMind
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(LanguagesMind): Languages = mind as LanguagesMind; break;
                case Type t when t == typeof(SheetsMind): Sheets = mind as SheetsMind; break;
                case Type t when t == typeof(DialogsMind): Dialogs = mind as DialogsMind; break;
            }
        }

        static public bool Verbose
        {
            get
            {
                return PlayerPrefs.GetInt("loca_verbose", 0) == 1;
            }
            set
            {
                if (Verbose != value)
                {
                    PlayerPrefs.SetInt("loca_verbose", value ? 1 : 0);
#if UNITY_EDITOR
                    // feedback state change
                    logLocaVerbose();
#endif
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Window/Localizator/verbose.log")]
        static void logLocaVerbose() => Debug.Log("Loca> " + Verbose);

        [UnityEditor.MenuItem("Window/Localizator/loca.verbose.toggle?")]
        static void toggleLocaVerbose() => Verbose = !Verbose;

#endif

        static public void log(string content, object target = null)
        {
            if (!LocalizationMind.Verbose) return;

            Debug.Log("Loca> "
                + ((target != null) ? target.GetType().ToString() : string.Empty)
                + "     "
                + content, target as UnityEngine.Object);
        }

        static public void logw(string content, object target = null)
        {
            Debug.LogWarning("Loca> "
                + ((target != null) ? target.GetType().ToString() : string.Empty)
                + "     "
                + content, target as UnityEngine.Object);
        }

    }

}