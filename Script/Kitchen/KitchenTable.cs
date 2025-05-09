using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class KitchenTable : MonoBehaviour
    {
        public List<OrderSO> Orders = new List<OrderSO>();
        public List<int> OrderQuantity = new List<int>();

        // Dictionary ����Ѻ���������Шӹǹ
        public Dictionary<FoodSO, int> foodStorage = new Dictionary<FoodSO, int>();

        public List<FoodTable> foodTableList = new List<FoodTable>(); // ��ʵ����ѵ�ط��١ Spawn ����

        [Header("Order List Canvas")]
        [SerializeField] private GameObject currentPlate;
        [SerializeField] private Image currentOrderIcon;
        [SerializeField] private TMP_Text currentOrderCountText;

        [SerializeField] private Image[] nextOrderIcons;
        [SerializeField] private GameObject[] nextOrderBG;
        [SerializeField] private TMP_Text nextOrderCountText;

        [SerializeField] private TMP_Text overOrderCountText;
        [SerializeField] private GameObject overOrderBG;

        private void Start()
        {
            StageManager.instance.EventOnGameEnd += ClearOrder;
        }

        /// <summary>
        /// Player Add Order To this
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderQuantity"></param>
        public void AddOrders(OrderSO order, int orderQuantity)
        {
            Orders.Add(order);
            OrderQuantity.Add(orderQuantity);
            UpdateOrderIcon();
            AudioManager.instance.PlayStageSFXOneShot("GetItem");
        }

        /// <summary>
        /// Chef Take Order form this
        /// </summary>
        /// <param name="food"></param>
        /// <param name="orderIndex"></param>
        public void TakeOrder(out FoodSO food, out int orderIndex)
        {
            food = Orders[0].FoodType;
            orderIndex = OrderQuantity[0];
        }

        /// <summary>
        /// Chef Sent Food
        /// </summary>
        public void FinishCooking()
        {
            FoodSO cookedFood;

            if (Orders != null && Orders.Count > 0)
            {
                cookedFood = Orders[0].FoodType;
            }
            else
            {
                Debug.LogError("Orders ��ҧ��������բ�����");
                return;
            }

            // ��Ǩ�ͺ��������������� storage �������
            if (!foodStorage.ContainsKey(cookedFood))
            {
                // ����ѧ����������������� storage
                foodStorage[cookedFood] = OrderQuantity[0];

                // �� foodTable �����ҧ������������
                var emptyFoodTable = foodTableList.FirstOrDefault(ft => ft.foodData == null);
                if (emptyFoodTable != null)
                {
                    emptyFoodTable.foodData = cookedFood;
                    emptyFoodTable.foodIndex = OrderQuantity[0];
                    emptyFoodTable.foodIcon.sprite = cookedFood.FoodIcon;
                    emptyFoodTable.canvas.SetActive(true);
                    emptyFoodTable.SetIndex();

                    // Spawn Object ������С�˹����������Ѻ foodTable �����ҧ����
                    var foodPref = foodStorage.FirstOrDefault(ft => ft.Key == cookedFood);
                    emptyFoodTable.foodPref = Instantiate(foodPref.Key.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;
                }
            }
            else
            {
                // �������������� storage ��������ӹǹ
                foodStorage[cookedFood] += OrderQuantity[0];

                // �� foodTable ���ç�Ѻ cookedFood ��������ӹǹ
                var existingFoodTable = foodTableList.FirstOrDefault(ft => ft.foodData == cookedFood);
                if (existingFoodTable != null)
                {
                    existingFoodTable.foodIndex += OrderQuantity[0];
                    existingFoodTable.SetIndex();
                }
            }

            Orders.RemoveAt(0);
            OrderQuantity.RemoveAt(0);
            UpdateOrderIcon();
            AudioManager.instance.PlayStageSFXOneShot("FoodReady");
        }

        private void UpdateOrderIcon()
        {
            foreach (var bg in nextOrderBG)
            {
                bg.SetActive(false);
            }

            // �ó������ order
            if (Orders.Count < 1)
            {
                currentOrderIcon.gameObject.SetActive(false);
                currentPlate.SetActive(false);
                return;
            }

            // �ó��� order ���ҧ���� 1 ��¡��
            currentOrderIcon.gameObject.SetActive(true);
            currentPlate.SetActive(true);
            currentOrderIcon.sprite = Orders[0].FoodType.FoodIcon;
            currentOrderCountText.text = $"{OrderQuantity[0]}";

            // �ó��� order �ҡ���� 1 ��¡��
            int maxNextIcons = Mathf.Min(Orders.Count - 1, nextOrderIcons.Length);

            for (int i = 0; i < maxNextIcons; i++)
            {
                nextOrderIcons[i].sprite = Orders[i + 1].FoodType.FoodIcon;
                nextOrderBG[i].SetActive(true);
                nextOrderCountText.text = OrderQuantity[i + 1].ToString();
            }

            //Over 2 in Order
            if (Orders.Count > 2)
            {
                overOrderBG.SetActive(true);
                var overCount = Orders.Count - 2;
                overOrderCountText.text = $"+{overCount}";
            }
            else
            {
                overOrderBG.SetActive(false);
            }
        }

        public void ClearOrder()
        {
            Orders.Clear();
            OrderQuantity.Clear();
            UpdateOrderIcon();
        }
    }
}

