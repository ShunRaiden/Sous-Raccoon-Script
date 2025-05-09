using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "StageDebuff", menuName = "GameData/StageDebuff")]
    public class StageDebuffDataBase : ScriptableObject
    {
        public string debuffName;
        public LocalizedString debuffDisplayName;
        public LocalizedString description;
        public Sprite icon;
        public DebuffStatModifier statModifiers;
    }

    [System.Serializable]
    public class DebuffStatModifier
    {
        public DebuffStatType stat;  // ประเภทของ Stat เช่น Health, Damage, Speed, IsInvincible

        public float floatValue;  // ค่าแบบ Float
        public int intValue;      // ค่าแบบ Int
        public bool boolValue;    // ค่าแบบ Bool
    }

    public enum DebuffStatType
    {
        MoveSpeed,
        RollCDR,
        RollRange,
        ATKSpeed,
        HealRange,
        ChefAngry,
        TableCount,
    }
}