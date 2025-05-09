using SousRaccoon.Data.Item;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class TrashCan : MonoBehaviour
    {
        public bool isOutSide;
        public ItemDataBase ingredientItem;
        public Animator animator;

        public void PlayAnimation()
        {
            animator.Play("Trash_Anim");
        }
    }
}

