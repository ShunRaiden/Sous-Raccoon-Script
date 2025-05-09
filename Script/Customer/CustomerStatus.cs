using SousRaccoon.Data;
using SousRaccoon.Manager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace SousRaccoon.Customer
{
    public class CustomerStatus : MonoBehaviour
    {
        [HideInInspector] public CustomerMovement movement;
        [HideInInspector] public bool canGiveOrder = false;

        [Header("Time")]
        // TODO : maxHealthTime เดี๋ยวจะไปดึงมาจาก Player Upgrade
        // Health Time
        public float maxHealthTime;
        public float currentHealthTime;
        public float countDownPerSecond = 1f; // damage dealt per second
        public float countDownPlusWaiting = 0.25f; // damage dealt per second

        [Header("Perk")]
        public int blockTimes = 0;

        public int maxMoneyDropRunStage;
        public int midMoneyDropRunStage;
        public int minMoneyDropRunStage;

        [SerializeField] int maxMoneyDrop;
        [SerializeField] int midMoneyDrop;
        [SerializeField] int minMoneyDrop;

        [Header("Take Damage")]
        [SerializeField] protected VisualEffect takeDamageVFX;
        [SerializeField] protected SkinnedMeshRenderer baseMeshRenderer;
        [SerializeField] protected Material baseMat;
        [SerializeField] protected Material takeDamageMat;
        [SerializeField] protected float matDuration;
        Coroutine takeDamageCoroutine;

        [Header("Heal")]
        public Animator healAnim;

        [Header("Condition")]
        public bool isCountDownOverTime = false; // flag to check if health is decreasing over time
        public bool isDead = false;
        public bool isUpset = false;
        public bool canHeal = true;
        public bool isTutorial = false;
        public bool isWaiting = false;

        GameObject currentPanel;

        public int htState;

        [SerializeField] Sprite greenBar;
        [SerializeField] Sprite yellowBar;
        [SerializeField] Sprite redBar;
        [SerializeField] Sprite purpleBar;

        [SerializeField] GameObject htBarPanel;
        [SerializeField] Image htBar; // health bar image

        [Header("Order")]
        [SerializeField] GameObject orderPanel; // Panel
        [SerializeField] Image orderOrderIcon; // Order Icon in order State
        [SerializeField] GameObject menuIcon;
        [SerializeField] GameObject waterMenuIcon;

        [Header("Wait")]
        [SerializeField] GameObject waitPanel; // Panel
        [SerializeField] Image waitOrderIcon; // Order Icon in wait State

        [Header("Upset")]
        [SerializeField] GameObject AngryVFX;

        [Header("Dead")]
        public GameObject deadPref;

        [Header("Debug")]
        //TODO : Delete it. This is for Debug
        public TMP_Text htBarText; // health bar text

        [SerializeField] public UIState currentState;

        public enum UIState
        {
            normal,
            thinking,
            order,
            wait,
            dead,
            getup,
            upset,
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadPlayerStatus();

            movement = GetComponent<CustomerMovement>();
            currentPanel = null;
            canHeal = true;
            UpdateHealthBar(); // Initialize health bar on start
        }

        // Update is called once per frame
        void Update()
        {
            if (isDead || isUpset) return;

            if (isCountDownOverTime && !isTutorial)
            {
                HealthTimeCount();
                UpdateHealthBar();
            }
        }

        #region Health Time

        // Function to stop health reduction over time
        public void StopHealthTimeCount()
        {
            isCountDownOverTime = false;
        }

        // Function to start health reduction over time
        public void StartHealthTimeCount()
        {
            isCountDownOverTime = true;
        }

        // Function to handle health reduction over time
        private void HealthTimeCount()
        {
            if (currentHealthTime > 0)
            {
                currentHealthTime -= countDownPerSecond * Time.deltaTime; // Decrease health over time

                if (isWaiting)
                    currentHealthTime -= countDownPlusWaiting * Time.deltaTime;

                if (currentHealthTime <= 0)
                {
                    currentHealthTime = 0;

                    if (!isDead)
                    {
                        if (isUpset) return;

                        isUpset = true;

                        StageManager.instance.UpdateLoseRate(1);

                        EnterUIState(UIState.upset);
                        movement.StartUpset();
                    }
                }
            }
        }

        // Function to apply immediate damage to the health (e.g., from attacks or hazards)
        public void TakeDamageTimeCount(float damage)
        {
            if (currentState == UIState.getup || isDead || isUpset || isTutorial) return;

            if (blockTimes > 0)
            {
                blockTimes--;
                movement.PlayHurtAnimation();
                //TODO : update block bar
                //TODO : Imprement block effect
                return;
            }

            currentHealthTime -= damage; // Reduce health instantly by a certain amount
            OnTakeDamageVFX();

            if (currentHealthTime <= 0)
            {
                currentHealthTime = 0;

                if (!isUpset)
                {
                    if (isDead) return;

                    isDead = true;

                    StageManager.instance.UpdateLoseRate(1);

                    EnterUIState(UIState.dead);

                    if (isDead)
                    {
                        canHeal = false;
                        movement.StartDead();
                    }
                }
            }
            else
            {
                movement.PlayHurtAnimation(); //Play Animation Hurt
            }

            UpdateHealthBar();
        }

        public void OnTakeDamageSFX()
        {
            AudioManager.instance.PlayStageSFXOneShot("Customer_Hurt");
        }

        public void OnTakeDamageVFX()
        {
            if (takeDamageCoroutine != null)
            {
                StopCoroutine(takeDamageCoroutine);
            }

            // เริ่ม Coroutine ใหม่และเก็บ reference ไว้
            if (takeDamageMat != null)
                takeDamageCoroutine = StartCoroutine(TakeDamageEffect());

            takeDamageVFX.gameObject.SetActive(true);
            takeDamageVFX.Stop();
            takeDamageVFX.Play();
        }

        IEnumerator TakeDamageEffect()
        {
            baseMeshRenderer.material = takeDamageMat;
            yield return new WaitForSeconds(matDuration);
            baseMeshRenderer.material = baseMat;
        }

        // Function to heal or increase health
        public void Heal(float amount)
        {
            if (!canHeal) return;

            healAnim.Play("HealVFX");
            currentHealthTime += amount;

            if (currentHealthTime > maxHealthTime)
            {
                currentHealthTime = maxHealthTime; // Cap health at the maximum value
            }

            UpdateHealthBar();
        }

        // Function to update the health bar UI
        public void UpdateHealthBar()
        {
            float healthPercentage = (currentHealthTime / maxHealthTime) * 100;

            if (!isWaiting)
            {
                // คำนวณเงินที่จะได้รับตามระดับของเลือดที่เหลือ
                if (healthPercentage > 75f)
                {
                    htBar.sprite = greenBar;
                    htState = 0;
                }
                else if (healthPercentage > 25f)
                {
                    htBar.sprite = yellowBar;
                    htState = 1;
                }
                else
                {
                    htBar.sprite = redBar;
                    htState = 2;
                }
            }
            else
            {
                htBar.sprite = purpleBar;
                htState = 3;
            }

            if (htBar != null)
            {
                htBar.fillAmount = currentHealthTime / maxHealthTime; // Update the fill amount of the health bar
            }

            //TODO : Delete it. This is for Debug
            if (htBarText != null)
            {
                htBarText.text = Mathf.Ceil(currentHealthTime).ToString() + " / " + maxHealthTime.ToString(); // Update the text
            }
        }
        #endregion

        // Function to Change UI State
        public void EnterUIState(UIState state)
        {
            currentState = state;

            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
            }

            switch (currentState)
            {
                case UIState.normal:
                    htBarPanel.SetActive(true);
                    break;
                case UIState.thinking:
                    break;
                case UIState.order:
                    orderPanel.SetActive(true);
                    currentPanel = orderPanel;
                    StartHealthTimeCount();//Customer Count Down
                    break;
                case UIState.wait:
                    waitPanel.SetActive(true);
                    currentPanel = waitPanel;
                    break;
                case UIState.dead:
                    htBarPanel.SetActive(false);
                    break;
                case UIState.getup:
                    canHeal = false;
                    htBarPanel.SetActive(false);
                    break;
                case UIState.upset:
                    AngryVFX.SetActive(true);
                    htBarPanel.SetActive(false);
                    break;
            }
        }

        public void SetIcon(Sprite icon, bool isWaterOrder)
        {
            orderOrderIcon.sprite = icon;
            waitOrderIcon.sprite = icon;

            if (isWaterOrder)
            {
                menuIcon.SetActive(true);
            }
            else
            {
                waterMenuIcon.SetActive(true);
            }
        }

        private void LoadPlayerStatus()
        {
            PlayerSaveData levelData = GameManager.instance.playerSaveData; //Load Level

            PlayerDataBase loadedData = GameManager.instance.playerDataBase; //Load Data of Level

            maxHealthTime = loadedData.defaultCustomersStat.MaxHealthTime + loadedData.upgradeCustomers[levelData.LevelCustomer].MaxHealthTime;
            currentHealthTime = maxHealthTime;
        }

        public int CalculateReward()
        {
            float healthPercentage = (currentHealthTime / maxHealthTime) * 100;

            // คำนวณเงินที่จะได้รับตามระดับของเลือดที่เหลือ
            if (healthPercentage > 75f)
            {
                // ให้เต็มจำนวน
                return maxMoneyDrop + maxMoneyDropRunStage;
            }
            else if (healthPercentage > 25f)
            {
                // ให้ระดับ 2 (อาจกำหนดเป็น 50% ของ fullReward)
                return midMoneyDrop + midMoneyDropRunStage;
            }
            else
            {
                // ให้ระดับ 3 (อาจกำหนดเป็น 25% ของ fullReward)
                return minMoneyDrop + minMoneyDropRunStage;
            }
        }

        public void SetMoneyDropRunStage(int max, int mid, int min)
        {
            maxMoneyDropRunStage += max;
            midMoneyDropRunStage += mid;
            minMoneyDropRunStage += min;
        }
    }
}
