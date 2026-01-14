using UnityEngine;

namespace fwp.localizator
{
    using fwp.localizator.subtitles;

    public class TesterSrt : MonoBehaviour
    {
        LocalizationSubtitleFile file;

        void Start()
        {
            file = new LocalizationSubtitleFile("sample");
        }

        [ContextMenu("log stringify file")]
        void cmLogStringify() => Debug.Log(file.stringify());
    }

}