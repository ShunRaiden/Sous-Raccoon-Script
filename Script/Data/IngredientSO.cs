using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Ingredient", menuName = "Restaurant/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        public string IngredientName; // �����ѵ�شԺ
        public Sprite IngredientIcon; // Icon �ѵ�شԺ
        public int IngredientQuantity;
        public GameObject IngredientPref;
        public Color IngredientColor;
    }
}
