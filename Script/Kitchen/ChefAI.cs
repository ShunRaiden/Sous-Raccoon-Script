using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using SousRaccoon.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class ChefAI : MonoBehaviour
    {
        // Get By State
        public PlayerKitchenAction player;
        bool isGameWin = false;

        NavMeshAgent agent;
        KitchenTable kitchenTable;
        [SerializeField] Animator animator;

        [Header("Move")]
        [SerializeField] string currentStation = "";
        [SerializeField] float walkSpeed;
        [SerializeField] float insaneWalkSpeed;
        [SerializeField] float rotationSpeed;

        [SerializeField] private FoodSO food;
        [SerializeField] private List<Transform> stationList = new(); // List ของสถานีทั้งหมด
        [SerializeField] private List<Transform> stationDirList = new(); // List Direction ของสถานีทั้งหมด
        private Dictionary<string, Transform> stationDict = new(); // Dictionary สำหรับแมปชื่อสถานีเข้ากับ Transform
        private Dictionary<string, Transform> stationDirDict = new(); // Dictionary สำหรับแมปชื่อสถานีเข้ากับ Dir ของ เชฟ

        private CookingStep currentStep; // ขั้นตอนการทำอาหารที่กำลังทำอยู่
        [SerializeField] private int currentStepIndex = 0; // ตัวแปรเก็บตำแหน่งของขั้นตอนปัจจุบันใน CookingSteps
        private Dictionary<IngredientSO, int> receivedIngredients = new(); // เก็บจำนวนวัตถุดิบที่ได้รับ

        public IngredientSO igdNeedType;
        public int igdNeedAmount;

        [SerializeField] private int multiplyOrder;
        public bool hasGetIngredients { get; private set; } //เช็คว่าเปิดรับวัตถุดิบได้ไหม

        [Header("Insane")]
        public bool isInsane;
        public float chefInsaneTime;
        private float speedInsaneMultiply = 0.5f;
        [SerializeField] GameObject insaneVFX;

        [Header("Run Stage Stat")]
        public float runStageSpeedMultiply = 1;

        [Header("Scenario Stat")]
        public float scenarioSpeedMultiply = 1;

        [Header("UI")]
        // UI Hover Chef
        public GameObject canvasIngredients;
        [SerializeField] private Image ingredientsIcon;
        [SerializeField] private TMP_Text ingredientsCountText;

        // UI Screen Canvas
        [SerializeField] private GameObject HeaderIngredients;
        [SerializeField] private Image HeaderIngredientsIcon;
        [SerializeField] private TMP_Text headerIngredientsCountText;

        [SerializeField] private List<Image> cookingStepUIList;
        [SerializeField] private Sprite currentStepSprite;
        [SerializeField] private Sprite otherStepSprite;

        [Header("Animation")]
        [SerializeField] GameObject panProp;
        [SerializeField] GameObject potProp;
        [SerializeField] GameObject ladleProp;
        [SerializeField] GameObject doughProp;

        public string currentAnimation;

        [Header("Item")]
        [SerializeField] GameObject foodHolding;
        [SerializeField] Transform holdingFoodPoint;
        [SerializeField] Transform spawnIngredientPos;
        [SerializeField] GameObject ingredientCurrent;

        [Header("Chef Aura")]
        [SerializeField] private ScriptableRendererFeature _fullScreenDamage;
        [SerializeField] private Material _material;
        public GameObject fireChef_VFX;

        [SerializeField][Range(0f, 1f)] private float _AuraSizeStat;
        [SerializeField] private float _hurtDisplayTime = 1.5f;
        [SerializeField] private float _hurtFadeOut = 1.5f;
        [SerializeField] private float _hurtFadeIn = 1.5f;

        private int _AuraSize = Shader.PropertyToID("_AuraSize");
        private bool isAngry = false;

        [Header("Chef VFX")]
        [SerializeField] private Material cuttingVFXMat;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            PlayAnimTarget("Idle_1");
            currentAnimation = "Idle_1";

            hasGetIngredients = false;

            panProp.SetActive(false);
            potProp.SetActive(false);
            ladleProp.SetActive(false);

            if (doughProp != null)
                doughProp.SetActive(false);

            _fullScreenDamage.SetActive(false);
            _material.SetFloat(_AuraSize, 20f);
            isAngry = false;

            // สมมุติว่า stationList จัดเรียงตามสถานีที่ต้องการ, หรือคุณสามารถปรับตามข้อมูลจริงได้
            // เพิ่ม Transform เข้า Dictionary โดยใช้ชื่อของสถานีเป็น Key
            foreach (var station in stationList)
            {
                stationDict[station.name] = station; // ใช้ชื่อของ GameObject เป็นชื่อของสถานี
            }

            for (int i = 0; i < stationList.Count; i++)
            {
                stationDirDict[stationList[i].name] = stationDirList[i];
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (StageManager.instance.isGameWin)
            {
                if (!isGameWin)
                {
                    if (foodHolding != null)
                    {
                        Destroy(foodHolding);
                    }

                    foodHolding = null;

                    PlayAnimTarget("Idle_1");
                    currentAnimation = "Idle_1";

                    receivedIngredients.Clear();
                    food = null; // เคลียร์ข้อมูลอาหาร
                    currentStepIndex = 0;

                    GoToStation("Kitchen Table");

                    isGameWin = true;
                }

                return;
            }

            if (kitchenTable.Orders.Count > 0 && food == null)
            {
                StartCooking();
            }

            RotateTowardsTarget();
        }

        private void StartCooking()
        {
            kitchenTable.TakeOrder(out var foodSO, out var orderIndex);
            food = foodSO;
            multiplyOrder = orderIndex;

            if (food != null && food.CookingSteps.Count > 0)
            {
                currentStepIndex = 0; // เริ่มจากขั้นตอนแรก
                currentStep = food.CookingSteps[currentStepIndex]; // ดึง CookingStep แรกมาใช้

                for (int i = 0; i < food.CookingSteps.Count; i++)
                {
                    cookingStepUIList[i].gameObject.SetActive(true);
                    if (i == currentStepIndex)
                        cookingStepUIList[currentStepIndex].sprite = currentStepSprite;
                    else
                        cookingStepUIList[i].sprite = otherStepSprite;
                }

                PlayAnimTarget("Walking");
                currentAnimation = "Walking";

                GoToStation(currentStep.StationName); // เริ่มเดินไปที่สถานีแรก
            }
        }

        // ฟังก์ชันสำหรับเชฟเดินไปที่สถานีที่กำหนด
        private void GoToStation(string stationName)
        {
            currentStation = stationName;

            if (stationDict.TryGetValue(stationName, out Transform station))
            {
                agent.SetDestination(station.position); // ตั้งค่า NavMeshAgent ให้ไปยังตำแหน่งของสถานี
                StartCoroutine(WaitForArrivalAtStation(station)); // รอให้เชฟเดินถึงสถานี
            }
            else
            {
                Debug.LogWarning($"ไม่พบสถานีที่ชื่อว่า {stationName}");
            }
        }

        private void RotateTowardsTarget()
        {
            if (currentStation == "" || !stationDirDict.TryGetValue(currentStation, out Transform targetStation))
                return;

            Vector3 direction = (targetStation.position - transform.position).normalized;
            direction.y = 0f; // ล็อกการหมุนในแนวแกน Y เท่านั้น

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        // ฟังก์ชันที่รอจนกว่าเชฟจะถึงสถานี
        private IEnumerator WaitForArrivalAtStation(Transform station)
        {
            while (Vector3.Distance(transform.position, station.position) > 0.5f)
            {
                yield return null; // รอจนกว่าจะถึงสถานี
            }

            Debug.Log($"เชฟถึงสถานี {station.name} แล้ว");
            if (foodHolding != null)
                foodHolding.SetActive(false);

            if (food != null && food.CookingSteps != null && currentStepIndex < food.CookingSteps.Count)
            {
                // เรียกฟังก์ชันที่รอรับวัตถุดิบก่อน
                WaitForIngredients();
            }
            else
            {
                if (food == null)
                    Debug.LogError("food เป็น null");
                if (food.CookingSteps == null)
                    Debug.LogError("CookingSteps เป็น null");
            }
        }

        // ฟังก์ชันที่รอวัตถุดิบในขั้นตอนนี้
        private void WaitForIngredients()
        {
            hasGetIngredients = true;

            igdNeedType = currentStep.Ingredients;
            igdNeedAmount = currentStep.IngredientQuantity * multiplyOrder;

            var igdIcon = currentStep.Ingredients.IngredientIcon;
            ingredientsIcon.sprite = igdIcon;
            HeaderIngredientsIcon.sprite = igdIcon;

            var igdCount = "0/" + (currentStep.IngredientQuantity * multiplyOrder);
            ingredientsCountText.text = igdCount;
            headerIngredientsCountText.text = igdCount;

            canvasIngredients.SetActive(true);
            HeaderIngredients.SetActive(true);

            PlayAnimTarget("IdleTranTo2");
            currentAnimation = "IdleTranTo2";

            Debug.Log($"เชฟกำลังรอวัตถุดิบ {currentStep.Ingredients.IngredientName} จำนวน {currentStep.IngredientQuantity}");
        }

        // ฟังก์ชันสำหรับรับวัตถุดิบ
        public void ReceiveIngredient(IngredientSO ingredient, int quantity, out bool isCollect)
        {
            isCollect = false;

            // เช็คว่า Ingredient ที่ได้รับตรงกับวัตถุดิบที่ต้องการในขั้นตอนนี้หรือไม่
            if (ingredient == currentStep.Ingredients)
            {
                isCollect = true;
                if (!receivedIngredients.ContainsKey(ingredient))
                {
                    receivedIngredients[ingredient] = 0;
                }

                receivedIngredients[ingredient] += quantity; // เพิ่มจำนวนวัตถุดิบที่ได้รับ
                Debug.Log($"เชฟได้รับ {ingredient.IngredientName} จำนวน {quantity}");

                // เช็คว่าวัตถุดิบครบหรือยัง
                if (CheckIfIngredientsComplete())
                {
                    canvasIngredients.SetActive(false);
                    HeaderIngredients.SetActive(false);

                    ingredientCurrent = Instantiate(ingredient.IngredientPref, spawnIngredientPos.position, Quaternion.identity, spawnIngredientPos);

                    cuttingVFXMat.SetColor("_Tint", ingredient.IngredientColor);

                    PerformCookingStep(); // เริ่มทำอาหารในขั้นตอนนี้ถ้าได้รับวัตถุดิบครบแล้ว
                }

                var igdCount = receivedIngredients[currentStep.Ingredients] + " / " + (currentStep.IngredientQuantity * multiplyOrder);
                ingredientsCountText.text = igdCount;
                headerIngredientsCountText.text = igdCount;

                igdNeedAmount--;

                if (receivedIngredients[currentStep.Ingredients] == (currentStep.IngredientQuantity * multiplyOrder))
                {
                    hasGetIngredients = false;
                    igdNeedType = null;
                    igdNeedAmount = 0;
                }
            }
            else
            {
                Debug.LogWarning($"วัตถุดิบที่ได้รับ {ingredient.IngredientName} ไม่ตรงกับที่ต้องการในขั้นตอนนี้: {currentStep.Ingredients.IngredientName}");
            }
        }

        // ฟังก์ชันสำหรับเช็คว่าวัตถุดิบครบหรือยัง
        public bool CheckIfIngredientsComplete()
        {
            if (currentStep == null || currentStep.Ingredients == null)
            {
                return true; // ไม่มีวัตถุดิบในขั้นตอนนี้
            }

            if (receivedIngredients.ContainsKey(currentStep.Ingredients) &&
                receivedIngredients[currentStep.Ingredients] >= currentStep.IngredientQuantity * multiplyOrder)
            {
                return true; // ได้วัตถุดิบครบแล้ว
            }

            return false; // วัตถุดิบยังไม่ครบ
        }

        // ฟังก์ชันสำหรับทำอาหารในขั้นตอนนี้
        private void PerformCookingStep()
        {
            Debug.Log($"เชฟกำลังทำขั้นตอน {currentStep.StationName}, ใช้เวลา {currentStep.CookingTime} วินาที");

            PlayAnimTarget(currentStep.StationName);
            currentAnimation = currentStep.StationName;

            // กำหนด Prop ที่ต้องเปิดใช้งานตามสถานีที่ทำอาหาร
            switch (currentStep.StationName)
            {
                case "cut_station":
                    AudioManager.instance.PlayStageSFXOneShot("Chef_Cutting");
                    break;
                case "boiled_station":
                    potProp.SetActive(true);
                    ladleProp.SetActive(true);
                    AudioManager.instance.PlayStageSFXOneShot("Chef_Pot");
                    break;
                case "grill_station":
                    panProp.SetActive(true);
                    AudioManager.instance.PlayStageSFXOneShot("Chef_Pan");
                    break;
                case "oven_station":
                    // เพิ่มเงื่อนไขการใช้เตาอบถ้ามี
                    //TODO : Audio
                    break;
                case "dough_station":
                    // เพิ่มเงื่อนไขการใช้สถานีนวดแป้งถ้ามี
                    if (doughProp != null)
                        doughProp.SetActive(true);
                    //TODO : Audio
                    break;
            }

            StartCoroutine(PerformStep());
        }

        private IEnumerator PerformStep()
        {

            animator.speed *= currentStep.CookingTime / CalculateCookingTime();

            yield return new WaitForSeconds(CalculateCookingTime()); // รอจนกว่าการทำอาหารจะเสร็จ         

            animator.speed = 1;

            panProp.SetActive(false);
            ladleProp.SetActive(false);
            potProp.SetActive(false);

            if (doughProp != null)
                doughProp.SetActive(false);

            Destroy(ingredientCurrent);
            ingredientCurrent = null;

            ProceedToNextStep(); // ไปยังขั้นตอนถัดไป
        }

        private float CalculateCookingTime()
        {
            var cookingTime = currentStep.CookingTime;

            if (runStageSpeedMultiply > 0 && runStageSpeedMultiply != 1)
                cookingTime *= runStageSpeedMultiply;

            if (scenarioSpeedMultiply > 0 && scenarioSpeedMultiply != 1)
                cookingTime *= scenarioSpeedMultiply;

            if (isInsane)
                cookingTime *= speedInsaneMultiply;

            return cookingTime;
        }

        // ฟังก์ชันสำหรับไปยังขั้นตอนถัดไป
        private void ProceedToNextStep()
        {
            if (food == null) return;

            currentStepIndex++;

            if (currentStepIndex < food.CookingSteps.Count)
            {
                cookingStepUIList[currentStepIndex].sprite = currentStepSprite;
                currentStep = food.CookingSteps[currentStepIndex]; // เปลี่ยนไปขั้นตอนถัดไป

                if (foodHolding == null)
                {
                    foodHolding = Instantiate(food.FoodPref, holdingFoodPoint);
                }
                else
                {
                    foodHolding.SetActive(true);
                }

                PlayAnimTarget("Walk_Hold");
                currentAnimation = "Walk_Hold";

                GoToStation(currentStep.StationName); // ไปยังสถานีใหม่
            }
            else
            {
                Debug.Log($"เชฟทำ {food.FoodName} เสร็จแล้ว");

                for (int i = 0; i < food.CookingSteps.Count; i++)
                {
                    cookingStepUIList[i].gameObject.SetActive(false);
                }

                foodHolding.SetActive(true);

                PlayAnimTarget("Walk_Hold");
                currentAnimation = "Walk_Hold";

                GoToStation("Kitchen Table"); // ให้เชฟเดินไปที่ Kitchen Table ก่อน
                StartCoroutine(WaitAndFinishCooking()); // รอจนเชฟถึง Kitchen Table แล้วค่อย FinishCooking
            }
        }

        // ฟังก์ชันสำหรับรอให้เชฟเดินไปถึง Kitchen Table และ Finish Cooking
        private IEnumerator WaitAndFinishCooking()
        {
            // รอจนกว่าเชฟจะเดินไปถึง Kitchen Table
            while (Vector3.Distance(transform.position, stationDict["Kitchen Table"].position) > 0.5f)
            {
                yield return null;
            }

            Destroy(foodHolding);
            foodHolding = null;

            PlayAnimTarget("Idle_1");
            currentAnimation = "Idle_1";

            Debug.Log("เชฟถึง Kitchen Table แล้ว");

            kitchenTable.FinishCooking(); // บอกโต๊ะว่าอาหารทำเสร็จแล้ว
            AudioManager.instance.PlayStageSFXOneShot("ChefDropFood");

            receivedIngredients.Clear();
            food = null; // เคลียร์ข้อมูลอาหาร
            currentStepIndex = 0;
        }

        public void PlayAnimTarget(string target)
        {
            if (isInsane)
                target += "_Insane";
            else
                animator.Play("No_Insane");

            animator.Play(target);
        }

        public void PlayAngryAnimation(string target)
        {
            animator.Play(target);
            SetAngryAura();
            StartCoroutine(AngryAnimation(target));
        }

        public void SetAngryAura()
        {
            if (!isAngry)
            {
                StartCoroutine(Angry());
            }
        }

        public void SetInsane()
        {
            isInsane = true;
            agent.speed = insaneWalkSpeed;
            insaneVFX.SetActive(true);
        }

        public void ResetInsane()
        {
            isInsane = false;
            agent.speed = walkSpeed;
            insaneVFX.SetActive(false);
        }

        IEnumerator AngryAnimation(string target)
        {
            // รอจนกว่าแอนิเมชันที่กำลังเล่นอยู่จะจบ
            yield return new WaitUntil(() => IsAnimationFinished(target));

            PlayAnimTarget(currentAnimation);
        }

        // ฟังก์ชันเพื่อตรวจสอบว่าแอนิเมชันจบแล้วหรือยัง
        private bool IsAnimationFinished(string animName)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // ตรวจสอบว่ากำลังเล่นแอนิเมชันที่ต้องการหรือไม่ และตรวจสอบว่าเล่นจบหรือยัง
            return stateInfo.IsName(animName) && stateInfo.normalizedTime >= 1.0f;
        }

        private IEnumerator Angry()
        {
            _fullScreenDamage.SetActive(true);
            isAngry = true;

            float countTime = 0f;

            while (countTime < _hurtFadeIn)
            {
                countTime += Time.deltaTime;
                float auraLerp = countTime / _hurtFadeIn;
                float fadeInStat = Mathf.Lerp(20f, _AuraSizeStat, auraLerp);
                _material.SetFloat(_AuraSize, fadeInStat);

                yield return null; //update loop ว่าผ่านไปแล้ว1เฟรม
            }

            yield return new WaitForSeconds(_hurtDisplayTime);

            countTime = 0f;

            while (countTime < _hurtFadeOut)
            {
                countTime += Time.deltaTime;
                float auraLerp = countTime / _hurtFadeOut;
                float fadeOutStat = Mathf.Lerp(_AuraSizeStat, 20f, auraLerp);
                _material.SetFloat(_AuraSize, fadeOutStat);

                yield return null;

            }

            _fullScreenDamage.SetActive(false);
            isAngry = false;
        }
    }
}