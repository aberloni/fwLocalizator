using System;
using UnityEngine;

namespace fwp.localizator
{

    public class LocalizatorMinds
    {
        static public LanguagesMind Languages => Instance.languages;
        static public SheetsMind Sheets => Instance.sheets;
        static public DialogsMind Dialogs => Instance.dialogs;

        /// <summary>
        /// isntance creation need to be explicit
        /// </summary>
        static LocalizatorMinds _instance;
        static public LocalizatorMinds Instance => _instance ??= Create();
        static Func<LocalizatorMinds> Factory = () => new LocalizatorMinds();
        static LocalizatorMinds Create() => Factory();

        LanguagesMind languages;
        SheetsMind sheets;
        DialogsMind dialogs;

        public LocalizatorMinds()
        {
            if (languages == null) languages = new LanguagesMind();
            if (sheets == null) sheets = new SheetsMind();
            if (dialogs == null) dialogs = new DialogsMind();
        }

        /// <summary>
        /// to replace any manager by a specific new one
        /// </summary>
        public void ReplaceMind<T>(T mind) where T : LocalizationMind
        {
            switch (typeof(T))
            {
                case System.Type t when t == typeof(LanguagesMind): languages = mind as LanguagesMind; break;
                case System.Type t when t == typeof(SheetsMind):    sheets = mind as SheetsMind; break;
                case System.Type t when t == typeof(DialogsMind):   dialogs = mind as DialogsMind; break;
            }
        }

    }

}