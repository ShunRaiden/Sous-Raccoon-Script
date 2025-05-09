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

            // 1. เช็คว่า Perk ปกติทั้งหมดเต็มแล้วหรือยัง
            bool allNormalPerksMaxed = perkDataBase
                .Where(p => p.typeOfPerk == TypeOfPerk.Status)
                .All(p => maxLevelPerk.ContainsKey(p.perkName) && maxLevelPerk[p.perkName]);

            List<ShopMerchantItemDataBase> selectedPerks = new List<ShopMerchantItemDataBase>();

            if (allNormalPerksMaxed)
            {
                // ถ้า Perk ปกติเต็มทั้งหมดแล้วให้เลือก Perk ค่าเงินมาเลย (ไม่มีการสุ่ม)
                selectedPerks = perkDataBase
                    .Where(p => p.typeOfPerk == TypeOfPerk.Currency)
                    .ToList();
            }
            else
            {
                // 2. หยิบเฉพาะ Perk ปกติที่ยังไม่เต็ม
                List<ShopMerchantItemDataBase> availablePerks = perkDataBase
                   .Where(p => p.typeOfPerk == TypeOfPerk.Status &&
                   maxLevelPerk.TryGetValue(p.perkName, out bool isMaxed) && !isMaxed)
                   .ToList();

                // 3. หยิบทั้งหมดที่มี ถ้ามีน้อยกว่า 4 ก็เอาเท่าที่มี
                int count = Mathf.Min(maxPerksToSelect, availablePerks.Count);

                selectedPerks = availablePerks.OrderBy(p => Random.value).Take(count).ToList();
            }

            return selectedPerks;
        }

        public bool CheckIfAllPerksMaxed()
        {
            // ตรวจสอบว่า Perk ปกติทั้งหมดเต็มหรือยัง
            isMaxAllPerk = maxLevelPerk.Values.All(value => value);
            return isMaxAllPerk;
        }

        public void LevelUpPerk(string perkName)
        {
            // ค้นหา Perk จาก perkDataBase
            var perkData = perkDataBase.FirstOrDefault(p => p.perkName == perkName);
            if (perkData == null)
            {
                Debug.LogWarning($"Perk '{perkName}' ไม่พบใน perkDataBase!");
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

            // ตรวจสอบว่า Perk ปกติทั้งหมดเต็มแล้วหรือยังหลังจากอัปเกรด
            CheckIfAllPerksMaxed();
        }

        public int GetPerkLevel(string perkName)
        {
            // ตรวจสอบว่ามีชื่อ Perk นี้อยู่ใน Dictionary หรือไม่
            if (currentLevelPerk.ContainsKey(perkName))
            {
                return currentLevelPerk[perkName];
            }

            // ถ้าไม่มีชื่อ Perk ใน Dictionary ให้คืนค่า -1 เพื่อบ่งบอกว่าไม่พบ Perk
            Debug.LogWarning($"Perk '{perkName}' ไม่พบในระบบ!");
            return -1;
        }

        public List<(string perkName, int level, ShopMerchantItemDataBase perkData)> GetUpgradedPerks()
        {
            // สร้างรายการสำหรับเก็บผลลัพธ์
            List<(string perkName, int level, ShopMerchantItemDataBase perkData)> upgradedPerks = new List<(string, int, ShopMerchantItemDataBase)>();

            foreach (var perk in currentLevelPerk)
            {
                // ตรวจสอบว่า Perk มี Level มากกว่า 0 หรือไม่
                if (perk.Value > 0)
                {
                    // ค้นหาข้อมูล Perk จากฐานข้อมูล perkDataBase
                    ShopMerchantItemDataBase perkData = perkDataBase.FirstOrDefault(p => p.perkName == perk.Key);

                    if (perkData != null)
                    {
                        upgradedPerks.Add((perk.Key, perk.Value, perkData));
                    }
                    else
                    {
                        Debug.LogWarning($"Perk '{perk.Key}' ไม่พบใน perkDataBase!");
                    }
                }
            }

            return upgradedPerks;
        }

        public ShopMerchantItemDataBase GetRandomUpgradablePerk()
        {
            // คัดกรอง Perk ที่ Level มากกว่า 0 และยังไม่เต็ม
            List<ShopMerchantItemDataBase> upgradablePerks = perkDataBase
                .Where(p => currentLevelPerk.TryGetValue(p.perkName, out int level) && level > 0 &&
                            maxLevelPerk.TryGetValue(p.perkName, out bool isMaxed) && !isMaxed)
                .ToList();

            // ถ้าไม่มี Perk ที่อัปเกรดได้ให้คืนค่า null
            if (upgradablePerks.Count == 0)
            {
                return null;
            }

            // สุ่มเลือก 1 Perk จากรายการ
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
