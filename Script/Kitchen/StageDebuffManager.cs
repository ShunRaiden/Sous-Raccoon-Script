using SousRaccoon.Data;
using SousRaccoon.Kitchen;
using SousRaccoon.Player;
using SousRaccoon.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class StageDebuffManager : MonoBehaviour
    {
        public List<StageDebuffDataBase> currentDataList = new List<StageDebuffDataBase>();

        private Dictionary<DebuffStatType, Action<DebuffStatModifier>> debuffStatUpdateActions;

        [SerializeField] UIStageDebuffPanel debuffPanel;

        private void Start()
        {

            // Initialize the dictionary with stat update methods
            debuffStatUpdateActions = new Dictionary<DebuffStatType, Action<DebuffStatModifier>>()
            {
                { DebuffStatType.MoveSpeed, ApplyMoveSpeed },
                { DebuffStatType.RollCDR, ApplyRollCoolDown },
                { DebuffStatType.RollRange, ApplyRollRange },
                { DebuffStatType.ATKSpeed, ApplyAttackSpeed },
                { DebuffStatType.HealRange, ApplyHealRange },
                { DebuffStatType.ChefAngry, ApplyAngryLimitChef },
                { DebuffStatType.TableCount, ApplyTableCount },
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
                (FindObjectOfType<StageScenarioManager>().scenarioSet || !FindObjectOfType<StageScenarioManager>().hasSceneario) &&
                FindObjectOfType<ShopMerchantManager>().hasPerkSetup);

            LoadDebuffStatRunStage();
        }

        public void LoadDebuffStatRunStage()
        {
            // Load Stat จาก RunStage
            currentDataList = RunStageManager.instance.currentstageDebuffs;

            for (int i = 0; i < currentDataList.Count; i++)
            {
                var debuff = currentDataList[i];
                debuffPanel.SetupDebuffInfoSlot(debuff, i);
                UpStatTarget(debuff.statModifiers);
            }
        }

        private void UpStatTarget(DebuffStatModifier statModifiers)
        {
            debuffStatUpdateActions[statModifiers.stat].Invoke(statModifiers);
        }

        #region Apply
        private void ApplyMoveSpeed(DebuffStatModifier modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.movementSpeedRunStage += modifier.floatValue;
        }

        private void ApplyRollCoolDown(DebuffStatModifier modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.rollCooldownRunStage += modifier.floatValue;
        }

        private void ApplyRollRange(DebuffStatModifier modifier)
        {
            var playerLocomotion = FindObjectOfType<PlayerLocomotion>();
            playerLocomotion.runStageRollSpeed += modifier.floatValue;
        }

        private void ApplyAttackSpeed(DebuffStatModifier modifier)
        {
            var playerCombat = FindObjectOfType<PlayerCombatSystem>();
            playerCombat.attackSpeedRunStage += modifier.floatValue;
        }

        private void ApplyHealRange(DebuffStatModifier modifier)
        {
            var playerHealing = FindObjectOfType<PlayerHealingDanceSystem>();
            playerHealing.currentHealRange += modifier.floatValue;
        }

        private void ApplyAngryLimitChef(DebuffStatModifier modifier)
        {
            StageManager.instance.maxLoseRate -= modifier.intValue;
            StageManager.instance.RemoveFireChefRage(modifier.intValue);
        }

        private void ApplyTableCount(DebuffStatModifier modifier)
        {
            int removeCount = Mathf.Min(modifier.intValue, StageManager.instance.tables.Count);

            for (int i = 0; i < removeCount; i++)
            {
                if (StageManager.instance.tables.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, StageManager.instance.tables.Count); // สุ่ม Index ของโต๊ะ
                    StageManager.instance.tables[randomIndex].gameObject.SetActive(false);
                    StageManager.instance.tablesBroke[randomIndex].gameObject.SetActive(true);
                    StageManager.instance.tablesBroke.RemoveAt(randomIndex);
                    StageManager.instance.tables.RemoveAt(randomIndex);
                }
            }
        }
        #endregion
    }
}