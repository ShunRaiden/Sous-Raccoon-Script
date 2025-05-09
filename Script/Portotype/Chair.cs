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

        public bool IsOccupied { get; set; } // ʶҹ������������١��ҹ�������
        public bool IsTakeMenu { get; set; } // ʶҹ���Ҷ١��Ժ����������ѧ
        public bool IsSitting { get; set; }// ʶҹ�����١��ҹ�������������
        public bool IsGetFood { get; set; } // ʶҹ�����Ѻ����èҡ Player �����ѧ

        public CustomerStatus customerStatus;
        public CustomerMovement CustomerMovement;

        // �������Ҥس��ҡ��� chairName �١��駪������ѵ��ѵ�
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

