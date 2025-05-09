using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
    public class PlayerDataBase : ScriptableObject
    {
        [Header("BaseStat")]
        public PlayerSpeedData defaultPlayerSpeed;
        public PlayerRollCooldownData defaultPlayerRollCooldown;
        public PlayerCombatData defaultPlayerCombat;
        public PlayerHealingData defaultPlayerHealing;
        public PlayerPrefab defaultPlayerSkin;
        public UpgradeCustomer defaultCustomersStat;

        [Header("Movement")]
        public List<PlayerSpeedData> playerSpeed = new();
        public List<PlayerRollCooldownData> playerRollCooldown = new();

        [Header("Combat")]
        public List<PlayerCombatData> playerCombat = new();

        [Header("Healing")]
        public List<PlayerHealingData> playerHealing = new();

        [Header("Skin")]
        public List<PlayerPrefab> playerSkin = new();

        [Header("Map And Customer")]
        public List<UpgradeCustomer> upgradeCustomers = new();
    }

    [System.Serializable]
    public class PlayerSpeedData
    {
        public string name = "Level :";

        public int Level;

        public float Speed;

        public int CostUpgrade;
    }

    [System.Serializable]
    public class PlayerRollCooldownData
    {
        public string name = "Level :";

        public int Level;

        public float RollCooldown;

        public int CostUpgrade;
    }

    [System.Serializable]
    public class PlayerCombatData
    {
        public string name = "Level :";

        public int Level;

        public int StunRate;
        public float StunMaxTime;

        public int Damage;
        public float AttackSpeed;

        public int CostUpgrade;
    }

    [System.Serializable]
    public class PlayerHealingData
    {
        public string name = "Level :";

        public int Level;

        public float HealRate;
        public float HealRange;

        public int CostUpgrade;
    }

    [System.Serializable]
    public class UpgradeCustomer
    {
        public string name = "Level :";

        public int Level;

        public float MaxHealthTime;
        public GameObject MapUpgradePrefab;

        public int CostUpgrade;
    }

    [System.Serializable]
    public class PlayerPrefab
    {
        public string name;
        public LocalizedString displayName;
        public int skinPrice;
        public Sprite skinProfile;
        public GameObject playerPrefab;
    }
}
