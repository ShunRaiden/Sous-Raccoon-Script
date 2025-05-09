using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Data
{
    public enum TypeOfPerk { Status, Currency, }

    [CreateAssetMenu(fileName = "ShopMerchantItemData", menuName = "GameData/ShopMerchantItemData")]
    public class ShopMerchantItemDataBase : ScriptableObject
    {
        public string perkName;
        public LocalizedString perkDisplayName;
        public LocalizedString description;
        public Sprite icon;
        [Space(35)]
        public TypeOfPerk typeOfPerk;
        [Space(20)]
        public List<int> levelPrices;
        public List<PerkStatModifier> statModifiers;
    }

    [System.Serializable]
    public class PerkStatModifier
    {
        public List<StatModifier> modifiers;  // รายการของค่าที่จะเปลี่ยน
    }

    [System.Serializable]
    public class StatModifier
    {
        public StatType stat;  // ประเภทของ Stat เช่น Health, Damage, Speed, IsInvincible

        public float floatValue;  // ค่าแบบ Float
        public int intValue;      // ค่าแบบ Int
        public bool boolValue;    // ค่าแบบ Bool
    }

    public enum StatType
    {
        MoveSpeed, RollCDR, RollRange, ATKSpeed, ATKDamage, Stun, HealRate, HealRange, TwoHand,
        CustomerMoneyDrop,
        CoinSpwnRate,
        CookSpeedChef, AngryLimitChef,
        Money,
        BarricadeStat,
        HelperGiverStat,
        CoinExchange,       
    }
}