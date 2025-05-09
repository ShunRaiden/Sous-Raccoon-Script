using SousRaccoon.Player;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class AudioListenerFollowPlayer : MonoBehaviour
    {
        [SerializeField] PlayerKitchenAction player;

        void Awake()
        {
            StartCoroutine(OnWaitPlayerSpawned());
        }

        void Update()
        {
            if (player != null)
            {
                transform.position = player.transform.position;
            }
        }

        IEnumerator OnWaitPlayerSpawned()
        {
            // รอจนกว่าจะเจอ PlayerKitchenAction ใน Scene
            yield return new WaitUntil(() =>
            {
                player = FindObjectOfType<PlayerKitchenAction>();
                return player != null;
            });
        }
    }
}
