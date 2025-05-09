using SousRaccoon.Data;
using SousRaccoon.Manager;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI.ShopMerchant
{
    public class UIShopMerchantItemSlot : MonoBehaviour
    {
        public event Action<ShopMerchantItemDataBase> EventOnBuyButtonClicked;

        [SerializeField] private Button buyButton;

        [SerializeField] private Image slotIcon;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private LocalizeStringEvent nameText;
        [SerializeField] private LocalizeStringEvent discriptionText;
        [SerializeField] private TMP_Text priceText;

        [SerializeField] private GameObject soldOutPanel;
        [SerializeField] private GameObject cantBuyButton;

        public ShopMerchantManager shopMerchantManager;

        public ShopMerchantItemDataBase slotData;

        public bool isSoldOut = false;

        protected void Awake()
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        protected void OnDestroy()
        {
            buyButton.onClick.RemoveAllListeners();
            EventOnBuyButtonClicked -= shopMerchantManager.OnBuyingPerk;
        }

        public void SetUpSlot(Sprite icon,
                              int levelOfPerk,
                              LocalizedString nameOfPerk,
                              LocalizedString discriptionOfPerk,
                              int priceOfPerk,
                              bool canBuy,
                              ShopMerchantManager shopManager)
        {
            shopMerchantManager = shopManager;
            EventOnBuyButtonClicked += shopMerchantManager.OnBuyingPerk;

            slotIcon.sprite = icon;
            levelText.text = levelOfPerk.ToString();

            nameText.StringReference = nameOfPerk;
            discriptionText.StringReference = discriptionOfPerk;
            priceText.text = $"{priceOfPerk}";

            cantBuyButton.SetActive(!canBuy);
            buyButton.interactable = canBuy;
        }

        public void OnUpdateSlot(bool canBuy)
        {
            cantBuyButton.SetActive(!canBuy);
            buyButton.interactable = canBuy;
        }

        private void OnBuyButtonClicked()
        {
            Debug.LogError("Buy");
            AudioManager.instance.PlayStageSFXOneShot("CashMoney");
            soldOutPanel.SetActive(true);
            buyButton.interactable = false;
            isSoldOut = true;

            EventOnBuyButtonClicked?.Invoke(slotData);
        }
    }
}
