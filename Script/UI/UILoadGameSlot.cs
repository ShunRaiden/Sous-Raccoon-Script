using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UILoadGameSlot : MonoBehaviour
    {
        public event Action<int> EventLoadButtonClick;
        public event Action<int> EventDeleteButtonClick;

        [SerializeField] private Button slotButton;
        [SerializeField] private Button slotDeleteButton;

        public int slotIndex;
        [SerializeField] private Image slotImage;
        [SerializeField] private TMP_Text slotStateUnlock;
        [SerializeField] private TMP_Text lastTimeSaveSlot;

        private void Awake()
        {
            slotButton.onClick.AddListener(OnLoadButtonClick);
            slotDeleteButton.onClick.AddListener(OnDeleteButtonClick);
        }

        private void OnDestroy()
        {
            slotButton.onClick.RemoveAllListeners();
        }

        public void LoadDataSlot(Sprite sprite, string state, string lastTime, bool hasData)
        {
            slotImage.sprite = sprite;
            slotStateUnlock.text = state;
            lastTimeSaveSlot.text = lastTime;

            slotDeleteButton.gameObject.SetActive(hasData);
        }

        private void OnLoadButtonClick()
        {
            EventLoadButtonClick?.Invoke(slotIndex);
        }

        private void OnDeleteButtonClick()
        {
            EventDeleteButtonClick?.Invoke(slotIndex);
        }
    }
}
