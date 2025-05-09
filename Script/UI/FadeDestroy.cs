using UnityEngine;

namespace SousRaccoon.UI
{
    public class FadeDestroy : MonoBehaviour
    {
        void Start()
        {
            Destroy(gameObject, 1);
        }
    }
}
