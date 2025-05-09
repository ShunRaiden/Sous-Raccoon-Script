//OutS
using EZCameraShake;
//
using SousRaccoon.CameraMove;
using SousRaccoon.Data;
using SousRaccoon.Data.Item;
using SousRaccoon.Kitchen;
using SousRaccoon.Player;
using SousRaccoon.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SousRaccoon.Manager
{
    public class StageManager : MonoBehaviour
    {
        #region Singleton
        public static StageManager instance { get { return _instance; } }
        private static StageManager _instance;

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
                //DontDestroyOnLoad(this.gameObject);
            }
        }
        #endregion

        [Header("Player")]
        //Player
        public GameObject playerPrefab;
        public PlayerKitchenAction playerKitchenAction;
        public PlayerInputManager playerInputManager;
        public PlayerCombatSystem playerCombatSystem;
        public UseItemGenerator useItemGenerator;

        //UI
        public UIPlayerPanel playerPanel;

        [Space(2)]
        [Header("Camera")]
        public CameraMovement cameraMovement;
        public CameraZoom cameraZoom;

        [Space(2)]
        [Header("Chef")]
        public ChefAI chefAI;
        public GameObject chefGuide;
        public bool hasChefAngry;
        private Coroutine currentChefInsaneCoroutine; // เก็บ `Coroutine` ปัจจุบัน

        [Space(2)]
        [Header("State Manager")]
        public int stageCurrentIndex;

        public SpawnerManager spawnerManager;
        public SceneManagement sceneManagement;
        public List<NPCSpawnSet> npcSpawnSet;
        public ItemListInState itemList;
        public UIPerkInfoListPanel uIPerkInfoListPanel;
        public UIStageDebuffPanel uIStageDebuffPanel;
        public RandomMapManager randomMapManager;

        public event Action OnChefRageEvent;

        [Header("Table")]
        public List<Table> tables = new List<Table>(); // รายการโต๊ะทั้งหมดในร้าน
        public List<GameObject> tablesBroke = new List<GameObject>();

        [Space(2)]
        [Header("Audio")]
        public AudioManager audioManager;
        public VolumeSetting volumeSetting;
        public AudioSource waterWave;

        [Space(2)]
        [Header("Days and Time")]
        public GameObject fadeInStage;
        public TMP_Text daysCountText;

        [Space(3)]
        [Header("Lose Condition")]
        public int customerSpawnCount;
        public int customerAmount;

        public int maxLoseRate;
        public int currentLoseRate;

        public int currentCustomerDieCount;
        public Animator hurtVignetteAnim;

        [Space(3)]
        [Header("Win Condition")]
        public int minimumFoodDeliver;
        public int foodDeliverAmount;

        public TMP_Text foodDeliverAmountText;

        public GameObject winPanel;
        public TMP_Text countDayText;
        public TMP_Text countNextDayText;
        public GameObject fadeWinDay;

        [Space(3)]
        [Header("GameEnding")]
        public UIChefFireRage fireRageSet;
        public Transform fireRageLayout;
        public List<UIChefFireRage> chefRageList;
        public GameObject chefRageWarning;
        public GameObject losePanel;
        public GameObject loseCutScenePrefab;
        public PlayableDirector loseCutSceneControl;

        public GameObject gameOverPanel;
        public GameObject gameOverCutScenePerfab;
        public PlayableDirector gameOverCutSceneControl;

        public GameObject hudPanel;

        public GameObject backToLobbyButton;

        public bool isTutorial = false;

        public bool isGameStart = false;
        public bool isGameLose = false;
        public bool isGameWin = false;
        public bool isGameEnd = false;
        public bool canPause = true;
        public bool isEndDay = false;
        public bool onNextDays = false;

        public event Action EventOnGameEnd;

        [Space(3)]
        [Header("Shop Merchant")]
        public ShopMerchantManager shopMerchantManager;

        [Space(4)]
        [Header("Scenario")]
        public StageScenarioManager stageScenarioManager;

        [Space(3)]
        [Header("Pause Game")]
        public bool isGamePaused = false;
        public GameObject pauseMenuUI;
        [SerializeField] GameObject pauseNodeUI;
        public GameObject settingNodeUI;
        public GameObject keyBindsNodeUI;
        public GameObject menuGuideNodeUI;

        [Space(3)]
        [Header("Info")]
        public bool isOpenInfo = false;

        //Event UI
        public event Action<int> OnPlayerDetectZoomZoneEvent;

        [Space(2)]
        [Header("Currency")]
        public UIAddMoney addCurrencyPrefab;
        [Space(1)]
        [Header("Money")]
        public TMP_Text moneyText;
        public int currentMoney = 0;
        public Transform addMoneySpawnPoint;
        [Space(1)]
        [Header("Coin UI")]
        public TMP_Text playerCoinText;
        public Transform addCoinSpawnPoint;

        [Space(3)]
        [Header("Position")]
        public Transform customerSpawnPoint;
        public Transform customerWaitPos;
        public Transform customerEndPoint;

        [Space(1)]
        [Header("Spawner")]

        public Transform playerSpawnPos;
        public int spawnQuantity;

        [Space(3)]
        [Header("Summary")]
        public GameObject summaryPanel;
        public TMP_Text summaryMoneyText;
        public TMP_Text summaryCoinText;
        public TMP_Text summaryOrdersDeliveredText;
        public TMP_Text summaryOrdersFailedText;
        public TMP_Text summaryMonstersDefeatedText;

        List<Button> buttonsList = new();

        private async void Start()
        {
            fadeInStage.SetActive(true);

            playerPrefab = Instantiate(GameManager.instance.playerPrefab, playerSpawnPos.position, Quaternion.identity);

            playerKitchenAction = playerPrefab.GetComponent<PlayerKitchenAction>();
            playerInputManager = playerPrefab.GetComponent<PlayerInputManager>();
            playerCombatSystem = playerPrefab.GetComponent<PlayerCombatSystem>();

            volumeSetting.StartSetVolume();

            audioManager.PlayMusic("BG-BeforeState");
            waterWave.Play();

            spawnerManager.npcSpawnSet = npcSpawnSet[UnityEngine.Random.Range(0, npcSpawnSet.Count)];

            SpawnFireChefRage(maxLoseRate);

            //SetRunStageLoseRate();
            chefAI.player = playerKitchenAction;

            moneyText.text = $"{RunStageManager.instance.summaryMoney}";
            SetCoinText(RunStageManager.instance.PlayerCoin);

            daysCountText.text = $"Day {1 + RunStageManager.instance.daysCount}";

            isGameEnd = false;
            isGameLose = false;
            isGameWin = false;
            isGamePaused = false;
            canPause = true;
            onNextDays = false;

            //minimumFoodDeliver += (RunStageManager.instance.foodDeliverAdder * RunStageManager.instance.daysCount);
            //foodDeliverAmountText.text = $"0/{minimumFoodDeliver}";

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (GameManager.instance.currentLevelIndex <= 0)
            {
                Debug.LogError("In-Test-Scene");
                GameManager.instance.GetSceneDebugStage(stageCurrentIndex);
            }

            itemList = GameManager.instance.allItemDataEachStage[stageCurrentIndex - 1].itemListInStates[RunStageManager.instance.daysCount];
            spawnerManager.npcSpawnSet = GameManager.instance.allItemDataEachStage[stageCurrentIndex - 1].nPCSpawnSets[RunStageManager.instance.daysCount];
            spawnerManager.monsterStatData = GameManager.instance.allItemDataEachStage[stageCurrentIndex - 1].monsterStatDataBases[RunStageManager.instance.daysCount];

            foreach (var button in FindObjectsOfType<Button>(true))
            {
                button.onClick.AddListener(AudioManager.instance.PlaySoundButtonClick);
                buttonsList.Add(button);
            }

            foreach (var trigger in FindObjectsOfType<EventTrigger>(true))
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { AudioManager.instance.PlaySoundButtonHover(); });

                trigger.triggers.Add(entry);
            }

            await AudioManager.instance.LoadStageAudio();
        }

        private void OnDestroy()
        {
            foreach (var button in buttonsList)
            {
                button.onClick.RemoveListener(AudioManager.instance.PlaySoundButtonClick);
            }
        }

        public void GameStart()
        {
            if (isTutorial)
            {
                if (TutorialManager.instance.canStart)
                {
                    chefAI.PlayAnimTarget("Pointing");
                    chefAI.currentAnimation = "Pointing";
                    spawnerManager.StartSpawning();
                    chefGuide.SetActive(false);
                }
                return;
            }

            chefAI.PlayAnimTarget("Pointing");
            chefAI.currentAnimation = "Pointing";

            AudioManager.instance.PlayStageSFXOneShot("StartStage");
            AudioManager.instance.PlayStageSFXOneShot("Chef_Smash_Table");

            spawnerManager.StartSpawning();
            chefGuide.SetActive(false);
        }

        #region NPC

        // ฟังก์ชันสำหรับดึงข้อมูลเก้าอี้ที่ว่างในโต๊ะ
        public Chair GetAvailableChair(out Table currentTable)
        {
            currentTable = null;

            // รวบรวมเฉพาะโต๊ะที่มีเก้าอี้ว่าง
            List<Table> availableTables = new();
            foreach (var table in tables)
            {
                if (table.HasAvailableChair()) // สมมติว่า Table มีเมธอดนี้ไว้เช็คว่ามีเก้าอี้ว่างหรือไม่
                {
                    availableTables.Add(table);
                }
            }

            if (availableTables.Count > 0)
            {
                // สุ่มเลือกโต๊ะ
                currentTable = availableTables[UnityEngine.Random.Range(0, availableTables.Count)];

                // สุ่มเลือกเก้าอี้จากโต๊ะที่เลือก
                if (currentTable.TryGetAvailableChair(out Chair selectedChair))
                {
                    return selectedChair;
                }
            }

            return null; // ถ้าไม่พบโต๊ะหรือเก้าอี้ว่าง
        }

        public ItemDataBase GenerateMenu()
        {
            return itemList.OrderItem[UnityEngine.Random.Range(0, itemList.OrderItem.Count)];
        }

        public ItemDataBase GenerateIngredient()
        {
            return itemList.IngredientsItem[UnityEngine.Random.Range(0, itemList.OrderItem.Count)];
        }
        #endregion

        #region Pause System
        // ฟังก์ชันสำหรับหยุดเกม
        public void TogglePause()
        {
            if (stageScenarioManager.isShowPanel)
            {
                stageScenarioManager.uIStageScenarioPanel.ClosePanel();
                return;
            }

            if (isOpenInfo)
            {
                isOpenInfo = false;
                uIPerkInfoListPanel.ClosePerkInfo();
                uIStageDebuffPanel.CloseDebuffInfo();

                if (!shopMerchantManager.isShopOpen || !stageScenarioManager.isShowPanel)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }

                return;
            }

            if (shopMerchantManager.isShopOpen)
            {
                shopMerchantManager.CloseShop();
                return;
            }

            if (!canPause) return;

            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            playerInputManager.isPauseGame = true;
            pauseMenuUI.SetActive(true); // แสดง Pause Menu
            pauseNodeUI.SetActive(true);
            Time.timeScale = 0f;         // หยุดเวลา
            isGamePaused = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ResumeGame()
        {
            playerInputManager.isPauseGame = false;
            pauseMenuUI.SetActive(false); // ซ่อน Pause Menu
            settingNodeUI.SetActive(false);
            keyBindsNodeUI.SetActive(false);
            menuGuideNodeUI.SetActive(false);
            Time.timeScale = 1f;          // กลับมาเล่นต่อ
            isGamePaused = false;

            if (!isGameEnd)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        #endregion

        #region Game Information
        public void ToggleInfo()
        {
            if (isEndDay) return;

            if (!isOpenInfo)
            {
                uIPerkInfoListPanel.OpenPerkInfo();
                uIStageDebuffPanel.OpenDebuffPanel();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                isOpenInfo = true;
            }
            else
            {
                uIPerkInfoListPanel.ClosePerkInfo();
                uIStageDebuffPanel.CloseDebuffInfo();

                if (!shopMerchantManager.isShopOpen)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                isOpenInfo = false;
            }
        }

        public void OnChangeSizeUIEvent(int index)
        {
            OnPlayerDetectZoomZoneEvent?.Invoke(index);
        }
        #endregion

        #region Win Condition
        public void UpdateWinRate()
        {
            if (isGameLose) return;

            customerAmount++;

            if (spawnerManager.currentPhase == SpawnerManager.TimePhase.Night)
            {
                isGameEnd = true;
                //TODO : Chef Stop Doing Menu Go to Last Phase

                if (customerAmount == customerSpawnCount)
                {
                    // Game Win Event
                    EventOnGameEnd?.Invoke();

                    AudioManager.instance.PlayMusic("BG-AfterState");

                    shopMerchantManager.SpawnMerchant();

                    chefAI.canvasIngredients.SetActive(false);
                    chefGuide.SetActive(true);

                    isGameWin = true;
                    /*
                    if (foodDeliverAmount >= minimumFoodDeliver)
                    {
                    }
                    else
                    {
                        HandleGameLose();
                    }
                    */
                }
            }
        }

        public void EndDay()
        {
            RunStageManager.instance.daysCount++;

            if (RunStageManager.instance.daysCount >= RunStageManager.instance.maxDay)
            {
                //Summary and Win Cutscene
                chefAI.PlayAnimTarget("Pointing");
                StartCoroutine(OnGameOver());

                if (GameManager.instance.playerSaveData.StageUnlock < stageCurrentIndex)
                {
                    GameManager.instance.SaveUnlockStage(stageCurrentIndex);
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                playerInputManager.SetMoveStage(true);

                randomMapManager.SetUpPanel();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            uIPerkInfoListPanel.ClosePerkInfo();
            uIStageDebuffPanel.CloseDebuffInfo();

            isEndDay = true;
        }

        public void MoveNextDay(string nameStage)
        {
            if (onNextDays) return;

            onNextDays = true;

            RunStageManager.instance.currentLoseRate = currentLoseRate;
            chefAI.PlayAnimTarget("Pointing");
            countDayText.text = $"Day {RunStageManager.instance.daysCount}";
            countNextDayText.text = $"Day {RunStageManager.instance.daysCount + 1}";

            StartCoroutine(OnNextDays(nameStage));

            Debug.LogError("Move Day");
        }
        #endregion

        #region Lose Condition
        /// <summary>
        /// Update Lose Condition
        /// </summary>
        public void UpdateLoseRate(int rateIndex)
        {
            if (isGameWin || isGameLose) return;

            // เพิ่มค่า currentLoseRate และป้องกันไม่ให้เกิน maxLoseRate
            currentLoseRate = Mathf.Clamp(currentLoseRate + rateIndex, 0, maxLoseRate);

            // อัปเดตการแสดงผลของ chefRageList
            UpdateChefRageUI();
            UpdateOrderFailed();

            // คำนวณอัตรา Rage
            float rageRate = (float)currentLoseRate / maxLoseRate;

            // เช็คแพ้เกม
            if (currentLoseRate >= maxLoseRate)
            {
                // Game Win Event
                EventOnGameEnd?.Invoke();
                HandleGameLose();
                return;
            }

            // เช็คว่า Chef ควรเข้าสู่โหมด Angry หรือไม่
            if (rageRate >= 0.6f && !hasChefAngry)
            {
                HandleChefRage();
            }
        }

        /// <summary>
        /// อัปเดต UI ของ Chef Rage List
        /// </summary>
        private void UpdateChefRageUI()
        {
            for (int i = 0; i < chefRageList.Count; i++)
            {
                chefRageList[i].fireRage.SetActive(i < currentLoseRate);
            }
        }

        public void SpawnFireChefRage(int spawnIndex)
        {
            for (int i = 0; i < spawnIndex; i++)
            {
                chefRageList.Add(Instantiate(fireRageSet, fireRageLayout));
            }
        }

        public void RemoveFireChefRage(int removeIndex)
        {
            for (int i = 0; i < removeIndex; i++)
            {
                Destroy(chefRageList[i].gameObject);
                chefRageList.RemoveAt(i);
            }
        }

        /// <summary>
        /// จัดการสถานะเมื่อเกมแพ้
        /// </summary>
        public void HandleGameLose()
        {
            CameraShaker.Instance.ShakeOnce(4f, 4f, .5f, .5f);
            chefAI.PlayAnimTarget("GameOver");
            AudioManager.instance.PlayStageSFXOneShot("Chef_Grunting");
            chefAI.SetAngryAura();
            chefAI.fireChef_VFX.SetActive(true);
            isGameLose = isGameEnd = true;
            cameraMovement.LookAtChef();
            StartCoroutine(OnGameLose());
        }

        /// <summary>
        /// ทำให้ Chef โกรธ
        /// </summary>
        private void HandleChefRage()
        {
            hasChefAngry = true;
            OnChefRageEvent?.Invoke();

            chefRageWarning.SetActive(true);
            chefAI.PlayAngryAnimation("Angry");

            CameraShaker.Instance.ShakeOnce(4f, 4f, .5f, .5f);
            TriggerChefInsane();
        }

        /// <summary>
        /// ตั้งค่า Lose Rate จาก RunStageManager
        /// </summary>
        public void SetRunStageLoseRate()
        {
            currentLoseRate = RunStageManager.instance.currentLoseRate;
            UpdateChefRageUI();
        }

        private IEnumerator OnGameLose()
        {
            AudioManager.instance.StopMusic();
            yield return new WaitForSeconds(2);
            playerPrefab.SetActive(false);
            loseCutSceneControl.gameObject.SetActive(true);
            loseCutSceneControl.Play();
            yield return new WaitUntil(() => loseCutSceneControl.state != PlayState.Playing);
            OnGameSummary();
            losePanel.SetActive(true);
            loseCutScenePrefab.SetActive(true);
            hudPanel.SetActive(false);
            summaryPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private IEnumerator OnNextDays(string stageName)
        {
            GameManager.instance.AddMoney(currentMoney);
            AudioManager.instance.StopMusic();
            AudioManager.instance.PlayStageSFXOneShot("Change_Day");
            yield return new WaitForSeconds(1.5f);
            winPanel.SetActive(true);
            yield return new WaitForSeconds(3.5f);
            sceneManagement.NextWinStage(stageName);
        }

        private IEnumerator OnGameOver()
        {
            AudioManager.instance.StopMusic();
            yield return new WaitForSeconds(1f);
            playerPrefab.SetActive(false);
            gameOverCutSceneControl.gameObject.SetActive(true);
            gameOverCutSceneControl.Play();
            yield return new WaitUntil(() => gameOverCutSceneControl.state != PlayState.Playing);
            OnGameSummary();
            gameOverPanel.SetActive(true);
            gameOverCutScenePerfab.SetActive(true);
            hudPanel.SetActive(false);
            summaryPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void TriggerChefInsane()
        {
            // ถ้ามี Coroutine เก่ากำลังทำงานอยู่ ให้หยุดก่อน
            if (currentChefInsaneCoroutine != null)
            {
                StopCoroutine(currentChefInsaneCoroutine);
            }

            // เริ่ม Coroutine ใหม่และบันทึกไว้
            currentChefInsaneCoroutine = StartCoroutine(OnChefInsane());
        }

        private IEnumerator OnChefInsane()
        {
            float elapsedTime = 0f;

            chefAI.SetInsane();

            while (elapsedTime < chefAI.chefInsaneTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null; // รอในเฟรมถัดไป
            }

            chefAI.ResetInsane();

            // เมื่อ Coroutine ทำงานเสร็จ ให้เคลียร์ตัวแปร
            currentChefInsaneCoroutine = null;
        }
        #endregion

        #region Currency
        public void AddMoney(int amount)
        {
            currentMoney += amount;
            RunStageManager.instance.summaryMoney += amount;

            var spawn = Instantiate(addCurrencyPrefab, addMoneySpawnPoint);
            spawn.GetComponent<UIAddMoney>().amount = amount;

            StartCoroutine(OnCurrencyTextUpdate(moneyText, RunStageManager.instance.summaryMoney.ToString()));
        }

        public void UpdateFoodDeliver()
        {
            //foodDeliverAmount++;
            RunStageManager.instance.summaryOrdersDerivered++;
            //foodDeliverAmountText.text = $"{foodDeliverAmount}/{minimumFoodDeliver}";
        }

        public void UpdateOrderFailed()
        {
            RunStageManager.instance.summaryOrdersFailed++;
        }

        public void UpdateMonsterDefeated()
        {
            RunStageManager.instance.summaryMonstersDefeated++;
        }

        public void OnGameSummary()
        {
            summaryMoneyText.text = RunStageManager.instance.summaryMoney.ToString();
            summaryCoinText.text = RunStageManager.instance.summaryCoin.ToString();
            summaryOrdersDeliveredText.text = RunStageManager.instance.summaryOrdersDerivered.ToString();
            summaryOrdersFailedText.text = RunStageManager.instance.summaryOrdersFailed.ToString();
            summaryMonstersDefeatedText.text = RunStageManager.instance.summaryMonstersDefeated.ToString();

            GameManager.instance.SaveRunComplete(1);

            GameManager.instance.AddMoney(currentMoney);
        }

        public void SetCoinText(int playerCoin)
        {
            playerCoinText.text = $"{playerCoin}";
        }

        public void AddCoinText(int coinCurrent, int coinAmount)
        {
            var spawn = Instantiate(addCurrencyPrefab, addCoinSpawnPoint);
            spawn.GetComponent<UIAddMoney>().amount = coinAmount;

            StartCoroutine(OnCurrencyTextUpdate(playerCoinText, coinCurrent.ToString()));
        }

        IEnumerator OnCurrencyTextUpdate(TMP_Text textObject, string textToSet)
        {
            yield return new WaitForSeconds(0.5f);

            textObject.text = textToSet;
        }
        #endregion
    }
}