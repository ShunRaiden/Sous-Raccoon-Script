using SousRaccoon.Data;
using SousRaccoon.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.ShopMerchant
{
    public class UIShopMerchantPanel : MonoBehaviour
    {
        public Button rerollButton;

        public TMP_Text rerollText;

        public int reRollTimes;

        public TMP_Text playerCoinText;

        public UIShopMerchantItemSlot baseSlot;

        public Transform perkLayout;

        public ShopMerchantManager shopMerchantManager;

        public List<ShopMerchantItemDataBase> perkSpawnList = new List<ShopMerchantItemDataBase>();

        //public List<ShopMerchantItemDataBase> perkCurrentList = new List<ShopMerchantItemDataBase>();

        public List<UIShopMerchantItemSlot> uIShopMerchantItemSlots = new List<UIShopMerchantItemSlot>();

        private void OnDestroy()
        {
            if (rerollButton != null)
                rerollButton.onClick.RemoveListener(RerollPerk);
        }

        public void SetUpShopUI()
        {
            rerollText.text = reRollTimes.ToString();
            rerollButton.onClick.AddListener(RerollPerk);
            playerCoinText.text = RunStageManager.instance.PlayerCoin.ToString();
            RandomPerk();
        }

        public void SetUpLastDayShopUI()
        {
            rerollButton.gameObject.SetActive(false);
            playerCoinText.text = RunStageManager.instance.PlayerCoin.ToString();
            CoinExchangePerk();
        }

        public void RandomPerk()
        {
            perkSpawnList = RunStageManager.instance.RandomPerks();

            foreach (var perk in perkSpawnList)
            {
                var perkCurrent = Instantiate(baseSlot, perkLayout);

                var perkLevel = RunStageManager.instance.GetPerkLevel(perk.perkName);

                var canBuy = shopMerchantManager.CheckPlayerCanBuy(perk.levelPrices[perkLevel]);
                perkCurrent.slotData = perk;
                perkCurrent.SetUpSlot(perk.icon, perkLevel + 1, perk.perkDisplayName, perk.description, perk.levelPrices[perkLevel], canBuy, shopMerchantManager);

                //perkCurrentList.Add(perk);
                uIShopMerchantItemSlots.Add(perkCurrent);
            }
        }

        public void CoinExchangePerk()
        {
            var perkCurrent = Instantiate(baseSlot, perkLayout);
            perkCurrent.slotData = shopMerchantManager.coinExchangePerk;
            var perkPrice = RunStageManager.instance.PlayerCoin;
            perkCurrent.SetUpSlot(shopMerchantManager.coinExchangePerk.icon, 0, shopMerchantManager.coinExchangePerk.perkDisplayName, shopMerchantManager.coinExchangePerk.description, perkPrice, true, shopMerchantManager);

            //perkCurrentList.Add(shopMerchantManager.coinExchangePerk);
            uIShopMerchantItemSlots.Add(perkCurrent);
        }

        public void UpdateSlot()
        {
            foreach (var perkSlot in uIShopMerchantItemSlots)
            {
                if (!perkSlot.isSoldOut)
                {
                    var perkLevel = RunStageManager.instance.GetPerkLevel(perkSlot.slotData.perkName);
                    var canBuy = shopMerchantManager.CheckPlayerCanBuy(perkSlot.slotData.levelPrices[perkLevel]);
                    perkSlot.OnUpdateSlot(canBuy);

                }
            }
            playerCoinText.text = RunStageManager.instance.PlayerCoin.ToString();
        }

        public void RerollPerk()
        {
            if (reRollTimes <= 0) return;

            reRollTimes--;

            rerollText.text = reRollTimes.ToString();

            foreach (var perk in uIShopMerchantItemSlots)
            {
                Destroy(perk.gameObject);
            }

            perkSpawnList.Clear();
            uIShopMerchantItemSlots.Clear();

            RandomPerk();
        }
    }
}
