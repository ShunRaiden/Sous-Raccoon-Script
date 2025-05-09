using SousRaccoon.Data;
using SousRaccoon.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIPerkInfoListPanel : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] UIPerkInfoSlot basePerkInfoSlot;
        [SerializeField] Transform slotParent;
        private List<UIPerkInfoSlot> perkInfoSlotList = new List<UIPerkInfoSlot>();

        [Header("------[ Description ]------")]
        [SerializeField] GameObject perkDescriptionPanel;
        [SerializeField] Image perkDescriptionIcon;
        [SerializeField] LocalizeStringEvent perkDescriptionNameText;
        [SerializeField] LocalizeStringEvent perkDescriptionText;
        [SerializeField] TMP_Text perkLevelText;

        public void OpenDiscriptionPerkInfo(Sprite icon, LocalizedString name, LocalizedString description, string level)
        {
            perkDescriptionPanel.SetActive(true);
            perkDescriptionIcon.sprite = icon;
            perkDescriptionNameText.StringReference = name;
            perkDescriptionText.StringReference = description;
            perkLevelText.text = level;
        }

        public void OpenPerkInfo()
        {
            perkDescriptionPanel.SetActive(false);
            List<(string perkName, int level, ShopMerchantItemDataBase perkData)> upgradedPerks = RunStageManager.instance.GetUpgradedPerks();
            EnsureSlotCount(upgradedPerks.Count);

            for (int i = 0; i < upgradedPerks.Count; i++)
            {
                perkInfoSlotList[i].SetUpPerkInfo(upgradedPerks[i].level.ToString(), this, upgradedPerks[i].perkData);
                perkInfoSlotList[i].gameObject.SetActive(true);
            }

            content.SetActive(true);
        }

        public void ClosePerkInfo()
        {
            /*
            foreach (var slot in perkInfoSlotList)
            {
                slot.gameObject.SetActive(false);
            }
            */

            content.SetActive(false);
            perkDescriptionPanel.SetActive(false);
        }

        private void EnsureSlotCount(int count)
        {
            while (perkInfoSlotList.Count < count)
            {
                UIPerkInfoSlot newSlot = Instantiate(basePerkInfoSlot, slotParent);
                newSlot.gameObject.SetActive(false);
                perkInfoSlotList.Add(newSlot);
            }
        }
    }
}
