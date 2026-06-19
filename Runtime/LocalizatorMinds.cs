using System;
using UnityEngine;

namespace fwp.localizator
{

    public class LocalizatorMinds
    {
        static public LanguagesMind Languages;
        static public SheetsMind Sheets;
        static public DialogsMind Dialogs;

        /// <summary>
        /// isntance creation need to be explicit
        /// </summary>
        static LocalizatorMinds _instance;
        static public LocalizatorMinds Instance => _instance ??= Create();
        static LocalizatorMinds Create() => Factory();

        /// <summary>
        /// to be override byt children class
        /// </summary>
        static protected Func<LocalizatorMinds> Factory = () => new LocalizatorMinds();
        
        public LocalizatorMinds()
        {
            if (Languages == null) new LanguagesMind();
            if (Sheets == null) new SheetsMind();
            if (Dialogs == null) new DialogsMind();
        }
        
    }

}