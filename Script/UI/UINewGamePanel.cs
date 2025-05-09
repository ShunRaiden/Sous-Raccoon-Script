using SousRaccoon.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UINewGamePanel : MonoBehaviour
    {
        public event Action EventAcceptSlot;
        public List<UINewGameSlot> newGameSlots;
        public GameObject acceptPanel;
        public Button acceptButton;

        public GameObject contentNewGamePanel;
        public GameObject contentMainMenuPanel;

        Sprite sprite;
        int state;
        string lastTime;

        int selectSlot;
        bool hasData = false; // ตัวแปรสำหรับเก็บว่ามีข้อมูลใน slot ที่เลือกหรือไม่

        public void SetDataSlot()
        {
            ClearAllEvents(); // ลบ Event ที่เหลืออยู่ก่อน

            acceptButton.onClick.RemoveListener(OnAcceptGame);

            int slotCount = Mathf.Min(newGameSlots.Count, 5); // กำหนดจำนวนให้ปลอดภัย

            for (int i = 0; i < slotCount; i++)
            {
                AddDataSlot(i);
            }

            acceptButton.onClick.AddListener(OnAcceptGame);

            //GameManager.instance.ChangeUISelectNav(newGameSlots[0].gameObject);
        }

        public void ClosePanel()
        {
            ClearAllEvents(); // ลบ Event ทุกตัว
            //LobbyManager.instance.mainMenuPanel.BackToMianMenuUINav();
            contentNewGamePanel.SetActive(false); // ปิด Panel
            contentMainMenuPanel.SetActive(true); // Back To Main Menu
            Debug.Log($"{gameObject.name} Panel closed.");
        }

        private void AddDataSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= newGameSlots.Count)
            {
                Debug.LogError($"Invalid slotIndex {slotIndex}. newGameSlots count = {newGameSlots.Count}");
                return;
            }

            var data = SaveManager.LoadData(slotIndex);
            var slot = newGameSlots[slotIndex];

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
            }
            else
            {
                sprite = null;
                state = 0;
                lastTime = "";
            }

            slot.LoadDataSlot(sprite, state.ToString(), lastTime);
            slot.EventNewSaveButtonClick += OnSlotSelected;
        }

        private void OnSlotSelected(int slotIndex)
        {
            // บันทึกข้อมูล slot ที่เลือก
            selectSlot = slotIndex;

            // ตรวจสอบว่ามีข้อมูลหรือไม่
            var data = SaveManager.LoadData(slotIndex);
            hasData = data != null;

            // เปิด acceptPanel เมื่อเลือก slot
            acceptPanel.SetActive(true);
        }

        private void OnNewGame()
        {
            LobbyManager.instance.NewGameSaveSlot(selectSlot);
            acceptPanel.SetActive(false); // ปิด panel หลังจากเริ่มเกมใหม่
        }

        private void OnOverrideGame()
        {
            OnDeleteSave(selectSlot);
            LobbyManager.instance.NewGameSaveSlot(selectSlot);
            acceptPanel.SetActive(false); // ปิด panel หลังจาก override เกม
        }

        private void OnDeleteSave(int slotIndex)
        {
            LobbyManager.instance.RemoveSaveSlot(slotIndex);
        }

        private void OnAcceptGame()
        {
            // ตรวจสอบว่ามีข้อมูลหรือไม่ใน slot ที่เลือก
            if (hasData)
            {
                // ถ้ามีข้อมูล ให้ override
                OnOverrideGame();
            }
            else
            {
                // ถ้าไม่มีข้อมูล ให้เริ่มเกมใหม่
                OnNewGame();
            }

            EventAcceptSlot?.Invoke();
        }

        private void ClearAllEvents()
        {
            foreach (var slot in newGameSlots)
            {
                slot.EventNewSaveButtonClick -= OnSlotSelected;
            }
            Debug.Log("All events cleared for NewGamePanel.");
        }
    }
}
