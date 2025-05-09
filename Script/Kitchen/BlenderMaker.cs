using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using SousRaccoon.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class BlenderMaker : MonoBehaviour
    {
        // Get By State
        public PlayerKitchenAction player;
        bool isGameWin = false;

        BlenderTable blenderTable;
        [SerializeField] Animator animator;

        [SerializeField] private FoodSO food;

        private CookingStep currentStep; // ขั้นตอนการทำอาหารที่กำลังทำอยู่
        [SerializeField] private int currentStepIndex = 0; // ตัวแปรเก็บตำแหน่งของขั้นตอนปัจจุบันใน CookingSteps
        private Dictionary<IngredientSO, int> receivedIngredients = new(); // เก็บจำนวนวัตถุดิบที่ได้รับ

        public IngredientSO igdNeedType;
        public int igdNeedAmount;

        [SerializeField] private int multiplyOrder;
        public bool hasGetIngredients { get; private set; } //เช็คว่าเปิดรับวัตถุดิบได้ไหม

        [Header("UI")]
        // UI Hover Chef
        public GameObject canvasIngredients;
        [SerializeField] private Image ingredientsIcon;
        [SerializeField] private TMP_Text ingredientsCountText;

        // Start is called before the first frame update
        void Start()
        {
            blenderTable = FindAnyObjectByType<BlenderTable>();
            PlayTargetAnimation("Idle");

            hasGetIngredients = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (StageManager.instance.isGameWin)
            {
                if (!isGameWin)
                {
                    receivedIngredients.Clear();
                    food = null; // เคลียร์ข้อมูลอาหาร
                    currentStepIndex = 0;
                    isGameWin = true;
                }

                return;
            }

            if (blenderTable.Orders.Count > 0 && food == null)
            {
                StartCooking();
            }
        }

        private void StartCooking()
        {
            blenderTable.TakeOrder(out var foodSO, out var orderIndex);
            food = foodSO;
            multiplyOrder = orderIndex;

            if (food != null && food.CookingSteps.Count > 0)
            {
                currentStepIndex = 0; // เริ่มจากขั้นตอนแรก
                currentStep = food.CookingSteps[currentStepIndex]; // ดึง CookingStep แรกมาใช้

                WaitForIngredients();
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

            var igdCount = "0/" + (currentStep.IngredientQuantity * multiplyOrder);
            ingredientsCountText.text = igdCount;

            canvasIngredients.SetActive(true);

            PlayTargetAnimation("Wait");
        }

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

                var igdCount = receivedIngredients[currentStep.Ingredients] + " / " + (currentStep.IngredientQuantity * multiplyOrder);
                ingredientsCountText.text = igdCount;

                igdNeedAmount--;

                if (receivedIngredients[currentStep.Ingredients] == (currentStep.IngredientQuantity * multiplyOrder))
                {
                    hasGetIngredients = false;
                    igdNeedType = null;
                    igdNeedAmount = 0;
                }

                // เช็คว่าวัตถุดิบครบหรือยัง
                if (CheckIfIngredientsComplete())
                {
                    canvasIngredients.SetActive(false);

                    PerformCookingStep();
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
            StartCoroutine(PerformStep());
        }

        private IEnumerator PerformStep()
        {
            PlayTargetAnimation("Blending");

            yield return new WaitForSeconds(currentStep.CookingTime); // รอจนกว่าการทำอาหารจะเสร็จ         

            ProceedToNextStep(); // ไปยังขั้นตอนถัดไป
        }

        // ฟังก์ชันสำหรับไปยังขั้นตอนถัดไป
        private void ProceedToNextStep()
        {
            if (food == null) return;

            currentStepIndex++;

            if (currentStepIndex < food.CookingSteps.Count)
            {
                currentStep = food.CookingSteps[currentStepIndex]; // เปลี่ยนไปขั้นตอนถัดไป

                WaitForIngredients();
            }
            else
            {
                PlayTargetAnimation("Idle");
                blenderTable.FinishCooking(); // บอกโต๊ะว่าอาหารทำเสร็จแล้ว
                receivedIngredients.Clear();
                food = null; // เคลียร์ข้อมูลอาหาร
                currentStepIndex = 0;
            }
        }

        private void PlayTargetAnimation(string animName)
        {
            if (animator != null)
                animator.Play(animName);
        }
    }
}
