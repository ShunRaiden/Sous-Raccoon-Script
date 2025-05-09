using SousRaccoon.Data;
using SousRaccoon.Kitchen;
using SousRaccoon.Player;
using SousRaccoon.UI.ShopMerchant;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class ShopMerchantManager : MonoBehaviour
    {
        [SerializeField] private GameObject merchantPrefab;
        [SerializeField] private Transform merchantSpawnPoint;
        [SerializeField] private float merchantLookAtTime;

        [Header("UI")]
        [SerializeField] private UIShopMerchantPanel merchantPanel;
        [SerializeField] private GameObject contentShop;

        public ShopMerchantItemDataBase coinExchangePerk;

        // เปลี่ยนให้ Action รับ int เพิ่ม
        private Dictionary<StatType, Action<List<StatModifier>>> statUpdateActions;

        public BarricadeManager barricadeManager;

        public HelperGiverAI helperGiverAIPref; // Assign By Prefab

        public Transform helperSpawnPoint;
        public Transform helperTrashPoint;

        public HelperGiverAI currentHelperGiver;

        public bool isShopOpen;
        public bool hasMoveIGD;

        [HideInInspector] public bool hasPerkSetup = false;

        private void Start()
        {
            barricadeManager = FindAnyObjectByType<BarricadeManager>();

            // Initialize the dictionary with stat update methods
            statUpdateActions = new Dictionary<StatType, Action<List<StatModifier>>>()
            {
                { StatType.MoveSpeed, ApplyMoveSpeed },
                { StatType.RollCDR, ApplyRollCoolDown },
                { StatType.RollRange, ApplyRollRange },
                { StatType.ATKSpeed, ApplyAttackSpeed },
                { StatType.ATKDamage, ApplyDamage },
                { StatType.Stun, ApplyStunRate },
                { StatType.HealRate, ApplyHealRate },
                { StatType.HealRange, ApplyHealRange },
                { StatType.TwoHand, ApplyTwoHand },
                { StatType.CustomerMoneyDrop, ApplyCustomerMoneyDrop },
                { StatType.CoinSpwnRate, ApplyCoinSpawnRate },
                { StatType.CookSpeedChef, ApplySpeedChef },
                { StatType.AngryLimitChef, ApplyAngryLimitChef },
                { StatType.Money, ApplyMoney },
                { StatType.BarricadeStat, ApplyBarricadeStat },
                { StatType.HelperGiverStat, ApplyHelperGiverStat },
                { StatType.CoinExchange, ApplyCoinExchange},

            };

            StartCoroutine(WaitAllThingSet());
        }

        private IEnumerator WaitAllThingSet()
        {
            yield return new WaitUntil(() =>
                FindObjectOfType<PlayerCombatSystem>() != null &&
                FindObjectOfType<PlayerLocomotion>() != null &&
                FindObjectOfType<SpawnerManager>() != null &&
                FindObjectOfType<ChefAI>() != null &&
                FindObjectOfType<StageScenarioManager>().scenarioSet || !FindObjectOfType<StageScenarioManager>().hasSceneario);

            LoadStatRunStage();
        }

        public void SpawnMerchant()
        {
            //SpawnMerchant
            merchantPrefab.SetActive(true);
            AudioManager.instance.PlayStageSFXOneShot("Merchant_Spawn");
            LookMerchant();
            SetupShop();
        }

        public void LoadStatRunStage()
        {
            var upgradedPerks = RunStageManager.instance.GetUpgradedPerks();
            foreach (var perk in upgradedPerks)
            {
                Debug.LogError($"Perk: {perk.perkName}, Level: {perk.level}");

                UpStatTarget(perk.perkData.statModifiers[perk.level - 1].modifiers);
            }

            hasPerkSetup = true;
        }

        public void SetupShop()
        {
            if (RunStageManager.instance.daysCount < 6)
                merchantPanel.SetUpShopUI();
            else
                merchantPanel.SetUpLastDayShopUI();
        }

        public void OpenShop()
        {
            isShopOpen = true;
            contentShop.gameObject.SetActive(true);
            StageManager.instance.playerInputManager.SetMoveStage(true);
            StageManager.instance.playerInputManager.PlayerStopAllAction();
            AudioManager.instance.PlayStageSFXOneShot("GetItem");

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseShop()
        {
            isShopOpen = false;
            StageManager.instance.playerInputManager.SetMoveStage(false);
            contentShop.gameObject.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LookMerchant()
        {
            StartCoroutine(OnLookingMerchant());
        }

        IEnumerator OnLookingMerchant()
        {
            StageManager.instance.cameraMovement.LookAtFreeCamTarget(merchantPrefab.transform);
            yield return new WaitForSeconds(merchantLookAtTime);
            StageManager.instance.cameraMovement.LookAtPlayer();
            StageManager.instance.cameraMovement.ResetFreeCam();
        }


        public bool CheckPlayerCanBuy(int price)
        {
            if (price > RunStageManager.instance.PlayerCoin)
                return false;
            else
                return true;
        }

        // OnBuyingPerk method will now be simplified
        public void OnBuyingPerk(ShopMerchantItemDataBase perkData)
        {
            int perkLevel = RunStageManager.instance.GetPerkLevel(perkData.perkName);
            int costIndex = (perkData.typeOfPerk == TypeOfPerk.Status) ? perkLevel : 0;

            // Apply stat modifications
            UpStatTarget(perkData.statModifiers[costIndex].modifiers);

            if (RunStageManager.instance.daysCount < 6)
                RunStageManager.instance.RemoveCoin(perkData.levelPrices[costIndex]);

            if (perkData.typeOfPerk == TypeOfPerk.Status)
            {
                RunStageManager.instance.LevelUpPerk(perkData.perkName);
            }

            merchantPanel.UpdateSlot();
        }

        // This method now handles the stat modifications dynamically
        private void UpStatTarget(List<StatModifier> statModifiers)
        {
            foreach (var modifier in statModifiers)
            {
                // Find the appropriate action for the stat and apply it
                if (statUpdateActions.ContainsKey(modifier.stat))
                {
                    statUpdateActions[modifier.stat].Invoke(statModifiers);
                    break;
                }
                else
                {
                    Debug.LogWarning($"No action found for stat: {modifier.stat}");
                }
            }
        }

        #region Apply
        // Example stat modification methods for each stat type
        private void ApplyMoveSpeed(List<StatModifier> modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.movementSpeedRunStage = modifier[0].floatValue;
        }

        private void ApplyRollCoolDown(List<StatModifier> modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.rollCooldownRunStage = modifier[0].floatValue;
        }

        private void ApplyRollRange(List<StatModifier> modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.runStageRollSpeed = modifier[0].floatValue;
        }

        private void ApplyAttackSpeed(List<StatModifier> modifier)
        {
            var playerCombat = FindObjectOfType<PlayerCombatSystem>();
            playerCombat.attackSpeedRunStage = modifier[0].floatValue;
        }

        private void ApplyDamage(List<StatModifier> modifier)
        {
            var playerCombat = FindObjectOfType<PlayerCombatSystem>();
            playerCombat.runStagePlayerDamageToMonster = modifier[0].intValue;
            playerCombat.SetRunStageStat();
        }

        private void ApplyStunRate(List<StatModifier> modifier)
        {
            var playerCombat = FindObjectOfType<PlayerCombatSystem>();
            var rate = playerCombat.stunRate + modifier[0].intValue;
            playerCombat.currentStunRate = rate;

            var time = playerCombat.stunTimeMax - modifier[0].floatValue;
            playerCombat.stunTimeMax = time;
        }

        private void ApplyHealRate(List<StatModifier> modifier)
        {
            var playerHealing = FindObjectOfType<PlayerHealingDanceSystem>();
            playerHealing.runStageHealRate = modifier[0].floatValue;
            playerHealing.ResultHealRate();
        }

        private void ApplyHealRange(List<StatModifier> modifier)
        {
            var playerHealing = FindObjectOfType<PlayerHealingDanceSystem>();
            var range = playerHealing.healingRadius + modifier[0].floatValue;
            playerHealing.currentHealRange = range;
        }

        private void ApplyTwoHand(List<StatModifier> modifier)
        {
            var playerKitchenAction = FindObjectOfType<PlayerKitchenAction>();
            playerKitchenAction.ActiveTwohand();
        }

        // Roll Dodge       
        /* 
        private void ApplyRollDodge(List<StatModifier> modifier)
        {
            var playerCombat = FindObjectOfType<PlayerCombatSystem>();
            playerCombat.canRollDodge = modifier[0].boolValue;
            playerCombat.dodgeTime = modifier[0].floatValue;
        }
        */
        //

        // Roll Damage
        /*
        private void ApplyRollDamage(List<StatModifier> modifier)
        {
        }
        */
        //

        // Customer Block
        /*
        private void ApplyCustomerBlock(List<StatModifier> modifier)
        {
            var spawner = FindObjectOfType<SpawnerManager>();
            spawner.blockTimes = modifier[0].intValue;
        }
        */
        //

        private void ApplyCustomerMoneyDrop(List<StatModifier> modifier)
        {
            var spawner = FindObjectOfType<SpawnerManager>();
            spawner.customerMoneyDropMax = modifier[0].intValue;
            spawner.customerMoneyDropMid = modifier[1].intValue;
            spawner.customerMoneyDropMin = modifier[2].intValue;
        }

        private void ApplySpeedChef(List<StatModifier> modifier)
        {
            var chef = FindObjectOfType<ChefAI>();
            chef.runStageSpeedMultiply = modifier[0].floatValue;
        }

        private void ApplyAngryLimitChef(List<StatModifier> modifier)
        {
            StageManager.instance.maxLoseRate += modifier[0].intValue;
            StageManager.instance.SpawnFireChefRage(modifier[0].intValue);
        }

        private void ApplyCoinSpawnRate(List<StatModifier> modifier)
        {
            var spawner = FindObjectOfType<SpawnerManager>();
            spawner.SetCoinDropperSpawnTime(modifier[0].floatValue, modifier[1].floatValue);
        }

        private void ApplyMoney(List<StatModifier> modifier)
        {
            StageManager.instance.AddMoney(modifier[0].intValue);
        }

        private void ApplyBarricadeStat(List<StatModifier> modifier)
        {
            barricadeManager.SetUpBarricade(modifier[0].intValue, modifier[0].floatValue);
        }

        private void ApplyHelperGiverStat(List<StatModifier> modifier)
        {
            if (currentHelperGiver == null)
            {
                currentHelperGiver = Instantiate(helperGiverAIPref, helperSpawnPoint.position, helperSpawnPoint.rotation);
            }

            currentHelperGiver.SetStatus(modifier[0].floatValue, modifier[1].floatValue, helperSpawnPoint, helperTrashPoint, hasMoveIGD);
        }

        private void ApplyCoinExchange(List<StatModifier> modifier)
        {
            Debug.LogError("UpStat");
            StageManager.instance.AddMoney(RunStageManager.instance.PlayerCoin / 2);
            RunStageManager.instance.RemoveCoin(RunStageManager.instance.PlayerCoin);
            merchantPanel.UpdateSlot();
        }
        #endregion
    }
}
