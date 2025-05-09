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

        // Dictionary สำหรับเก็บอาหารและจำนวน
        public Dictionary<FoodSO, int> foodStorage = new Dictionary<FoodSO, int>();

        public List<FoodTable> foodTableList = new List<FoodTable>(); // ลิสต์เก็บวัตถุที่ถูก Spawn แล้ว

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
                Debug.LogError("Orders ว่างหรือไม่มีข้อมูล");
                return;
            }

            // ตรวจสอบว่าอาหารมีอยู่ใน storage หรือไม่
            if (!foodStorage.ContainsKey(cookedFood))
            {
                // ถ้ายังไม่มีให้เพิ่มเข้าไปใน storage
                foodStorage[cookedFood] = OrderQuantity[0];

                // หา foodTable ที่ว่างเพื่อใส่ข้อมูล
                var emptyFoodTable = foodTableList.FirstOrDefault(ft => ft.foodData == null);
                if (emptyFoodTable != null)
                {
                    emptyFoodTable.foodData = cookedFood;
                    emptyFoodTable.foodIndex = OrderQuantity[0];
                    emptyFoodTable.foodIcon.sprite = cookedFood.FoodIcon;
                    emptyFoodTable.canvas.SetActive(true);
                    emptyFoodTable.SetIndex();

                    // Spawn Object ใหม่และกำหนดข้อมูลให้กับ foodTable ที่ว่างอยู่
                    var foodPref = foodStorage.FirstOrDefault(ft => ft.Key == cookedFood);
                    emptyFoodTable.foodPref = Instantiate(foodPref.Key.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;
                }
            }
            else
            {
                // ถ้ามีอยู่แล้วใน storage ให้เพิ่มจำนวน
                foodStorage[cookedFood] += OrderQuantity[0];

                // หา foodTable ที่ตรงกับ cookedFood และเพิ่มจำนวน
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

            // กรณีไม่มี order
            if (Orders.Count < 1)
            {
                currentOrderIcon.gameObject.SetActive(false);
                currentPlate.SetActive(false);
                return;
            }

            // กรณีมี order อย่างน้อย 1 รายการ
            currentOrderIcon.gameObject.SetActive(true);
            currentPlate.SetActive(true);
            currentOrderIcon.sprite = Orders[0].FoodType.FoodIcon;
            currentOrderCountText.text = $"{OrderQuantity[0]}";

            // กรณีมี order มากกว่า 1 รายการ
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

