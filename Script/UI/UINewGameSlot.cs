using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UINewGameSlot : MonoBehaviour
    {
        public event Action<int> EventNewSaveButtonClick;

        [SerializeField] private Button slotButton;

        public int slotIndex;
        [SerializeField] private Image slotImage;
        [SerializeField] private TMP_Text slotStateUnlock;
        [SerializeField] private TMP_Text lastTimeSaveSlot;

        private void Awake()
        {
            slotButton.onClick.AddListener(OnLoadButtonClick);
        }

        private void OnDestroy()
        {
            slotButton.onClick.RemoveAllListeners();
        }

        public void LoadDataSlot(Sprite sprite, string state, string lastTime)
        {
            slotImage.sprite = sprite;
            slotStateUnlock.text = state;
            lastTimeSaveSlot.text = lastTime;
        }

        private void OnLoadButtonClick()
        {
            EventNewSaveButtonClick?.Invoke(slotIndex);
        }
    }
}
