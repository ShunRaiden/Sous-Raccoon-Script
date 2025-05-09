using SousRaccoon.Data;
using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UIUpgradeLevelPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject content;
        [SerializeField] private Animator anim;

        Coroutine panelCoroutine;

        [Header("Player Status")]
        [SerializeField] private TMP_Text moveSpeed;
        [SerializeField] private TMP_Text rollingCooldown;
        [SerializeField] private TMP_Text attackDamage;
        [SerializeField] private TMP_Text attackSpeed;
        [SerializeField] private TMP_Text healingRate;
        [SerializeField] private TMP_Text healingRange;

        [Header("Upgrade Status")]

        [Header("Icon")]
        //TODO Assign Icon of Stat

        [Header("Stat Point Prefab")]
        [SerializeField] private Image statPointPrefab;
        [SerializeField] private Sprite skillPointSprite;

        [SerializeField] private Transform movementStatPointLayout;
        [SerializeField] private Transform rollingStatPointLayout;
        [SerializeField] private Transform combatStatPointLayout;
        [SerializeField] private Transform healingStatPointLayout;
        [SerializeField] private Transform customerStatPointLayout;

        [SerializeField] private List<Image> movementStatPoint;
        [SerializeField] private List<Image> rollingStatPoint;
        [SerializeField] private List<Image> combatStatPoint;
        [SerializeField] private List<Image> healingStatPoint;
        [SerializeField] private List<Image> customerStatPoint;

        [Header("Text")]
        [SerializeField] private TMP_Text playerMoney;

        [SerializeField] private TMP_Text movementCostText;
        [SerializeField] private TMP_Text rollingCostText;
        [SerializeField] private TMP_Text combatCostText;
        [SerializeField] private TMP_Text healingCostText;
        [SerializeField] private TMP_Text customerCostText;

        [Header("Button")]
        [SerializeField] private Button movementCostButton;
        [SerializeField] private Button rollingCostButton;
        [SerializeField] private Button combatCostButton;
        [SerializeField] private Button healingCostButton;
        [SerializeField] private Button customerCostButton;

        [Header("MaxStat")]
        [SerializeField] private GameObject movementMaxStatText;
        [SerializeField] private GameObject rollingMaxStatText;
        [SerializeField] private GameObject combatMaxStatText;
        [SerializeField] private GameObject healingMaxStatText;
        [SerializeField] private GameObject customerMaxStatText;

        [Header("Cost")]
        private int movementCost;
        private int rollingCost;
        private int combatCost;
        private int healingCost;
        private int customerCost;

        PlayerSaveData levelData;
        PlayerDataBase dataBase;

        private void Awake()
        {
            movementCostButton.onClick.AddListener(OnMovementUpgrade);
            rollingCostButton.onClick.AddListener(OnRollingUpgrade);
            combatCostButton.onClick.AddListener(OnCombatUpgrade);
            healingCostButton.onClick.AddListener(OnHealingUpgrade);
            customerCostButton.onClick.AddListener(OnCustomerUpgrade);
        }

        private void OnDestroy()
        {
            movementCostButton.onClick.RemoveAllListeners();
            rollingCostButton.onClick.RemoveAllListeners();
            combatCostButton.onClick.RemoveAllListeners();
            healingCostButton.onClick.RemoveAllListeners();
            customerCostButton.onClick.RemoveAllListeners();
        }

        public void SetupUpLevelPanel()
        {
            StopCoroutine(OnClosePanel());

            if (levelData == null)
                levelData = GameManager.instance.playerSaveData;

            if (dataBase == null)
                dataBase = GameManager.instance.playerDataBase;

            // อัปเดตสถานะของผู้เล่น
            UpdatePlayerStatus();
            UpdateCurrency();

            // ตั้งค่าปุ่มและค่าอัปเกรด
            GetUpgradeCosts();
            UpdateUpgradeButtons();
            UpdateStatPoint();
            //TODO : SetUp Stat icon
            content.SetActive(true);
            anim.Play("Open");
        }

        public void ClosePanel()
        {
            if (panelCoroutine != null) return;
            panelCoroutine = StartCoroutine(OnClosePanel());
        }

        IEnumerator OnClosePanel()
        {
            anim.Play("Close");
            yield return new WaitForSeconds(0.66f);
            content.SetActive(false);

            panelCoroutine = null;
        }

        private void UpdatePlayerStatus()
        {
            moveSpeed.text = $"{dataBase.defaultPlayerSpeed.Speed + dataBase.playerSpeed[levelData.LevelSpeed].Speed} m/s";
            rollingCooldown.text = $"{dataBase.defaultPlayerRollCooldown.RollCooldown + dataBase.playerRollCooldown[levelData.LevelRollCooldown].RollCooldown} s";
            attackDamage.text = $"{dataBase.defaultPlayerCombat.Damage + dataBase.playerCombat[levelData.LevelCombat].Damage}";
            attackSpeed.text = $"{AttackSpeedPerSecond().ToString("F1")}/s";
            healingRate.text = $"{dataBase.defaultPlayerHealing.HealRate + dataBase.playerHealing[levelData.LevelHeal].HealRate}/s";
            healingRange.text = $"{dataBase.defaultPlayerHealing.HealRange + dataBase.playerHealing[levelData.LevelHeal].HealRange} m";
        }

        private void UpdateCurrency()
        {
            playerMoney.text = $"{GameManager.instance.PlayerMoney}";
        }

        private void GetUpgradeCosts()
        {
            if (levelData.LevelSpeed + 1 < dataBase.playerSpeed.Count)
                movementCost = dataBase.playerSpeed[levelData.LevelSpeed + 1].CostUpgrade;
            else
                movementCost = 0;

            if (levelData.LevelRollCooldown + 1 < dataBase.playerRollCooldown.Count)
                rollingCost = dataBase.playerRollCooldown[levelData.LevelRollCooldown + 1].CostUpgrade;
            else
                rollingCost = 0;

            if (levelData.LevelCombat + 1 < dataBase.playerCombat.Count)
                combatCost = dataBase.playerCombat[levelData.LevelCombat + 1].CostUpgrade;
            else
                combatCost = 0;

            if (levelData.LevelHeal + 1 < dataBase.playerHealing.Count)
                healingCost = dataBase.playerHealing[levelData.LevelHeal + 1].CostUpgrade;
            else
                healingCost = 0;

            if (levelData.LevelCustomer + 1 < dataBase.upgradeCustomers.Count)
                customerCost = dataBase.upgradeCustomers[levelData.LevelCustomer + 1].CostUpgrade;
            else
                customerCost = 0;
        }

        private void UpdateUpgradeButtons()
        {
            SetUpgradeButton(movementCostText, movementMaxStatText, movementCost, movementCostButton);
            SetUpgradeButton(rollingCostText, rollingMaxStatText, rollingCost, rollingCostButton);
            SetUpgradeButton(combatCostText, combatMaxStatText, combatCost, combatCostButton);
            SetUpgradeButton(healingCostText, healingMaxStatText, healingCost, healingCostButton);
            SetUpgradeButton(customerCostText, customerMaxStatText, customerCost, customerCostButton);
        }

        private void UpdateStatPoint()
        {
            SetStatPoint(dataBase.playerSpeed.Count, levelData.LevelSpeed, movementStatPointLayout, movementStatPoint);
            SetStatPoint(dataBase.playerRollCooldown.Count, levelData.LevelRollCooldown, rollingStatPointLayout, rollingStatPoint);
            SetStatPoint(dataBase.playerCombat.Count, levelData.LevelCombat, combatStatPointLayout, combatStatPoint);
            SetStatPoint(dataBase.playerHealing.Count, levelData.LevelHeal, healingStatPointLayout, healingStatPoint);
            SetStatPoint(dataBase.upgradeCustomers.Count, levelData.LevelCustomer, customerStatPointLayout, customerStatPoint);
        }

        private void SetUpgradeButton(TMP_Text costText, GameObject maxStat, int cost, Button button)
        {
            if (cost == 0)
            {
                costText.gameObject.SetActive(false);
                maxStat.SetActive(true);
                button.interactable = false;
            }
            else
            {
                costText.gameObject.SetActive(true);
                maxStat.SetActive(false);

                costText.text = cost.ToString();
                button.interactable = GameManager.instance.PlayerMoney >= cost;
            }
        }

        private void SetStatPoint(int maxLevel, int currentLevel, Transform layout, List<Image> listPoint)
        {
            if (listPoint.Count < maxLevel)
            {
                int trueIndex = maxLevel - listPoint.Count;
                for (int i = 0; i < trueIndex; i++)
                {
                    var statPointObj = Instantiate(statPointPrefab, layout.position, layout.rotation, layout);
                    listPoint.Add(statPointObj);
                }
            }

            listPoint[0].sprite = skillPointSprite;

            for (int i = 1; i <= currentLevel; i++)
            {
                listPoint[i].sprite = skillPointSprite;
            }
        }

        private float AttackSpeedPerSecond()
        {
            var attackTimePerHit = 0.65f / (1 + dataBase.defaultPlayerCombat.AttackSpeed + dataBase.playerCombat[levelData.LevelCombat].AttackSpeed);
            var attackPerSecond = 1 / attackTimePerHit;

            return attackPerSecond;
        }

        #region OnUpgrade
        private void OnMovementUpgrade()
        {
            if (GameManager.instance.PlayerMoney >= movementCost)
            {
                GameManager.instance.RemoveMoney(movementCost);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                levelData.LevelSpeed++;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);

                // ตรวจสอบถ้าเลเวลถึงสูงสุดแล้ว
                if (levelData.LevelSpeed + 1 >= dataBase.playerSpeed.Count)
                {
                    movementCostButton.interactable = false;
                    movementCostText.gameObject.SetActive(false);
                    movementMaxStatText.SetActive(true);
                }
                else
                {
                    movementCost = dataBase.playerSpeed[levelData.LevelSpeed + 1].CostUpgrade;
                    SetUpgradeButton(movementCostText, movementMaxStatText, movementCost, movementCostButton);
                }

                SetupUpLevelPanel();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
            }
        }

        // ส่วนอัปเกรดที่เหลือก็จะคล้าย ๆ กับ OnMovementUpgrade แต่สำหรับปุ่มอื่น ๆ 
        private void OnRollingUpgrade()
        {
            if (GameManager.instance.PlayerMoney >= rollingCost)
            {
                GameManager.instance.RemoveMoney(rollingCost);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                levelData.LevelRollCooldown++;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);

                if (levelData.LevelRollCooldown + 1 >= dataBase.playerRollCooldown.Count)
                {
                    rollingCostButton.interactable = false;
                    rollingCostText.text = "Level Max";
                }
                else
                {
                    rollingCost = dataBase.playerRollCooldown[levelData.LevelRollCooldown + 1].CostUpgrade;
                    SetUpgradeButton(rollingCostText, rollingMaxStatText, rollingCost, rollingCostButton);
                }

                SetupUpLevelPanel();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
            }
        }

        private void OnCombatUpgrade()
        {
            if (GameManager.instance.PlayerMoney >= combatCost)
            {
                GameManager.instance.RemoveMoney(combatCost);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                levelData.LevelCombat++;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);

                if (levelData.LevelCombat + 1 >= dataBase.playerCombat.Count)
                {
                    combatCostButton.interactable = false;
                    combatCostText.text = "Level Max";
                }
                else
                {
                    combatCost = dataBase.playerCombat[levelData.LevelCombat + 1].CostUpgrade;
                    SetUpgradeButton(combatCostText, combatMaxStatText, combatCost, combatCostButton);
                }

                SetupUpLevelPanel();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
            }
        }

        private void OnHealingUpgrade()
        {
            if (GameManager.instance.PlayerMoney >= healingCost)
            {
                GameManager.instance.RemoveMoney(healingCost);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                levelData.LevelHeal++;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);

                if (levelData.LevelHeal + 1 >= dataBase.playerHealing.Count)
                {
                    healingCostButton.interactable = false;
                    healingCostText.text = "Level Max";
                }
                else
                {
                    healingCost = dataBase.playerHealing[levelData.LevelHeal + 1].CostUpgrade;
                    SetUpgradeButton(healingCostText, healingMaxStatText, healingCost, healingCostButton);
                }

                SetupUpLevelPanel();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
            }
        }

        private void OnCustomerUpgrade()
        {
            if (GameManager.instance.PlayerMoney >= customerCost)
            {
                GameManager.instance.RemoveMoney(customerCost);
                levelData.MoneyCurrency = GameManager.instance.PlayerMoney;
                levelData.LevelCustomer++;

                SaveManager.SaveData(GameManager.instance.currentSaveSlot, levelData);

                if (levelData.LevelCustomer + 1 >= dataBase.upgradeCustomers.Count)
                {
                    customerCostButton.interactable = false;
                    customerCostText.text = "Level Max";
                }
                else
                {
                    customerCost = dataBase.upgradeCustomers[levelData.LevelCustomer + 1].CostUpgrade;
                    SetUpgradeButton(customerCostText, customerMaxStatText, customerCost, customerCostButton);
                }

                SetupUpLevelPanel();

                OnUpgradeSound();

                LobbyManager.instance.DestroyPlayer();
                LobbyManager.instance.SpawnPlayer();
            }
        }

        private void OnUpgradeSound()
        {
            AudioManager.instance.PlayOneShotSFX("UpLevel_UI");
        }
        #endregion
    }
}

