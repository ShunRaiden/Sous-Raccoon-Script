using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Item", menuName = "Restaurant/ItemData")]
    public class ItemDataBase : ScriptableObject
    {
        public string itemName;
        public ItemType itemType;
        public FoodSO food;
        public IngredientSO ingredient;
        public OrderSO order;
        public CustomerTrash trash;
        public GameObject itemPref;
        public GameObject trashPref;

        public UseItemSO useItem;

        public void GetItemIcon(out Sprite icon)
        {
            icon = null;
            switch (itemType)
            {
                case ItemType.Ingredient: icon = ingredient.IngredientIcon; break;
                case ItemType.Order: icon = order.FoodType.FoodIcon; break;
                case ItemType.Food: icon = food.FoodIcon; break;
                case ItemType.Trash: icon = trash.TrashIcon; break;
                case ItemType.UseItem: icon = useItem.UseItemIcon; break;
            }
        }

        public void GetItemPrefab(out GameObject pref)
        {
            pref = null;
            switch (itemType)
            {
                case ItemType.Ingredient: pref = ingredient.IngredientPref; break;
                case ItemType.Order: pref = order.OrderPref; break;
                case ItemType.Food: pref = food.FoodPref; break;
                case ItemType.Trash: pref = trashPref; break;
                    //case ItemType.UseItem: pref = useItem.UseItemPref; break;
            }
        }

        public enum ItemType
        {
            Order,
            Food,
            Ingredient,
            Trash,
            UseItem,
        }
    }
}

