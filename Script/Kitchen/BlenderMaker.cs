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

        private CookingStep currentStep; // ��鹵͹��÷�����÷����ѧ������
        [SerializeField] private int currentStepIndex = 0; // ������纵��˹觢ͧ��鹵͹�Ѩ�غѹ� CookingSteps
        private Dictionary<IngredientSO, int> receivedIngredients = new(); // �纨ӹǹ�ѵ�شԺ������Ѻ

        public IngredientSO igdNeedType;
        public int igdNeedAmount;

        [SerializeField] private int multiplyOrder;
        public bool hasGetIngredients { get; private set; } //������Դ�Ѻ�ѵ�شԺ�����

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
                    food = null; // ����������������
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
                currentStepIndex = 0; // ������ҡ��鹵͹�á
                currentStep = food.CookingSteps[currentStepIndex]; // �֧ CookingStep �á����

                WaitForIngredients();
            }
        }

        // �ѧ��ѹ������ѵ�شԺ㹢�鹵͹���
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

            // ����� Ingredient ������Ѻ�ç�Ѻ�ѵ�شԺ����ͧ���㹢�鹵͹����������
            if (ingredient == currentStep.Ingredients)
            {
                isCollect = true;
                if (!receivedIngredients.ContainsKey(ingredient))
                {
                    receivedIngredients[ingredient] = 0;
                }

                receivedIngredients[ingredient] += quantity; // �����ӹǹ�ѵ�شԺ������Ѻ
                Debug.Log($"િ���Ѻ {ingredient.IngredientName} �ӹǹ {quantity}");

                var igdCount = receivedIngredients[currentStep.Ingredients] + " / " + (currentStep.IngredientQuantity * multiplyOrder);
                ingredientsCountText.text = igdCount;

                igdNeedAmount--;

                if (receivedIngredients[currentStep.Ingredients] == (currentStep.IngredientQuantity * multiplyOrder))
                {
                    hasGetIngredients = false;
                    igdNeedType = null;
                    igdNeedAmount = 0;
                }

                // ������ѵ�شԺ�ú�����ѧ
                if (CheckIfIngredientsComplete())
                {
                    canvasIngredients.SetActive(false);

                    PerformCookingStep();
                }
            }
            else
            {
                Debug.LogWarning($"�ѵ�شԺ������Ѻ {ingredient.IngredientName} ���ç�Ѻ����ͧ���㹢�鹵͹���: {currentStep.Ingredients.IngredientName}");
            }
        }

        // �ѧ��ѹ����Ѻ������ѵ�شԺ�ú�����ѧ
        public bool CheckIfIngredientsComplete()
        {
            if (currentStep == null || currentStep.Ingredients == null)
            {
                return true; // ������ѵ�شԺ㹢�鹵͹���
            }

            if (receivedIngredients.ContainsKey(currentStep.Ingredients) &&
                receivedIngredients[currentStep.Ingredients] >= currentStep.IngredientQuantity * multiplyOrder)
            {
                return true; // ���ѵ�شԺ�ú����
            }

            return false; // �ѵ�شԺ�ѧ���ú
        }

        // �ѧ��ѹ����Ѻ�������㹢�鹵͹���
        private void PerformCookingStep()
        {
            StartCoroutine(PerformStep());
        }

        private IEnumerator PerformStep()
        {
            PlayTargetAnimation("Blending");

            yield return new WaitForSeconds(currentStep.CookingTime); // �ͨ����ҡ�÷�����è�����         

            ProceedToNextStep(); // ��ѧ��鹵͹�Ѵ�
        }

        // �ѧ��ѹ����Ѻ��ѧ��鹵͹�Ѵ�
        private void ProceedToNextStep()
        {
            if (food == null) return;

            currentStepIndex++;

            if (currentStepIndex < food.CookingSteps.Count)
            {
                currentStep = food.CookingSteps[currentStepIndex]; // ����¹仢�鹵͹�Ѵ�

                WaitForIngredients();
            }
            else
            {
                PlayTargetAnimation("Idle");
                blenderTable.FinishCooking(); // �͡����������÷���������
                receivedIngredients.Clear();
                food = null; // ����������������
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
