using SousRaccoon.Data;
using SousRaccoon.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UISkinShopSlot : MonoBehaviour
    {
        public int slotIndex;
        public Image skinImage;
        public Button slotButton;
        public PlayerPrefab playerSkinData;

        [SerializeField] GameObject slotSelected;
        [SerializeField] GameObject skinActivateUI; // Has Purchase
        [SerializeField] GameObject skinSelectedUI; // On Selected Skin

        [SerializeField] UISkinShopPanel skinPanel;
        PlayerSaveData levelData;

        private void OnEnable()
        {
            slotButton.onClick.AddListener(OnSlotSelected);
            skinPanel.OnSkinSelectedButtonClickEvent += OnSkinUnselected;
        }

        private void OnDisable()
        {
            slotButton.onClick.RemoveListener(OnSlotSelected);
            skinPanel.OnSkinSelectedButtonClickEvent -= OnSkinUnselected;
        }

        public void OnSkinSelected()
        {
            levelData.Skin = slotIndex;
            SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);
            GameManager.instance.playerPrefab = playerSkinData.playerPrefab;
            skinSelectedUI.SetActive(true);

            LobbyManager.instance.DestroyPlayer();
            LobbyManager.instance.SpawnPlayer();
        }

        public void OnSkinUnselected(int index)
        {
            skinSelectedUI.SetActive(false);
        }

        public void OnSetPurchase()
        {
            if (levelData.UnlockedSkins[slotIndex])
                skinActivateUI.SetActive(false);
            else
                skinActivateUI.SetActive(true);
        }

        public void OnSlotSelected()
        {
            slotSelected.SetActive(true);
            skinPanel.OnSlotSelected(slotIndex, this);
        }

        public void OnSlotUnselected()
        {
            slotSelected.SetActive(false);
        }

        public void ForceInit()
        {
            playerSkinData = GameManager.instance.playerDataBase.playerSkin[slotIndex];
            skinImage.sprite = playerSkinData.skinProfile;

            if (levelData == null)
                levelData = GameManager.instance.playerSaveData;

            OnSetPurchase();
        }
    }
}
