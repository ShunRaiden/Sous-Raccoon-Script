using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "GameData/ScenarioData")]
    public class ScenarioDataBase : ScriptableObject
    {
        public string nameScenario;
        public LocalizedString nameScenarioLocalized;
        public string displayName;

        [TextArea(1, 3)]
        public string discription;
        public LocalizedString discriptionLocalized;

        public Sprite icon;
        public Sprite statIcon;
        public Sprite headIcon;

        public string statValue;

        [Header("Value Can be negative")]
        public List<int> intValue;
        public List<float> floatValue;

        public ScenarioType type;
    }

    public enum ScenarioType
    {
        ChefRage,
        ChefSpeed,
        RaccoonHealRate,
        MonsterSpawnRate,
        CustomerSpawnRate,
        TableCount,
    }
}
