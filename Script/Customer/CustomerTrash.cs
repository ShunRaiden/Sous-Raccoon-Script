using UnityEngine;

namespace SousRaccoon.Data.Item
{
    public class CustomerTrash : MonoBehaviour
    {
        public Sprite TrashIcon; // Icon ¢ÂÐ

        public Vector3 yPos;

        public void SetPos()
        {
            transform.position += yPos;
        }
    }
}

