using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Ingredient", menuName = "Restaurant/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        public string IngredientName; // ชื่อวัตถุดิบ
        public Sprite IngredientIcon; // Icon วัตถุดิบ
        public int IngredientQuantity;
        public GameObject IngredientPref;
        public Color IngredientColor;
    }
}
