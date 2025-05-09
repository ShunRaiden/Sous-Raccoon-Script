using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Food", menuName = "Restaurant/Food")]
    public class FoodSO : ScriptableObject
    {
        public string FoodName; // ชื่ออาหาร
        public Sprite FoodIcon; // Icon อาหาร
        public GameObject FoodPref; // Prefab ของอาหาร
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
        public string StationName;  // ชื่อขั้นตอน
        public float CookingTime;  // เวลาที่ใช้ในขั้นตอนนี้
    }
}
