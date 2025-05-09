using SousRaccoon.Player;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class MovingPlatform : MonoBehaviour
    {
        private Vector3 lastPlatformPosition;
        //private PlayerLocomotion playerLoco;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                lastPlatformPosition = transform.position;
                other.transform.SetParent(transform);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerController = other.GetComponent<CharacterController>();
                var playerLoco = other.GetComponent<PlayerLocomotion>();

                if (playerController != null)
                {
                    Vector3 deltaPosition = transform.position - lastPlatformPosition;
                    playerController.Move(deltaPosition); // ขยับ Player ตาม Platform
                    lastPlatformPosition = transform.position;
                }

                if (playerLoco != null && !playerLoco.isGrounded)
                {
                    other.transform.SetParent(null);
                }

                if (!GetComponent<Collider>().bounds.Intersects(other.bounds))
                {
                    Debug.Log("Exit (Forcing) - Player out of bounds");
                    other.transform.SetParent(null);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Exit");
                other.transform.SetParent(null);
            }
        }
    }
}