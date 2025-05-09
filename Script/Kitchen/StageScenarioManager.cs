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
    public class StageScenarioManager : MonoBehaviour
    {
        public List<ScenarioDataBase> scenarioDataBases;

        private Dictionary<ScenarioType, Action<ScenarioDataBase>> statUpdateActions;

        public UIStageScenarioPanel uIStageScenarioPanel;

        public bool isShowPanel;
        public bool scenarioSet;

        public bool hasSceneario = true;

        // Start is called before the first frame update
        void Start()
        {
            if (!hasSceneario) return;

            scenarioSet = false;

            if (RunStageManager.instance.daysCount > 0)
            {
                statUpdateActions = new Dictionary<ScenarioType, Action<ScenarioDataBase>>()
                {
                    { ScenarioType.ChefRage, ApplyChefRage },
                    { ScenarioType.ChefSpeed, ApplyChefSpeed },
                    { ScenarioType.RaccoonHealRate, ApplyRaccoonHealRate },
                    { ScenarioType.MonsterSpawnRate, ApplyMonsterSpawnRate },
                    { ScenarioType.CustomerSpawnRate, ApplyCustomerSpawnRate },
                    { ScenarioType.TableCount, ApplyTableCount },
                };

                StartCoroutine(WaitAllThingSet());
            }
        }

        private IEnumerator WaitAllThingSet()
        {
            yield return new WaitUntil(() =>
                FindObjectOfType<PlayerCombatSystem>() != null &&
                FindObjectOfType<PlayerLocomotion>() != null &&
                FindObjectOfType<SpawnerManager>() != null &&
                FindObjectOfType<ChefAI>() != null);

            RandomScenario();
        }

        public void RandomScenario()
        {
            var scenarioChance = UnityEngine.Random.Range(0, 2);
            if (scenarioChance == 0)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                var scenario = scenarioDataBases[UnityEngine.Random.Range(0, scenarioDataBases.Count)];
                UpStatTarget(scenario);
                FindObjectOfType<PlayerInputManager>().SetMoveStage(true);
                isShowPanel = true;
            }

            scenarioSet = true;
        }

        // This method now handles the stat modifications dynamically
        private void UpStatTarget(ScenarioDataBase scenarioData)
        {
            // Find the appropriate action for the stat and apply it
            if (statUpdateActions.ContainsKey(scenarioData.type))
            {
                statUpdateActions[scenarioData.type].Invoke(scenarioData);
                uIStageScenarioPanel.SetupPanel(scenarioData.icon, scenarioData.nameScenarioLocalized, scenarioData.discriptionLocalized, scenarioData.statIcon, scenarioData.statValue, scenarioData.headIcon);
                uIStageScenarioPanel.content.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"No action found for stat: {scenarioData.type}");
            }
        }

        public void ApplyChefRage(ScenarioDataBase scenarioData)
        {
            StageManager.instance.maxLoseRate += scenarioData.intValue[0];

            if (scenarioData.intValue[0] > 0)
                StageManager.instance.SpawnFireChefRage(scenarioData.intValue[0]);
            else
                StageManager.instance.RemoveFireChefRage(-scenarioData.intValue[0]);
        }

        public void ApplyChefSpeed(ScenarioDataBase scenarioData)
        {
            var chef = FindObjectOfType<ChefAI>();
            chef.scenarioSpeedMultiply = scenarioData.floatValue[0];
        }

        public void ApplyRaccoonHealRate(ScenarioDataBase scenarioData)
        {
            var playerHealing = FindObjectOfType<PlayerHealingDanceSystem>();
            playerHealing.scenarioHealRate = scenarioData.floatValue[0];
            playerHealing.ResultHealRate();
        }

        public void ApplyMonsterSpawnRate(ScenarioDataBase scenarioData)
        {
            var spawner = FindObjectOfType<SpawnerManager>();
            spawner.minNextMonsterSpawnTime += scenarioData.floatValue[0];
            spawner.maxNextMonsterSpawnTime += scenarioData.floatValue[1];
        }

        public void ApplyCustomerSpawnRate(ScenarioDataBase scenarioData)
        {
            var spawner = FindObjectOfType<SpawnerManager>();
            spawner.minNextCustomerSpawnTime += scenarioData.floatValue[0];
            spawner.maxNextCustomerSpawnTime += scenarioData.floatValue[1];
        }

        private void ApplyTableCount(ScenarioDataBase scenarioData)
        {
            int removeCount = Mathf.Min(scenarioData.intValue[0], StageManager.instance.tables.Count);

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
    }
}
