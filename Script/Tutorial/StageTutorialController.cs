using SousRaccoon.CameraMove;
using SousRaccoon.Customer;
using SousRaccoon.Kitchen;
using SousRaccoon.Monster;
using SousRaccoon.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace SousRaccoon.Manager
{
    public class StageTutorialController : MonoBehaviour
    {
        public CustomerStatus customer;
        public CustomerStatus currentCustomer;

        public MonsterBasicStatus monster;
        public MonsterBasicStatus currentMonster;

        public MonsterWeakerAI weaker;
        public MonsterWeakerAI currentWeaker;

        public CustomerDead customerDead;
        public CustomerDead currentCustomerDead;

        public Transform spawnPoint;
        public Transform TrashSpawnPoint;

        public UIStageQuest stageQuestUI;
        public UIStageQuest currentQuest;
        public Transform questSpawnPoint;

        public ChefAI chefAI;

        public GameObject kitchenTable;
        public GameObject gKitchenTable;

        public GameObject igdFirst;
        public GameObject gIGDFirst;
        public GameObject igdSecond;
        public GameObject gIGDSecond;

        public GameObject bigTrash;

        public GameObject guidePanel;
        public TMP_Text guideText;
        public LocalizeStringEvent guideLocalizeEvent;

        [Header("----- Camera -----")]
        public CameraMovement cameraMove;

        // Start is called before the first frame update
        void Start()
        {
            TutorialManager.instance.stageTutorialController = this;

            StageManager.instance.isTutorial = true;
            StageManager.instance.chefGuide.SetActive(false);
        }

        public void SpawnMonster()
        {
            currentMonster = Instantiate(monster, spawnPoint.position, spawnPoint.rotation);
            currentMonster.SetStatus(3, 0, 0, 0);
        }

        public void SpawnCustomer(bool isOverThink)
        {
            currentCustomer = Instantiate(customer, spawnPoint.position, spawnPoint.rotation);
            currentCustomer.isTutorial = true;

            currentCustomer.movement = currentCustomer.GetComponent<CustomerMovement>();
            currentCustomer.movement.isOverThinking = isOverThink;
        }

        public void SpawnCustomerDead()
        {
            currentCustomerDead = Instantiate(customerDead, TrashSpawnPoint.position, TrashSpawnPoint.rotation);
        }

        public void StopTutorial()
        {
            TutorialManager.instance.StopTutorial();
        }

        public void QuestSetUp(string headerQuest)
        {
            currentQuest = Instantiate(stageQuestUI, questSpawnPoint);
            currentQuest.SetUp(headerQuest, "");
        }

        public void QuestSetUpLocalized(LocalizedString headerLocalized)
        {
            currentQuest = Instantiate(stageQuestUI, questSpawnPoint);
            currentQuest.SetUpLocalized(headerLocalized);
        }

        public void FinishQuest()
        {
            currentQuest.FinishQuest();
        }

        public IEnumerator OnWeakerSpawn()
        {
            currentWeaker = Instantiate(weaker, TrashSpawnPoint.position, TrashSpawnPoint.rotation);

            yield return new WaitUntil(() => currentWeaker == null);

            LoopWeaker();
        }

        public void LoopWeaker()
        {
            if (RunStageManager.instance.PlayerCoin <= 0)
            {
                StartCoroutine(OnWeakerSpawn());
            }
        }

        public void CameraTarget(Transform target)
        {
            cameraMove.LookAtTarget(target);
        }

        public void ResetCamTarget()
        {
            cameraMove.LookAtPlayer();
        }

        public void SetGuideText(string text = "")
        {
            if (text == "")
                guidePanel.SetActive(false);
            else
                guidePanel.SetActive(true);

            guideText.text = text;
        }

        public void SetLocalizedGuideText(LocalizedString localizedString = null)
        {
            if (localizedString == null)
                guidePanel.SetActive(false);
            else
                guidePanel.SetActive(true);

            guideLocalizeEvent.StringReference = localizedString;
        }
    }
}
