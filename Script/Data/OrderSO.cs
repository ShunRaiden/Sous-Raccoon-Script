using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "Order", menuName = "Restaurant/Order")]
    public class OrderSO : ScriptableObject
    {
        public FoodSO FoodType;
        public GameObject OrderPref;
        public OrderType type;
        public enum OrderType
        {
            Food,
            Drink,
        }
    }
}