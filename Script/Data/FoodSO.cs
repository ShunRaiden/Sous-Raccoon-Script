using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Food", menuName = "Restaurant/Food")]
    public class FoodSO : ScriptableObject
    {
        public string FoodName; // ���������
        public Sprite FoodIcon; // Icon �����
        public GameObject FoodPref; // Prefab �ͧ�����
        public List<CookingStep> CookingSteps;
        public FoodType TypeFood;

        public enum FoodType
        {
            Food,
            Drink,
        }
    }

    [System.Serializable]
    public class CookingStep
    {
        public IngredientSO Ingredients;
        public int IngredientQuantity;
        public string StationName;  // ���͢�鹵͹
        public float CookingTime;  // ���ҷ����㹢�鹵͹���
    }
}
