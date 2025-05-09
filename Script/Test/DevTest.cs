using SousRaccoon.Data;
using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using SousRaccoon.Monster;
using SousRaccoon.UI;
using System.Collections;
using UnityEngine;

public class DevTest : MonoBehaviour
{
    public UIStageScenarioPanel uIStageScenarioPanel;
    public ShopMerchantManager shopMerchantManager;

    public ShopMerchantItemDataBase helperItemDataBase;
    public ShopMerchantItemDataBase barricadeItemDataBase;
    public ShopMerchantItemDataBase raccoonSpeedItemDataBase;
    public ShopMerchantItemDataBase raccoonASPDItemDataBase;
    public ShopMerchantItemDataBase raccoonHealRateItemDataBase;
    public ShopMerchantItemDataBase raccoonHealRangeItemDataBase;

    public ShopMerchantItemDataBase targetItemDataBase;

    public ItemListInState itemList;
    public NPCSpawnSet allSetSpawn;

    public GameObject nextLevelButton;
    public GameObject nextStageButton;
    public GameObject backToLobbyButton;

    public GameObject overlay;

    [Space(3)]
    [Header("Spawn")]
    public GameObject MonsterWantToSpawn;
    public Transform SpawnPos;

    public int hp;
    public float dmCustomer;
    public int dmPlayer;
    public float healDeadMutiply;

    public bool toggle;
    public bool canToggle = true;

    public bool upHealper;

    public Sprite someIcon;

    public bool isMidterm;

    private void Start()
    {
        StartCoroutine(BuySomthing());
        /*
        shopMerchantManager.OnBuyingPerk(helperItemDataBase);
        shopMerchantManager.OnBuyingPerk(helperItemDataBase);
        shopMerchantManager.OnBuyingPerk(barricadeItemDataBase);

        if (targetItemDataBase != null)
            shopMerchantManager.OnBuyingPerk(targetItemDataBase);

        */
    }

    public void SpeedGameX(float xIndex)
    {
        Time.timeScale = xIndex;
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightControl))
        {
            canToggle = true;
        }

        if (Input.GetKeyDown(KeyCode.RightControl) && canToggle)
        {
            canToggle = false;
            toggle = !toggle;
            if (toggle)
            {
                overlay.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                overlay.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public IEnumerator BuySomthing()
    {
        yield return new WaitForSeconds(0.2f);

        if (upHealper)
        {
            RunStageManager.instance.AddCoin(300);
            shopMerchantManager.OnBuyingPerk(helperItemDataBase);
            shopMerchantManager.OnBuyingPerk(barricadeItemDataBase);

            if (targetItemDataBase != null)
            {
                shopMerchantManager.OnBuyingPerk(targetItemDataBase);
            }
        }

        if (isMidterm)
        {
            RunStageManager.instance.daysCount = 5;
            RunStageManager.instance.AddCoin(300);
            StageManager.instance.spawnerManager.npcSpawnSet = allSetSpawn;
            StageManager.instance.itemList = itemList;
            shopMerchantManager.OnBuyingPerk(helperItemDataBase);
            shopMerchantManager.OnBuyingPerk(barricadeItemDataBase);
            shopMerchantManager.OnBuyingPerk(raccoonSpeedItemDataBase);
            shopMerchantManager.OnBuyingPerk(raccoonASPDItemDataBase);
            shopMerchantManager.OnBuyingPerk(raccoonHealRangeItemDataBase);
            shopMerchantManager.OnBuyingPerk(raccoonHealRateItemDataBase);
        }
    }

    public void EndDays()
    {
        StageManager.instance.EndDay();
    }

    public void EndLevel()
    {
        RunStageManager.instance.daysCount = 6;
        StageManager.instance.EndDay();
    }

    public void SpawnMonster()
    {
        var monst = Instantiate(MonsterWantToSpawn, SpawnPos.position, SpawnPos.rotation);

        var monStat = monst.GetComponent<MonsterStatus>();
        monStat.SetStatus(hp, dmCustomer, dmPlayer, healDeadMutiply);
    }

    public void SpawnMosnterSpawner()
    {
        StageManager.instance.spawnerManager.monsterSpawner.hasSpawnToday = true;
        StageManager.instance.spawnerManager.monsterSpawner.StartSpawnMinion();
    }

    public void SpawnTargetMonster(GameObject target)
    {
        var monst = Instantiate(target, SpawnPos.position, SpawnPos.rotation);

        var monStat = monst.GetComponent<MonsterStatus>();
        monStat.SetStatus(hp, dmCustomer, dmPlayer, healDeadMutiply);
    }

    public void SpawnMerchant()
    {
        shopMerchantManager.SpawnMerchant();
    }

    public void AddCoin(int coinIndex)
    {
        RunStageManager.instance.AddCoin(coinIndex);
        Debug.LogError(RunStageManager.instance.PlayerCoin);
    }

    public void TestLoseCutScene()
    {
        StageManager.instance.HandleGameLose();
    }

    public void TestWinCutScene()
    {
        RunStageManager.instance.daysCount = 6;
        StageManager.instance.EndDay();
    }

    public void SpawnCustomerBySpawner()
    {
        StageManager.instance.spawnerManager.StartSpawnCustomer();
    }

    public void StartDialogue(DialogDataSO dialogData)
    {
        DialogManager.instance.StartDialogue(dialogData);
    }

    public void MockScenarioUI(ScenarioDataBase scenarioData)
    {
        uIStageScenarioPanel.SetupPanel(scenarioData.icon, scenarioData.nameScenarioLocalized, scenarioData.discriptionLocalized, scenarioData.statIcon, scenarioData.statValue, scenarioData.headIcon);
        uIStageScenarioPanel.content.SetActive(true);
    }

    bool twoH = true;

    public void ToggleTwoHand()
    {
        twoH = !twoH;
        StageManager.instance.playerKitchenAction.useTwoHand = twoH;
        StageManager.instance.playerKitchenAction.isUsingFirstHand = true;
    }
}