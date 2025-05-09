using SousRaccoon.Data.Item;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class FoodTable : MonoBehaviour
    {
        public FoodTableType type;

        public enum FoodTableType
        {
            kitchen,
            blender
        }

        KitchenTable kitchenTable;
        BlenderTable blenderTable;

        public GameObject foodPref;
        public FoodSO foodData;
        public int foodIndex = 0;
        public Transform spawnPos;
        public Collider foodCollider;
        public HighlightObject highlightFood;

        [Header("Canvas")]
        public GameObject canvas;
        public Image foodIcon;
        public TMP_Text index_Text;

        private void Start()
        {
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            blenderTable = FindAnyObjectByType<BlenderTable>();
            foodCollider.enabled = false;
        }

        public FoodSO GiveFood(ItemDataBase playerItem)
        {
            switch (type)
            {
                case FoodTableType.kitchen:
                    return OnKitchenTable(playerItem);
                case FoodTableType.blender:
                    return OnBlenderTable(playerItem);
                default: return null;
            }
        }

        public FoodSO OnKitchenTable(ItemDataBase playerItem)
        {
            // กำหนดค่าเริ่มต้นให้ foodToReturn เป็น null
            FoodSO foodToReturn = null;

            // ถ้า playerItem เป็น null, ใช้ foodData ตรวจสอบใน foodStorage
            if (playerItem == null && kitchenTable.foodStorage.TryGetValue(foodData, out int foodCount) && foodCount > 0)
            {
                foodToReturn = foodData;
            }
            else
            {
                if (playerItem.food.FoodName == foodData.FoodName)
                {
                    // หาอาหารที่ตรงกับ playerItem
                    var matchingFood = kitchenTable.foodStorage.FirstOrDefault(entry => entry.Key.FoodName == playerItem?.food.FoodName);
                    if (matchingFood.Key != null && matchingFood.Value > 0)
                    {
                        foodToReturn = matchingFood.Key;
                    }
                }
                else
                {
                    foodToReturn = null;
                }
            }

            // ถ้า foodToReturn ไม่ใช่ null, ลดจำนวนใน storage
            if (foodToReturn != null)
            {
                kitchenTable.foodStorage[foodToReturn] -= 1;
                foodIndex -= 1;

                // ถ้าค่า Value เท่ากับ 0 ให้ลบออกจาก storage และ foodTableList
                if (kitchenTable.foodStorage[foodToReturn] <= 0)
                {
                    // ลบออกจาก storage
                    ResetFoodHighlightMaterial();
                    kitchenTable.foodStorage.Remove(foodToReturn);
                    Destroy(foodPref);
                    foodCollider.enabled = false;
                    foodPref = null;

                    // รีเซ็ตค่า foodData และ foodIndexK
                    foodData = null;
                    foodIndex = 0;
                    canvas.SetActive(false);
                }

                SetIndex();

                return foodToReturn; // คืนค่าอาหารที่พบ
            }

            Debug.LogWarning("ไม่พบอาหารที่ตรงกันใน storage");
            return null; // ถ้าไม่พบอาหาร
        }

        public FoodSO OnBlenderTable(ItemDataBase playerItem)
        {
            // กำหนดค่าเริ่มต้นให้ foodToReturn เป็น null
            FoodSO foodToReturn = null;

            // ถ้า playerItem เป็น null, ใช้ foodData ตรวจสอบใน foodStorage
            if (playerItem == null && blenderTable.foodStorage.TryGetValue(foodData, out int foodCount) && foodCount > 0)
            {
                foodToReturn = foodData;
            }
            else
            {
                if (playerItem.food.FoodName == foodData.FoodName)
                {
                    // หาอาหารที่ตรงกับ playerItem
                    var matchingFood = blenderTable.foodStorage.FirstOrDefault(entry => entry.Key.FoodName == playerItem?.food.FoodName);
                    if (matchingFood.Key != null && matchingFood.Value > 0)
                    {
                        foodToReturn = matchingFood.Key;
                    }
                }
                else
                {
                    foodToReturn = null;
                }
            }

            // ถ้า foodToReturn ไม่ใช่ null, ลดจำนวนใน storage
            if (foodToReturn != null)
            {
                blenderTable.foodStorage[foodToReturn] -= 1;
                foodIndex -= 1;

                // ถ้าค่า Value เท่ากับ 0 ให้ลบออกจาก storage และ foodTableList
                if (blenderTable.foodStorage[foodToReturn] <= 0)
                {
                    // ลบออกจาก storage
                    ResetFoodHighlightMaterial();
                    blenderTable.foodStorage.Remove(foodToReturn);
                    Destroy(foodPref);
                    foodCollider.enabled = false;
                    foodPref = null;

                    // รีเซ็ตค่า foodData และ foodIndexK
                    foodData = null;
                    foodIndex = 0;
                    canvas.SetActive(false);
                }

                SetIndex();

                return foodToReturn; // คืนค่าอาหารที่พบ
            }

            Debug.LogWarning("ไม่พบอาหารที่ตรงกันใน storage");
            return null; // ถ้าไม่พบอาหาร
        }

        public void SetIndex()
        {
            index_Text.text = foodIndex.ToString();
        }

        public void SetFoodHighlightMaterial() => UpdateFoodHighlightMaterial(true);
        public void ResetFoodHighlightMaterial() => UpdateFoodHighlightMaterial(false);

        public void UpdateFoodHighlightMaterial(bool isAdd)
        {
            if (foodPref == null) return;

            var foodHL = foodPref.GetComponent<HighlightFood>();
            if (foodHL == null) return;

            if (isAdd)
            {
                highlightFood.meshRenderer.Add(foodHL.meshRenderer);
                highlightFood.defaultMat.Add(foodHL.defalutMaterial);
                highlightFood.highlightMat.Add(foodHL.highlightMaterial);
            }
            else
            {
                highlightFood.meshRenderer.Remove(foodHL.meshRenderer);
                highlightFood.defaultMat.Remove(foodHL.defalutMaterial);
                highlightFood.highlightMat.Remove(foodHL.highlightMaterial);
            }
        }
    }
}
