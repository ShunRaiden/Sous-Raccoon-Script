using SousRaccoon.Data;
using SousRaccoon.Kitchen;
using SousRaccoon.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SousRaccoon.Manager
{
    public class TutorialManager : MonoBehaviour
    {
        #region Singleton
        public static TutorialManager instance { get { return _instance; } }
        private static TutorialManager _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion

        [Header("------ Dialogue Data ------")]
        public List<DialogDataSO> dialogDataList;
        [TextArea(3, 10)]
        public List<string> guideDialogueList;
        public List<LocalizedString> guideDialogueLists = new List<LocalizedString>();

        [Header("------ Quest Data ------")]
        public List<string> questTextList;
        public List<LocalizedString> questTextLists = new List<LocalizedString>();

        [Header("------ Player Input ------")]
        public PlayerInputManager playerInputManager;
        public PlayerKitchenAction playerKitchenAction;

        public StageTutorialController stageTutorialController;
        public KitchenTable kitchenTable;

        public TutorialStep currentStep = TutorialStep.Intro; // เริ่มต้นที่แนะนำตัว
        private bool startDialogue = false;
        public bool hasNewGame = false;
        public bool isTutorial = false;

        public bool isJumpCheck = false;
        public bool jumpCheck = false;

        public bool isRollCheck = false;
        public bool rollCheck = false;

        public bool canStart = false;

        private Coroutine _currentCoroutine;

        public enum TutorialStep
        {
            Intro, Movement, Jump, Roll,
            OpenShop, GetOrder, GiveOrder, GetIngredient,
            GetFood, Heal, AttackEnemy, Cleaning, DroppingTrash, HitCoin, ReturnLobby, UpgradeStat,
        }

        public void StartTutorial()
        {
            if (_currentCoroutine == null) // ตรวจสอบว่าไม่มี Coroutine ที่กำลังรันอยู่
            {
                hasNewGame = false;
                isTutorial = true;
                _currentCoroutine = StartCoroutine(StartTutorialSequence());
            }
        }

        public void StopTutorial()
        {
            hasNewGame = false;
            isTutorial = false;

            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            StopAllCoroutines(); // เผื่อมี Coroutine อื่นที่รันอยู่
            DialogManager.instance.StopAllDialogue();
        }


        private IEnumerator StartTutorialSequence()
        {
            playerInputManager = FindAnyObjectByType<PlayerInputManager>();
            //Start
            yield return StartDialogue(TutorialStep.Intro);
            GameManager.instance.sceneManagement.StartSceneTutorial();
            //Load Scene
            yield return new WaitUntil(() => SceneIsLoaded("Tutorial")); // รอจนกว่า Scene จะโหลดเสร็จ
            playerInputManager = FindAnyObjectByType<PlayerInputManager>();

            //Movement
            yield return StartDialogue(TutorialStep.Movement);
            stageTutorialController.QuestSetUpLocalized(questTextLists[0]);
            yield return WaitForPlayerAction(() => playerInputManager.moveAmount != 0);
            stageTutorialController.FinishQuest();
            yield return new WaitForSeconds(2f);

            //Jump
            yield return StartDialogue(TutorialStep.Jump);
            stageTutorialController.QuestSetUpLocalized(questTextLists[1]);
            isJumpCheck = true;
            yield return WaitForPlayerAction(() => jumpCheck);
            stageTutorialController.FinishQuest();
            yield return new WaitForSeconds(2f);

            //Rolling
            yield return StartDialogue(TutorialStep.Roll);
            stageTutorialController.QuestSetUpLocalized(questTextLists[2]);
            isRollCheck = true;
            yield return WaitForPlayerAction(() => rollCheck);
            stageTutorialController.FinishQuest();
            yield return new WaitForSeconds(2f);

            //Start Game
            stageTutorialController.CameraTarget(stageTutorialController.chefAI.transform);
            yield return StartDialogue(TutorialStep.OpenShop);
            stageTutorialController.ResetCamTarget();
            stageTutorialController.QuestSetUpLocalized(questTextLists[3]);
            canStart = true;
            StageManager.instance.chefGuide.SetActive(true);
            yield return WaitForPlayerAction(() => StageManager.instance.isGameStart);
            stageTutorialController.FinishQuest();

            //Get Order
            stageTutorialController.SpawnCustomer(false);
            yield return WaitForPlayerAction(() => stageTutorialController.currentCustomer != null);
            yield return new WaitForSeconds(0.1f);
            stageTutorialController.CameraTarget(stageTutorialController.currentCustomer.transform);
            playerKitchenAction = FindAnyObjectByType<PlayerKitchenAction>();
            yield return StartDialogue(TutorialStep.GetOrder);
            stageTutorialController.QuestSetUpLocalized(questTextLists[4]);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem != null && playerKitchenAction.CurrentItem.itemType == Data.Item.ItemDataBase.ItemType.Order);
            stageTutorialController.FinishQuest();

            //Give Order
            stageTutorialController.kitchenTable.SetActive(true);
            stageTutorialController.CameraTarget(stageTutorialController.kitchenTable.transform);
            yield return StartDialogue(TutorialStep.GiveOrder);
            stageTutorialController.ResetCamTarget();
            stageTutorialController.QuestSetUpLocalized(questTextLists[5]);
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem == null);
            stageTutorialController.gKitchenTable.SetActive(false);
            stageTutorialController.FinishQuest();

            //Get IGD
            stageTutorialController.igdFirst.SetActive(true);
            stageTutorialController.CameraTarget(stageTutorialController.igdFirst.transform);
            yield return StartDialogue(TutorialStep.GetIngredient);
            stageTutorialController.QuestSetUpLocalized(questTextLists[6]);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem != null && playerKitchenAction.CurrentItem.itemType == Data.Item.ItemDataBase.ItemType.Ingredient);
            stageTutorialController.gIGDFirst.SetActive(false);
            stageTutorialController.FinishQuest();

            //Give IGD
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            StageManager.instance.chefGuide.SetActive(true);
            stageTutorialController.SetLocalizedGuideText(guideDialogueLists[0]);
            stageTutorialController.QuestSetUpLocalized(questTextLists[7]);

            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem == null);
            StageManager.instance.chefGuide.SetActive(false);
            stageTutorialController.gIGDSecond.SetActive(true);
            stageTutorialController.igdSecond.SetActive(true);
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem != null && playerKitchenAction.CurrentItem.itemType == Data.Item.ItemDataBase.ItemType.Ingredient);
            stageTutorialController.gIGDSecond.SetActive(false);
            yield return WaitForPlayerAction(() => kitchenTable.foodTableList[0].foodIndex > 0);
            stageTutorialController.FinishQuest();
            stageTutorialController.SetLocalizedGuideText();

            //Get Food
            yield return StartDialogue(TutorialStep.GetFood);
            stageTutorialController.QuestSetUpLocalized(questTextLists[8]);
            //stageTutorialController.SetGuideText(guideDialogueList[1]);
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem != null && playerKitchenAction.CurrentItem.itemType == Data.Item.ItemDataBase.ItemType.Food);
            stageTutorialController.FinishQuest();
            //stageTutorialController.SetGuideText();

            //Heal
            playerInputManager.canInteract = false;
            stageTutorialController.currentCustomer.currentHealthTime = 10;
            stageTutorialController.currentCustomer.maxHealthTime = 30;
            stageTutorialController.currentCustomer.UpdateHealthBar();
            stageTutorialController.SetLocalizedGuideText(guideDialogueLists[2]);
            stageTutorialController.CameraTarget(stageTutorialController.currentCustomer.transform);
            yield return StartDialogue(TutorialStep.Heal);
            stageTutorialController.QuestSetUpLocalized(questTextLists[9]);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => stageTutorialController.currentCustomer.currentHealthTime >= 30);
            stageTutorialController.FinishQuest();
            stageTutorialController.SetLocalizedGuideText();

            //Give Food
            playerInputManager.canInteract = true;
            stageTutorialController.QuestSetUpLocalized(questTextLists[10]);
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem == null);
            stageTutorialController.FinishQuest();

            //Attack Enemy
            stageTutorialController.QuestSetUpLocalized(questTextLists[11]);
            stageTutorialController.SpawnCustomer(true);
            stageTutorialController.SpawnMonster();
            stageTutorialController.CameraTarget(stageTutorialController.currentMonster.transform);
            yield return StartDialogue(TutorialStep.AttackEnemy);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => stageTutorialController.currentMonster == null);
            stageTutorialController.FinishQuest();
            Destroy(stageTutorialController.currentCustomer.gameObject);
            stageTutorialController.currentCustomer = null;

            //Clean
            StageManager.instance.UpdateLoseRate(1);
            stageTutorialController.SpawnCustomerDead();
            stageTutorialController.SetLocalizedGuideText(guideDialogueLists[3]);
            stageTutorialController.CameraTarget(stageTutorialController.currentCustomerDead.transform);
            yield return StartDialogue(TutorialStep.Cleaning);
            stageTutorialController.ResetCamTarget();
            stageTutorialController.QuestSetUpLocalized(questTextLists[12]);
            yield return WaitForPlayerAction(() => stageTutorialController.currentCustomerDead == null);
            stageTutorialController.FinishQuest();
            stageTutorialController.SetLocalizedGuideText();

            //Get Trash

            stageTutorialController.QuestSetUpLocalized(questTextLists[13]);
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem != null && playerKitchenAction.CurrentItem.itemType == Data.Item.ItemDataBase.ItemType.Trash);
            stageTutorialController.FinishQuest();

            //Drop Trash
            stageTutorialController.CameraTarget(stageTutorialController.bigTrash.transform);
            yield return StartDialogue(TutorialStep.DroppingTrash);
            stageTutorialController.SetLocalizedGuideText(guideDialogueLists[4]);
            stageTutorialController.QuestSetUpLocalized(questTextLists[14]);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => playerKitchenAction.CurrentItem == null);
            stageTutorialController.FinishQuest();
            stageTutorialController.SetLocalizedGuideText();

            //Attack Coin
            stageTutorialController.LoopWeaker();
            stageTutorialController.CameraTarget(stageTutorialController.currentWeaker.transform);
            yield return StartDialogue(TutorialStep.HitCoin);
            stageTutorialController.SetLocalizedGuideText(guideDialogueLists[5]);
            stageTutorialController.QuestSetUpLocalized(questTextLists[15]);
            stageTutorialController.ResetCamTarget();
            yield return WaitForPlayerAction(() => RunStageManager.instance.PlayerCoin > 0);
            stageTutorialController.FinishQuest();
            stageTutorialController.SetLocalizedGuideText();

            //Return Lobby
            yield return StartDialogue(TutorialStep.ReturnLobby);
            GameManager.instance.sceneManagement.ReturnToLobby();
            GameManager.instance.AddMoney(50);

            yield return new WaitUntil(() => SceneIsLoaded("Lobby"));
            yield return new WaitForSeconds(1f);
            yield return StartDialogue(TutorialStep.UpgradeStat);

            Debug.Log("Tutorial Complete!");
            isTutorial = false;
            _currentCoroutine = null;
        }

        private IEnumerator StartDialogue(TutorialStep step)
        {
            startDialogue = true;

            DialogManager.instance.StartDialogue(dialogDataList[(int)step]);

            if (playerInputManager != null)
            {
                playerInputManager.isCutScene = true;
            }

            yield return new WaitUntil(() => !DialogManager.instance._isDialogueStart);

            if (playerInputManager != null)
            {
                playerInputManager.isCutScene = false;
            }

            startDialogue = false;
        }

        private IEnumerator WaitForPlayerAction(Func<bool> condition)
        {
            yield return new WaitUntil(condition);
        }

        private bool SceneIsLoaded(string sceneName)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName;
        }
    }
}