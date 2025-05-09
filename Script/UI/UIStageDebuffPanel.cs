using SousRaccoon.Data;
using SousRaccoon.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.UI
{
    public class UIStageDebuffPanel : MonoBehaviour
    {
        public event Action OnOpenPanelEvent;
        public event Action OnClosePanelEvent;

        public StageDebuffManager stageDebuffManager;
        public List<UIStageDebuffSlot> slots;

        [SerializeField] Animator animator;

        private void Start()
        {
            StageManager.instance.EventOnGameEnd += ClosePanel;
        }

        private void OnDestroy()
        {
            StageManager.instance.EventOnGameEnd -= ClosePanel;
        }

        public void OpenDebuffPanel()
        {
            if (animator != null)
                animator.Play("Open");

            OnOpenPanelEvent?.Invoke();
        }

        public void CloseDebuffInfo()
        {
            if (animator != null)
                animator.Play("Close");

            OnClosePanelEvent?.Invoke();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        public void SetupDebuffInfoSlot(StageDebuffDataBase dataBase, int index)
        {
            slots[index].SetupDebuffInfoSlot(dataBase.icon, this, dataBase);
            slots[index].gameObject.SetActive(true);
        }
    }
}