using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using System.Linq;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class ReturnPoint : MonoBehaviour
    {
        public ReturnPointType type;

        public enum ReturnPointType
        {
            kitchen,
            blender
        }

        KitchenTable kitchenTable;
        BlenderTable blenderTable;

        private void Start()
        {
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            blenderTable = FindAnyObjectByType<BlenderTable>();
        }

        public bool ReturnFood(FoodSO foodToReturn)
        {
            switch (type)
            {
                case ReturnPointType.kitchen:
                    return OnKitchenTable(foodToReturn);

                case ReturnPointType.blender:
                    return OnBlenderTable(foodToReturn);
                default:
                    // ถ้าไม่มีโต๊ะที่ว่าง ให้แสดงข้อความเตือน
                    Debug.LogWarning("ไม่รู้สาเหตุแต่แม่งไม่ได้อะ");
                    return false; // การคืนอาหารล้มเหลว
            }

        }

        public bool OnKitchenTable(FoodSO foodToReturn)
        {
            // ค้นหา FoodTable ที่มีอาหารชนิดเดียวกัน
            FoodTable matchingFoodTable;

            matchingFoodTable = kitchenTable.foodTableList.FirstOrDefault(ft => ft.foodData == foodToReturn);

            if (matchingFoodTable != null)
            {
                // ถ้ามี FoodTable อื่นที่มีอาหารชนิดเดียวกัน เพิ่มจำนวนในโต๊ะนั้น
                matchingFoodTable.foodIndex += 1;
                kitchenTable.foodStorage[foodToReturn] += 1;
                matchingFoodTable.SetIndex();
                Debug.Log("เพิ่มอาหารใน FoodTable ที่มีอยู่แล้ว");

                StageManager.instance.playerKitchenAction.CurrentItemCount--;

                return true; // คืนอาหารสำเร็จ
            }
            else
            {
                // หาโต๊ะที่ว่าง (ไม่มีอาหารอยู่บนโต๊ะ)
                var emptyFoodTable = kitchenTable.foodTableList.FirstOrDefault(ft => ft.foodData == null);

                if (emptyFoodTable != null)
                {
                    // ถ้ามีโต๊ะที่ว่าง เพิ่มอาหารในโต๊ะนั้น
                    emptyFoodTable.foodData = foodToReturn;
                    kitchenTable.foodStorage[foodToReturn] = 1; // เริ่มต้นด้วยจำนวน 1

                    // อัปเดต Icon
                    emptyFoodTable.foodIcon.sprite = foodToReturn.FoodIcon;

                    // อัปเดตจำนวนใน foodTable
                    emptyFoodTable.foodIndex = 1;
                    emptyFoodTable.SetIndex();

                    // สร้าง foodPref ใหม่บนโต๊ะว่าง
                    emptyFoodTable.foodPref = Instantiate(foodToReturn.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;

                    // แสดง UI
                    emptyFoodTable.canvas.SetActive(true);

                    StageManager.instance.playerKitchenAction.CurrentItemCount--;

                    Debug.Log("เพิ่มอาหารใหม่ในโต๊ะว่าง");
                    return true; // คืนอาหารสำเร็จ
                }
                else
                {
                    // ถ้าไม่มีโต๊ะที่ว่าง ให้แสดงข้อความเตือน
                    Debug.LogWarning("ไม่มีโต๊ะที่ว่างสำหรับอาหารที่ต้องการคืน");
                    return false; // การคืนอาหารล้มเหลว
                }
            }
        }

        public bool OnBlenderTable(FoodSO foodToReturn)
        {
            // ค้นหา FoodTable ที่มีอาหารชนิดเดียวกัน
            FoodTable matchingFoodTable;

            matchingFoodTable = blenderTable.foodTableList.FirstOrDefault(ft => ft.foodData == foodToReturn);

            if (matchingFoodTable != null)
            {
                // ถ้ามี FoodTable อื่นที่มีอาหารชนิดเดียวกัน เพิ่มจำนวนในโต๊ะนั้น
                matchingFoodTable.foodIndex += 1;
                blenderTable.foodStorage[foodToReturn] += 1;
                matchingFoodTable.SetIndex();
                Debug.Log("เพิ่มอาหารใน FoodTable ที่มีอยู่แล้ว");

                StageManager.instance.playerKitchenAction.CurrentItemCount--;

                return true; // คืนอาหารสำเร็จ
            }
            else
            {
                // หาโต๊ะที่ว่าง (ไม่มีอาหารอยู่บนโต๊ะ)
                var emptyFoodTable = blenderTable.foodTableList.FirstOrDefault(ft => ft.foodData == null);

                if (emptyFoodTable != null)
                {
                    // ถ้ามีโต๊ะที่ว่าง เพิ่มอาหารในโต๊ะนั้น
                    emptyFoodTable.foodData = foodToReturn;
                    blenderTable.foodStorage[foodToReturn] = 1; // เริ่มต้นด้วยจำนวน 1

                    // อัปเดต Icon
                    emptyFoodTable.foodIcon.sprite = foodToReturn.FoodIcon;

                    // อัปเดตจำนวนใน foodTable
                    emptyFoodTable.foodIndex = 1;
                    emptyFoodTable.SetIndex();

                    // สร้าง foodPref ใหม่บนโต๊ะว่าง
                    emptyFoodTable.foodPref = Instantiate(foodToReturn.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;

                    // แสดง UI
                    emptyFoodTable.canvas.SetActive(true);

                    StageManager.instance.playerKitchenAction.CurrentItemCount--;

                    Debug.Log("เพิ่มอาหารใหม่ในโต๊ะว่าง");
                    return true; // คืนอาหารสำเร็จ
                }
                else
                {
                    // ถ้าไม่มีโต๊ะที่ว่าง ให้แสดงข้อความเตือน
                    Debug.LogWarning("ไม่มีโต๊ะที่ว่างสำหรับอาหารที่ต้องการคืน");
                    return false; // การคืนอาหารล้มเหลว
                }
            }
        }
    }
}