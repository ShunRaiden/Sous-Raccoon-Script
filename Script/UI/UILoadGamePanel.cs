using SousRaccoon.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.UI.MainMenu
{
    public class UILoadGamePanel : MonoBehaviour
    {
        public event Action EventAcceptSlot;

        public List<UILoadGameSlot> loadSlots;

        public GameObject contentLoadGamePanel;
        public GameObject contentMainMenuPanel;

        Sprite sprite;
        int state;
        string lastTime;

        public void SetDataSlot()
        {
            ClearAllEvents(); // ลบ Event ที่เหลืออยู่ก่อน

            for (int i = 0; i < loadSlots.Count; i++)
            {
                AddDataSlot(i);
            }
        }

        public void AddDataSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= loadSlots.Count)
            {
                Debug.LogWarning($"Invalid slotIndex {slotIndex}. loadSlots count = {loadSlots.Count}");
                return;
            }

            var data = SaveManager.LoadData(slotIndex);
            if (data != null)
            {
                state = data.StageUnlock;

                if (state >= GameManager.instance.spriteResource.stageIconSprite.Count)
                {
                    Debug.LogError($"Invalid state {state}: out of range for stageIconSprite.");
                    sprite = null;
                }
                else
                {
                    sprite = GameManager.instance.spriteResource.stageIconSprite[state];
                }

                lastTime = data.LastTimeSave;

                var slot = loadSlots[slotIndex];
                slot.LoadDataSlot(sprite, state.ToString(), lastTime, true);
                slot.EventLoadButtonClick += OnLoadGame;
                slot.EventDeleteButtonClick += OnDeleteSave;
            }
            else
            {
                sprite = null;
                state = 0;
                lastTime = "";

                var slot = loadSlots[slotIndex];
                slot.LoadDataSlot(sprite, state.ToString(), lastTime, false);
                slot.EventLoadButtonClick += OnNewGame;
            }
        }


        public void ClosePanel()
        {
            ClearAllEvents(); // ลบ Event ทุกตัว
            contentLoadGamePanel.SetActive(false); // ปิด Panel
            contentMainMenuPanel.SetActive(true); // Back To Main Menu
        }

        private void OnNewGame(int slotIndex)
        {
            LobbyManager.instance.NewGameSaveSlot(slotIndex);
            EventAcceptSlot?.Invoke();
        }

        private void OnLoadGame(int slotIndex)
        {
            LobbyManager.instance.LoadSaveSlot(slotIndex);
            EventAcceptSlot?.Invoke();
        }

        private void OnDeleteSave(int slotIndex)
        {
            var slot = loadSlots[slotIndex];

            slot.EventLoadButtonClick -= OnLoadGame;
            slot.EventDeleteButtonClick -= OnDeleteSave;
            loadSlots[slotIndex].EventLoadButtonClick -= OnNewGame;

            LobbyManager.instance.RemoveSaveSlot(slotIndex);
            SetDataSlot();
        }

        private void ClearAllEvents()
        {
            foreach (var slot in loadSlots)
            {
                slot.EventLoadButtonClick -= OnLoadGame;
                slot.EventDeleteButtonClick -= OnDeleteSave;
                slot.EventLoadButtonClick -= OnNewGame;
            }
            Debug.Log("All events cleared for LoadGamePanel.");
        }
    }
}
