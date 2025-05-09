using SousRaccoon.Customer;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class Chair : MonoBehaviour
    {
        public string chairName;
        public int chairNumber;
        public Transform chairPosition;
        public Transform walkInPositon;
        public Transform foodPlacePositon;

        private GameObject currentFood;

        public bool IsOccupied { get; set; } // สถานะว่าเก้าอี้นี้ถูกใช้งานหรือไม่
        public bool IsTakeMenu { get; set; } // สถานะว่าถูกหยิบเมนูไปหรือยัง
        public bool IsSitting { get; set; }// สถานะว่าลูกค้านั่งอยู่หรือไม่
        public bool IsGetFood { get; set; } // สถานะว่ารับอาหารจาก Player หรือยัง

        public CustomerStatus customerStatus;
        public CustomerMovement CustomerMovement;

        // สมมติว่าคุณอยากให้ chairName ถูกตั้งชื่อโดยอัตโนมัติ
        private void Awake()
        {
            chairName = $"Chair{chairNumber}";
        }

        public void SpawnFood(GameObject foodPref)
        {
            currentFood = Instantiate(foodPref, foodPlacePositon);
        }

        public void DestroyFood()
        {
            Destroy(currentFood);
        }
    }
}

