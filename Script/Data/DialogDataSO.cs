using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Playables;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "DialogData", menuName = "GameData/DialogsData ")]
    public class DialogDataSO : ScriptableObject
    {
        public List<LocalizedString> dialogSentences = new List<LocalizedString>();

        public bool switchChat;

        public string speakerName;
        public LocalizedString speakerNameLocalized;

        public string playerName;
        public Sprite speakerIcon;

        [Header("ForAE")]
        public PlayableDirector cutScene;
        public bool haveCutScene;
        public int cutSceneDialogIndex;
    }
}