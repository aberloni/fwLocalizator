using UnityEngine;

namespace fwp.localizator
{

    static public class LocalizatorMinds
    {
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
                case System.Type t when t == typeof(LanguagesMind): Languages = mind as LanguagesMind; break;
                case System.Type t when t == typeof(SheetsMind): Sheets = mind as SheetsMind; break;
                case System.Type t when t == typeof(DialogsMind): Dialogs = mind as DialogsMind; break;
            }
        }

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

    }

}