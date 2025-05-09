using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "UseItem", menuName = "Restaurant/UseItem")]
    public class UseItemSO : ScriptableObject
    {
        public string UseItemName;
        public Sprite UseItemIcon;
        //public GameObject UseItemPref;
        public IngredientSO ingredientToUse;
        public float BuffStatfloat;
        public int BuffStatint;
        public float BuffTime;
        public float generateTime;
        public BuffType buffType;

        public enum BuffType
        {
            Speed,
            AttackSpeed,
            AttackDamage,
            HealRate,
            HealRange,
        }
    }
}
