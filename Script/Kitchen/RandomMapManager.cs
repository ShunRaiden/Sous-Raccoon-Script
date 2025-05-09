using SousRaccoon.Data;
using SousRaccoon.UI;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Manager
{
    public class RandomMapManager : MonoBehaviour
    {
        [SerializeField] UIRandomMapPanel randomMapPanel;
        [SerializeField] Sprite coinSprite;
        [SerializeField] int defaultCoinOldGive = 5;
        [SerializeField] int coinOldGive;
        [SerializeField] int coinNewGive;

        [SerializeField] LocalizedString noDebuffLocalizedText;

        string oldMapName;
        string newMapName;
        string nameNewReward;

        int oldMapIndex;
        int newMapIndex;

        bool hasPerk = false;

        Sprite oldMapIcon;
        Sprite newMapIcon;
        Sprite iconNewReward;

        ShopMerchantItemDataBase perkData;
        StageDebuffDataBase debuffDataBase;

        #region Set Up
        public void SetUpPanel()
        {
            RandomMap();
            GetPerkPlayer();

            //Generate Debuff
            if (RunStageManager.instance.mapPlayCount > 1)
                debuffDataBase = RunStageManager.instance.RandomDebuffs();

            coinOldGive = defaultCoinOldGive * RunStageManager.instance.mapPlayCount;

            var debuffText = noDebuffLocalizedText;
            if (debuffDataBase != null)
                debuffText = debuffDataBase.debuffDisplayName;

            randomMapPanel.SetUpPanel(oldMapIcon,
                                      coinSprite,
                                      $"+{coinOldGive}",
                                      newMapIcon,
                                      iconNewReward,
                                      nameNewReward,
                                      debuffText,
                                      debuffDataBase?.icon);

            randomMapPanel.onOldMapButtonClickEvent += OnOldMapSelected;
            randomMapPanel.onNewMapButtonClickEvent += OnNewMapSelected;

            GameManager.instance.OnChangeLanguageEvent += GetPerkPlayer;
        }

        private void OnDestroy()
        {
            randomMapPanel.onOldMapButtonClickEvent -= OnOldMapSelected;
            randomMapPanel.onNewMapButtonClickEvent -= OnNewMapSelected;
            GameManager.instance.OnChangeLanguageEvent -= GetPerkPlayer;
        }

        public void GetPerkPlayer()
        {
            if (perkData == null)
                perkData = RunStageManager.instance.GetRandomUpgradablePerk();

            if (perkData != null)
            {
                iconNewReward = perkData.icon;
                nameNewReward = $"{perkData.perkDisplayName.GetLocalizedString()} Lv. +1";
                hasPerk = true;
            }
            else
            {
                iconNewReward = coinSprite;
                nameNewReward = $"+{coinNewGive}";
            }
        }

        public void RandomMap()
        {
            //Random Map Name and Icon
            GameManager.instance.GetRandomMap(StageManager.instance.stageCurrentIndex,
                                              out oldMapName,
                                              out oldMapIndex,
                                              out newMapName,
                                              out newMapIndex);

            oldMapIcon = GameManager.instance.spriteResource.mapIconSprite[StageManager.instance.stageCurrentIndex - 1].mapIcon[oldMapIndex];
            newMapIcon = GameManager.instance.spriteResource.mapIconSprite[StageManager.instance.stageCurrentIndex - 1].mapIcon[newMapIndex];
        }
        #endregion

        #region System
        public void OnOldMapSelected()
        {
            AddCoin(coinOldGive);

            if (debuffDataBase != null)
                RunStageManager.instance.currentstageDebuffs.Add(debuffDataBase); //Stack Debuff

            RunStageManager.instance.mapPlayCount++;
            StageManager.instance.MoveNextDay(oldMapName);
        }

        public void OnNewMapSelected()
        {
            if (hasPerk)
                UpPerk();
            else
                AddCoin(coinNewGive);

            RunStageManager.instance.currentstageDebuffs.Clear(); //Clear Debuff
            RunStageManager.instance.mapPlayCount = 1;
            StageManager.instance.MoveNextDay(newMapName);
        }

        public void AddCoin(int coin)
        {
            RunStageManager.instance.AddCoin(coin);
        }

        public void UpPerk()
        {
            RunStageManager.instance.LevelUpPerk(perkData.perkName);
        }
        #endregion
    }
}