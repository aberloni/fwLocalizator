using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.dialog
{
    using fwp.localizator;

    public class DialogManager<LineData> where LineData : LocaDialogLineData
    {
        static public DialogManager<LineData> instance;

        public string[] dialogsUids;

        public LocaDialogData<LineData>[] dialogs;

        public DialogManager()
        {
            refresh();
        }

        public LocaDialogData<LineData> getDialogInstance(string uid)
        {
            foreach (var d in dialogs)
            {
                if (d == null)
                    continue;

                if (d.name == uid)
                    return d;
            }
            return null;
        }

        public bool hasDialogInstance(string uid)
        {
            foreach(var d in dialogs)
            {
                if (d.name == uid)
                    return true;
            }
            return false;
        }

        public void refresh()
        {
            dialogs = getDialogs();

            List<string> tmp = new List<string>();
            var mgr = fwp.localizator.LocalizationManager.instance;
            var file = mgr.getFileByLang(IsoLanguages.fr);
            var lines = file.getLines();
            foreach(var l in lines)
            {
                var split = l.Split("=");
                var uid = split[0];
                uid = uid.Substring(0, uid.LastIndexOf("-"));

                if(!tmp.Contains(uid))
                    tmp.Add(uid);
            }
            dialogsUids = tmp.ToArray();
        }

        LocaDialogData<LineData>[] getDialogs()
            => LocalizatorUtils.getScriptableObjectsInEditor<LocaDialogData<LineData>>();

    }
}
