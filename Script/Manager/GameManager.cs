using SousRaccoon.Data;
using SousRaccoon.Data.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SousRaccoon.Manager
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager instance { get { return _instance; } }
        private static GameManager _instance;

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
            }

            if (isDebug)
                Testing();
        }
        #endregion

        public event Action OnChangeLanguageEvent;

        public SceneManagement sceneManagement;

        [Header("Player Data")]
        public PlayerDataBase playerDataBase;
        public PlayerSaveData playerSaveData;

        [Header("Asset Data")]
        public SpriteResourceSO spriteResource;

        public GameObject playerPrefab;

        public int currentSaveSlot;

        [Header("Stage List")]
        public StageListDataSO stageListData;

        public int currentLevelIndex; //Use For Check Level Index

        public List<AllItemDataEachList> allItemDataEachStage;

        public void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
        }

        #region Currency
        public int PlayerMoney { get; private set; }

        public bool isDebug;
        public bool isDemo;

        public void Testing()
        {
            PlayerSaveData saveData = new PlayerSaveData
            {
                LevelSpeed = 0,
                LevelRollCooldown = 0,
                LevelCombat = 0,
                LevelHeal = 0,
                LevelCustomer = 0,
                Skin = 0,
                MoneyCurrency = 0,
            };

            playerSaveData = saveData;
        }

        public string GetSceneDebugStage(int levelIndex)
        {
            currentLevelIndex = levelIndex - 1;
            return stageListData.stageLists[1].sceneNameList[levelIndex - 1];
        }

        public string GetSceneLevel(int levelIndex)
        {
            currentLevelIndex = levelIndex;
            return stageListData.stageLists[levelIndex].sceneNameList[0];
        }

        public void SetCurrency()
        {
            PlayerMoney = PlayerPrefs.GetInt($"PLAYER_MONEY_CURRENCY_SAVE_{currentSaveSlot}");

            SaveMoney();
        }

        public void AddMoney(int money)
        {
            PlayerMoney += money;
            SaveMoney();
        }

        public void RemoveMoney(int money)
        {
            PlayerMoney -= money;
            SaveMoney();
        }

        private void SaveMoney()
        {
            playerSaveData.MoneyCurrency = PlayerMoney;

            PlayerPrefs.SetInt($"PLAYER_MONEY_CURRENCY_SAVE_{currentSaveSlot}", PlayerMoney);
        }

        public void SaveUnlockStage(int stageUnlockIndex)
        {
            playerSaveData.StageUnlock = stageUnlockIndex;

            PlayerPrefs.SetInt($"PLAYER_STATE_UNLOCK_SAVE_{currentSaveSlot}", stageUnlockIndex);
        }

        public void SaveRunComplete(int runCompleteIndex)
        {
            playerSaveData.RunComplete += runCompleteIndex;

            PlayerPrefs.SetInt($"RUN_COMPLETE_{currentSaveSlot}", playerSaveData.RunComplete);
        }
        #endregion

        public void GetRandomMap(int stageIndex, out string oldMapName, out int oldMapIndex, out string newMapName, out int newMapIndex)
        {
            // ��˹���� oldMapName ��͹
            string currentStage = SceneManager.GetActiveScene().name;
            oldMapName = currentStage;

            // �� Index �ͧ Stage �Ѩ�غѹ� List
            oldMapIndex = stageListData.stageLists[stageIndex].sceneNameList.IndexOf(oldMapName);

            // ��ͧ੾�� Stage �������� Stage �Ѩ�غѹ
            List<string> availableStages = stageListData.stageLists[stageIndex].sceneNameList
                .Where(stage => stage != currentStage) // ������᷹ oldMapName
                .ToList();

            // ������� Stage ��������������
            if (availableStages.Count == 0)
            {
                Debug.LogWarning("����� Stage �������!");
                newMapName = null;
                newMapIndex = -1; // �� -1 ���ͺ͡�������մ�ҹ�������
                return;
            }

            // ���� Stage �ҡ List ��������
            newMapName = availableStages[UnityEngine.Random.Range(0, availableStages.Count)];

            // �� Index �ͧ Stage ���������
            newMapIndex = stageListData.stageLists[stageIndex].sceneNameList.IndexOf(newMapName);
        }

        public void OnUpdateLanguage()
        {
            OnChangeLanguageEvent?.Invoke();
        }

        public void AddOrUpdateEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
        {
            var entry = trigger.triggers.FirstOrDefault(e => e.eventID == type);
            if (entry == null)
            {
                entry = new EventTrigger.Entry { eventID = type };
                trigger.triggers.Add(entry);
            }

            // ��ͧ�ѹ������� Callback ��� (���͡���Ҩ���)
            if (!entry.callback.GetPersistentEventCount().Equals(0))
            {
                for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
                {
                    var methodName = entry.callback.GetPersistentMethodName(i);
                    if (methodName == action.Method.Name)
                        return; // ����������
                }
            }

            entry.callback.AddListener((data) => action.Invoke());
        }

        public void ChangeUISelectNav(GameObject newButtonUI)
        {
            // ��ҧ Selection ��͹ ���ͻ�ͧ�ѹ��꡺ҧ UI ����������¹
            EventSystem.current.SetSelectedGameObject(null);

            // ���͡ UI ����á
            EventSystem.current.SetSelectedGameObject(newButtonUI);
        }
    }
}
