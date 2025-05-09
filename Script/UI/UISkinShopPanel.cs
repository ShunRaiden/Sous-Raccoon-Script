using SousRaccoon.Data;
using SousRaccoon.Manager;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UISkinShopPanel : MonoBehaviour
    {
        public event Action<int> OnSkinSelectedButtonClickEvent;

        [SerializeField] List<UISkinShopSlot> skinShopSlots = new List<UISkinShopSlot>();

        [Header("UI")]
        [SerializeField] GameObject content;

        [SerializeField] TMP_Text playerMoneyText;
        [SerializeField] TMP_Text priceText;
        [SerializeField] private LocalizeStringEvent skinNameText;

        [SerializeField] Image skinImage;

        [SerializeField] Button selectSkinButton;
        [SerializeField] Button buySkinButton;

        UISkinShopSlot currentSlot;
        PlayerSaveData levelData;

        private int currentSlotIndex;

        private void OnEnable()
        {
            if (levelData == null)
                levelData = GameManager.instance.playerSaveData;

            // ให้ทุก Slot โหลดข้อมูลก่อน
            foreach (var slot in skinShopSlots)
            {
                slot.ForceInit(); // เรียกฟังก์ชันใหม่ที่เราจะสร้าง
            }

            // จากนั้นค่อย Auto เลือก Slot
            currentSlotIndex = GameManager.instance.playerSaveData.Skin;
            currentSlot = skinShopSlots[currentSlotIndex];
            OnSlotSelected(currentSlotIndex, currentSlot);
            OnSkinButtonSelected();

            selectSkinButton.onClick.AddListener(OnSkinButtonSelected);
            buySkinButton.onClick.AddListener(OnBuySkinButton);
        }

        private void OnDisable()
        {
            selectSkinButton.onClick.RemoveListener(OnSkinButtonSelected);
            buySkinButton.onClick.RemoveListener(OnBuySkinButton);
        }

        public void SetUpPanel(UISkinShopSlot slot)
        {
            skinImage.sprite = slot.playerSkinData.skinProfile;
            skinNameText.StringReference = slot.playerSkinData.displayName;
            priceText.text = slot.playerSkinData.skinPrice.ToString();
        }

        public void OnSlotSelected(int slotIndex, UISkinShopSlot slot)
        {
            currentSlot.OnSlotUnselected();
            currentSlotIndex = slotIndex;
            SetUpPanel(slot);
            currentSlot = slot;
            UpdateButtonsState();
        }

        public void OnSkinButtonSelected()
        {
            OnSkinSelectedButtonClickEvent?.Invoke(currentSlotIndex);
            currentSlot.OnSkinSelected();
        }

        public void OnBuySkinButton()
        {
            if (GameManager.instance.PlayerMoney >= currentSlot.playerSkinData.skinPrice)
            {
                GameManager.instance.RemoveMoney(currentSlot.playerSkinData.skinPrice);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                playerMoneyText.text = GameManager.instance.PlayerMoney.ToString();
                levelData.UnlockedSkins[currentSlotIndex] = true;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);
                GameManager.instance.playerPrefab = currentSlot.playerSkinData.playerPrefab;

                currentSlot.OnSetPurchase();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
                UpdateButtonsState();
            }
        }

        public void OnOpenPanel()
        {
            content.SetActive(true);
            playerMoneyText.text = GameManager.instance.PlayerMoney.ToString();
            UpdateButtonsState();
        }

        public void OnClosePanel()
        {
            content.SetActive(false);
        }

        private void OnUpgradeSound()
        {
            AudioManager.instance.PlayOneShotSFX("CashMoney");
        }

        private void UpdateButtonsState()
        {
            bool isUnlocked = levelData.UnlockedSkins[currentSlotIndex];
            bool canAfford = GameManager.instance.PlayerMoney >= currentSlot.playerSkinData.skinPrice;

            // ปุ่มเลือกจะใช้ได้ถ้าปลดล็อกแล้ว
            selectSkinButton.interactable = isUnlocked;

            // ปุ่มซื้อจะใช้ได้ถ้ายังไม่ปลดล็อก และมีเงินพอ
            buySkinButton.interactable = !isUnlocked && canAfford;
        }
    }
}
