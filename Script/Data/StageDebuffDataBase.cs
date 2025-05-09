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
        public DebuffStatType stat;  // �������ͧ Stat �� Health, Damage, Speed, IsInvincible

        public float floatValue;  // ���Ẻ Float
        public int intValue;      // ���Ẻ Int
        public bool boolValue;    // ���Ẻ Bool
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