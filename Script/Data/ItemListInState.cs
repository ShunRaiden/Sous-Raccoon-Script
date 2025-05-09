using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "ItemList", menuName = "Restaurant/ItemList")]
    public class ItemListInState : ScriptableObject
    {
        public List<ItemDataBase> FoodItem;
        public List<ItemDataBase> OrderItem;
        public List<ItemDataBase> IngredientsItem;
        public ItemDataBase trashItem;
    }
}

