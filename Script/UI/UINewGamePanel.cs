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
        bool hasData = false; // ���������Ѻ������բ������ slot ������͡�������

        public void SetDataSlot()
        {
            ClearAllEvents(); // ź Event �������������͹

            acceptButton.onClick.RemoveListener(OnAcceptGame);

            int slotCount = Mathf.Min(newGameSlots.Count, 5); // ��˹��ӹǹ����ʹ���

            for (int i = 0; i < slotCount; i++)
            {
                AddDataSlot(i);
            }

            acceptButton.onClick.AddListener(OnAcceptGame);

            //GameManager.instance.ChangeUISelectNav(newGameSlots[0].gameObject);
        }

        public void ClosePanel()
        {
            ClearAllEvents(); // ź Event �ء���
            //LobbyManager.instance.mainMenuPanel.BackToMianMenuUINav();
            contentNewGamePanel.SetActive(false); // �Դ Panel
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
            // �ѹ�֡������ slot ������͡
            selectSlot = slotIndex;

            // ��Ǩ�ͺ����բ������������
            var data = SaveManager.LoadData(slotIndex);
            hasData = data != null;

            // �Դ acceptPanel ��������͡ slot
            acceptPanel.SetActive(true);
        }

        private void OnNewGame()
        {
            LobbyManager.instance.NewGameSaveSlot(selectSlot);
            acceptPanel.SetActive(false); // �Դ panel ��ѧ�ҡ�����������
        }

        private void OnOverrideGame()
        {
            OnDeleteSave(selectSlot);
            LobbyManager.instance.NewGameSaveSlot(selectSlot);
            acceptPanel.SetActive(false); // �Դ panel ��ѧ�ҡ override ��
        }

        private void OnDeleteSave(int slotIndex)
        {
            LobbyManager.instance.RemoveSaveSlot(slotIndex);
        }

        private void OnAcceptGame()
        {
            // ��Ǩ�ͺ����բ������������� slot ������͡
            if (hasData)
            {
                // ����բ����� ��� override
                OnOverrideGame();
            }
            else
            {
                // �������բ����� ��������������
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
