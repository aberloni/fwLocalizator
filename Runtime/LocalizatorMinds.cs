
namespace fwp.localizator
{
    /// <summary>
    /// no auto-init
    /// context must create what is needed
    /// </summary>
    static public class LocalizatorMinds
    {
        static public bool CanLocalize()
        {
            if (Languages == null) return false;
            if (Sheets == null) return false;
            return true;
        }

        static public bool CanDialogs()
        {
            if (Dialogs == null) return false;
            return CanLocalize();
        }

        /// <summary>
        /// basic setup
        /// </summary>
        static public void Presence()
        {
            Languages = new();
            Sheets = new();
            Dialogs = new();
        }

        /// <summary>
        /// supported languages
        /// </summary>
        static public LanguagesMind Languages;

        /// <summary>
        /// loca files
        /// </summary>
        static public SheetsMind Sheets;

        /// <summary>
        /// dialogs
        /// </summary>
        static public DialogsMind Dialogs;
    }

}