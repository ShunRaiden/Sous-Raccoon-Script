using SousRaccoon.Data;
using SousRaccoon.Data.Item;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class RunStageManager : MonoBehaviour
    {
        #region Singleton
        public static RunStageManager instance { get { return _instance; } }
        private static RunStageManager _instance;

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;//Avoid doing anything else
            }
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                StartNewRunStage();
            }
        }

        #endregion

        public int PlayerCoin { get; private set; }

        public int summaryMoney;
        public int summaryCoin;
        public int summaryOrdersDerivered;
        public int summaryOrdersFailed;
        public int summaryMonstersDefeated;

        public List<ItemListInState> itemListInStates;
        public int maxDay;
        public int daysCount;

        public int foodDeliverAdder;

        public float dayPhaseTime;
        public float noonPhaseTime;
        public float eveningPhaseTime;

        [Space(3)]
        [Header("Perk")]
        [SerializeField] private List<ShopMerchantItemDataBase> perkDataBase;

        public Dictionary<string, int> currentLevelPerk = new Dictionary<string, int>();
        public Dictionary<string, bool> maxLevelPerk = new Dictionary<string, bool>();
        public bool isMaxAllPerk = false;

        public List<StageDebuffDataBase> debuffDataList = new List<StageDebuffDataBase>();
        public List<StageDebuffDataBase> currentstageDebuffs = new List<StageDebuffDataBase>();
        public int mapPlayCount;

        [Space(3)]
        [Header("Lose Rate")]
        public int currentLoseRate;

        public void StartNewRunStage()
        {
            PlayerCoin = 0;
            daysCount = 0;
            currentLoseRate = 0;
            isMaxAllPerk = false;
            SetUpPerkLevelDict();

            currentstageDebuffs.Clear();
            mapPlayCount = 1;

            summaryMoney = 0;
            summaryCoin = 0;
            summaryOrdersDerivered = 0;
            summaryOrdersFailed = 0;
            summaryMonstersDefeated = 0;
        }

        public void AddCoin(int coin)
        {
            PlayerCoin += coin;
            summaryCoin += coin;
            StageManager.instance.AddCoinText(PlayerCoin, coin);
        }

        public void RemoveCoin(int coin)
        {
            PlayerCoin -= coin;
            StageManager.instance.SetCoinText(PlayerCoin);
        }

        #region Perk System
        public void SetUpPerkLevelDict()
        {
            currentLevelPerk.Clear();
            maxLevelPerk.Clear();

            foreach (var perk in perkDataBase)
            {
                if (!currentLevelPerk.ContainsKey(perk.perkName))
                {
                    currentLevelPerk.Add(perk.perkName, 0);
                }

                if (!maxLevelPerk.ContainsKey(perk.perkName))
                {
                    maxLevelPerk.Add(perk.perkName, false);
                }
            }
        }

        public List<ShopMerchantItemDataBase> RandomPerks()
        {
            const int maxPerksToSelect = 4;

            // 1. ����� Perk ���Է�����������������ѧ
            bool allNormalPerksMaxed = perkDataBase
                .Where(p => p.typeOfPerk == TypeOfPerk.Status)
                .All(p => maxLevelPerk.ContainsKey(p.perkName) && maxLevelPerk[p.perkName]);

            List<ShopMerchantItemDataBase> selectedPerks = new List<ShopMerchantItemDataBase>();

            if (allNormalPerksMaxed)
            {
                // ��� Perk �����������������������͡ Perk ����Թ����� (����ա������)
                selectedPerks = perkDataBase
                    .Where(p => p.typeOfPerk == TypeOfPerk.Currency)
                    .ToList();
            }
            else
            {
                // 2. ��Ժ੾�� Perk ���Է���ѧ������
                List<ShopMerchantItemDataBase> availablePerks = perkDataBase
                   .Where(p => p.typeOfPerk == TypeOfPerk.Status &&
                   maxLevelPerk.TryGetValue(p.perkName, out bool isMaxed) && !isMaxed)
                   .ToList();

                // 3. ��Ժ����������� ����չ��¡��� 4 �������ҷ����
                int count = Mathf.Min(maxPerksToSelect, availablePerks.Count);

                selectedPerks = availablePerks.OrderBy(p => Random.value).Take(count).ToList();
            }

            return selectedPerks;
        }

        public bool CheckIfAllPerksMaxed()
        {
            // ��Ǩ�ͺ��� Perk ���Է�������������ѧ
            isMaxAllPerk = maxLevelPerk.Values.All(value => value);
            return isMaxAllPerk;
        }

        public void LevelUpPerk(string perkName)
        {
            // ���� Perk �ҡ perkDataBase
            var perkData = perkDataBase.FirstOrDefault(p => p.perkName == perkName);
            if (perkData == null)
            {
                Debug.LogWarning($"Perk '{perkName}' ��辺� perkDataBase!");
                return;
            }

            int maxLevel = perkData.levelPrices.Count;

            if (currentLevelPerk.ContainsKey(perkName))
            {
                currentLevelPerk[perkName]++;
                if (currentLevelPerk[perkName] >= maxLevel)
                {
                    maxLevelPerk[perkName] = true;
                }
            }

            // ��Ǩ�ͺ��� Perk ���Է�����������������ѧ��ѧ�ҡ�ѻ�ô
            CheckIfAllPerksMaxed();
        }

        public int GetPerkLevel(string perkName)
        {
            // ��Ǩ�ͺ����ժ��� Perk �������� Dictionary �������
            if (currentLevelPerk.ContainsKey(perkName))
            {
                return currentLevelPerk[perkName];
            }

            // �������ժ��� Perk � Dictionary ���׹��� -1 ���ͺ觺͡�����辺 Perk
            Debug.LogWarning($"Perk '{perkName}' ��辺��к�!");
            return -1;
        }

        public List<(string perkName, int level, ShopMerchantItemDataBase perkData)> GetUpgradedPerks()
        {
            // ���ҧ��¡������Ѻ�纼��Ѿ��
            List<(string perkName, int level, ShopMerchantItemDataBase perkData)> upgradedPerks = new List<(string, int, ShopMerchantItemDataBase)>();

            foreach (var perk in currentLevelPerk)
            {
                // ��Ǩ�ͺ��� Perk �� Level �ҡ���� 0 �������
                if (perk.Value > 0)
                {
                    // ���Ң����� Perk �ҡ�ҹ������ perkDataBase
                    ShopMerchantItemDataBase perkData = perkDataBase.FirstOrDefault(p => p.perkName == perk.Key);

                    if (perkData != null)
                    {
                        upgradedPerks.Add((perk.Key, perk.Value, perkData));
                    }
                    else
                    {
                        Debug.LogWarning($"Perk '{perk.Key}' ��辺� perkDataBase!");
                    }
                }
            }

            return upgradedPerks;
        }

        public ShopMerchantItemDataBase GetRandomUpgradablePerk()
        {
            // �Ѵ��ͧ Perk ��� Level �ҡ���� 0 ����ѧ������
            List<ShopMerchantItemDataBase> upgradablePerks = perkDataBase
                .Where(p => currentLevelPerk.TryGetValue(p.perkName, out int level) && level > 0 &&
                            maxLevelPerk.TryGetValue(p.perkName, out bool isMaxed) && !isMaxed)
                .ToList();

            // �������� Perk ����ѻ�ô�����׹��� null
            if (upgradablePerks.Count == 0)
            {
                return null;
            }

            // �������͡ 1 Perk �ҡ��¡��
            return upgradablePerks[Random.Range(0, upgradablePerks.Count)];
        }
        #endregion

        #region Stage Debuff
        public StageDebuffDataBase RandomDebuffs()
        {
            var debuffDatas = GetAvailableDebuffs();
            return debuffDatas[Random.Range(0, debuffDatas.Count)];
        }

        private List<StageDebuffDataBase> GetAvailableDebuffs()
        {
            return debuffDataList.Except(currentstageDebuffs).ToList();
        }
        #endregion
    }
}
