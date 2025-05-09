using SousRaccoon.Data;
using SousRaccoon.Lobby;
using SousRaccoon.UI;
using SousRaccoon.UI.MainMenu;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SousRaccoon.Manager
{
    public class LobbyManager : MonoBehaviour
    {
        #region Singleton
        public static LobbyManager instance { get { return _instance; } }
        private static LobbyManager _instance;

        private void Awake()
        {
            Debug.Log("LobbyManager Awake");

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
                playerDataBase = GameManager.instance.playerDataBase;
            }

            if (GameManager.instance.playerPrefab != null)
            {
                StartLobby();
            }
            else
                demoPanel.SetActive(true);
        }
        #endregion

        [Header("Audio")]
        public AudioManager audioManager;
        public VolumeSetting volumeSetting;

        [Header("UI")]
        public UIMainMenuPanel mainMenuPanel;
        [SerializeField] LobbyBook lobbyBook;
        [SerializeField] float timingAnimate;
        [SerializeField] GameObject pauseMenuPanel;
        [SerializeField] GameObject pauseNodeUI;
        [SerializeField] GameObject settingNodeUI;
        [SerializeField] GameObject keyBindsNodeUI;
        [SerializeField] GameObject menuGuideNodeUI;
        [SerializeField] TMP_Text gameVersionText;

        [Header("UI Lobby")]
        [SerializeField] GameObject uiOpenPanelLayout;
        [SerializeField] FadeDestroy fadePanel;

        [Header("Raccoon Anim MainMenu")]
        [SerializeField] GameObject raccoonMainMenu;
        [SerializeField] Animator raccoonMainMenuAnim;

        [Header("Camera")]
        [SerializeField] GameObject containerCamera;
        [SerializeField] GameObject mainMenuCamera;
        [SerializeField] GameObject playerCamera;

        [Header("Player")]
        public GameObject playerPrefab;

        public Transform playerSpawnPos;

        PlayerDataBase playerDataBase;
        Player.PlayerInputManager playerInputManager;

        [Header("Boolean")]
        bool isGamePaused;
        public bool canPause;

        [Header("Debug")]
        [SerializeField] TMP_Text moneyDebugText;
        [SerializeField] TMP_Text coinDebugText;

        [SerializeField] List<Button> buttonsList = new();

        //Demo
        [SerializeField] GameObject demoPanel;

        private void Start()
        {
            canPause = true;
            volumeSetting.StartSetVolume();
            audioManager.PlayMusic("BG-MainMenu");

            Instantiate(fadePanel);
            raccoonMainMenu.SetActive(true);

            gameVersionText.text = SaveManager.GAME_VERSION_TEXT;

            foreach (var button in FindObjectsOfType<Button>(true))
            {
                button.onClick.AddListener(AudioManager.instance.PlaySoundButtonClick);
                buttonsList.Add(button);
            }

            foreach (var trigger in FindObjectsOfType<EventTrigger>(true))
            {
                GameManager.instance.AddOrUpdateEvent(trigger, EventTriggerType.PointerEnter, () => AudioManager.instance.PlaySoundButtonHover());
                GameManager.instance.AddOrUpdateEvent(trigger, EventTriggerType.Select, () => AudioManager.instance.PlaySoundButtonHover());
            }
        }

        private void OnDestroy()
        {
            foreach (var button in buttonsList)
            {
                button.onClick.RemoveListener(AudioManager.instance.PlaySoundButtonClick);
            }
        }

        public void NewGameSaveSlot(int saveSlot)
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
                StageUnlock = 0,
                RunComplete = 0,
                LastTimeSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            saveData.AddDataSkins(GameManager.instance.playerDataBase.playerSkin.Count);
            SaveManager.SaveData(saveSlot, saveData);

            if (TutorialManager.instance != null)
                TutorialManager.instance.hasNewGame = true;

            SetGameManager(saveData, saveSlot);
        }


        public void LoadSaveSlot(int saveSlot)
        {
            PlayerSaveData loadedData = SaveManager.LoadData(saveSlot);

            loadedData.AddDataSkins(GameManager.instance.playerDataBase.playerSkin.Count);

            SetGameManager(loadedData, saveSlot);
        }

        public void RemoveSaveSlot(int saveSlot)
        {
            SaveManager.ClearSaveData(saveSlot);
        }

        private void SetGameManager(PlayerSaveData data, int saveSlot)
        {
            GameManager.instance.playerSaveData = data;
            GameManager.instance.currentSaveSlot = saveSlot;
            GameManager.instance.playerPrefab = playerDataBase.playerSkin[data.Skin].playerPrefab;
            GameManager.instance.SetCurrency();

            StartLobby();
            SaveManager.SaveData(GameManager.instance.currentSaveSlot, GameManager.instance.playerSaveData);
        }

        private void StartLobby()
        {
            StartCoroutine(StartAnimationCamera());

            //Debug
            DeBugCurrency();
        }

        public void DeBugCurrency()
        {
            moneyDebugText.text = GameManager.instance.PlayerMoney.ToString();
        }

        IEnumerator StartAnimationCamera()
        {
            Debug.Log("StartAnimationCamera");

            mainMenuPanel.OnCloseAllPanel();

            // หลังจากแอนิเมชันกล้องเสร็จแล้ว
            SpawnPlayer();

            playerInputManager.isCanInput = false;

            raccoonMainMenuAnim.Play("WalkOut");
            yield return new WaitForSeconds(3);

            raccoonMainMenu.SetActive(false);

            var animator = mainMenuCamera.GetComponent<Animator>();
            animator.Play("MainMenuCameraZoomOut");
            // ตั้งเวลาเริ่มต้น
            float time = 0f;

            // รอจนกว่าจะถึงเวลาที่กำหนด (timingAnimate)
            while (time < timingAnimate)
            {
                // เพิ่มเวลาโดยใช้ deltaTime (เวลาที่ผ่านไประหว่าง frame)
                time += Time.deltaTime;
                yield return null;
            }

            playerCamera.SetActive(true);
            mainMenuCamera.SetActive(false);

            OpenUILobby();

            if (TutorialManager.instance != null && TutorialManager.instance.hasNewGame)
                TutorialManager.instance.StartTutorial();

            // เปิดการรับ input ของ player
            playerInputManager.isCanInput = true;
        }

        public void SpawnPlayer()
        {
            // สร้าง playerPrefab จาก GameManager
            playerPrefab = Instantiate(GameManager.instance.playerPrefab, playerSpawnPos.position, Quaternion.identity);
            playerInputManager = playerPrefab.GetComponent<Player.PlayerInputManager>();
        }

        public void DestroyPlayer()
        {
            playerSpawnPos.position = playerInputManager.gameObject.transform.position;

            playerInputManager = null;

            Destroy(playerPrefab);

            playerPrefab = null;
        }

        private void OpenUILobby()
        {
            uiOpenPanelLayout.SetActive(true);
        }

        public void BackToMainMenu()
        {
            if (TutorialManager.instance.isTutorial)
                TutorialManager.instance.StopTutorial();

            GameManager.instance.playerPrefab = null;
            GameManager.instance.sceneManagement.RestartScene();
        }

        #region Pause System
        // ฟังก์ชันสำหรับหยุดเกม
        public void TogglePause()
        {
            if (lobbyBook.isBookOpen)
            {
                lobbyBook.CloseLobbyPanel();
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
            pauseMenuPanel.SetActive(true); // แสดง Pause Menu
            pauseNodeUI.SetActive(true);
            Time.timeScale = 0f;         // หยุดเวลา
            isGamePaused = true;
        }

        public void ResumeGame()
        {
            playerInputManager.isPauseGame = false;
            pauseMenuPanel.SetActive(false); // ซ่อน Pause Menu
            settingNodeUI.SetActive(false);
            keyBindsNodeUI.SetActive(false);
            menuGuideNodeUI.SetActive(false);
            Time.timeScale = 1f;          // กลับมาเล่นต่อ
            isGamePaused = false;
        }
        #endregion

    }
}

