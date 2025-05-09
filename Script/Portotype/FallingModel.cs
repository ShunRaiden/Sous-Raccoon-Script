using SousRaccoon.Customer;
using SousRaccoon.Player;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class FallingModel : MonoBehaviour
    {
        GameObject fallingObject;
        float fallingSpeed;
        float damageToCustomer; // ดาเมจที่จะทำกับลูกค้า
        int damageToPlayer; // ดาเมจที่จะทำกับผู้เล่น

        float groundDetectionRadius = 0.5f; // รัศมีการตรวจจับพื้น
        LayerMask groundLayerMask; // เลเยอร์ที่ใช้ตรวจจับพื้น

        public bool startFalling = false;

        public void SetUpFallingModel(GameObject objectToDestory, float speed, float damageCustomer, int damagePlayer, LayerMask layerMask)
        {
            fallingObject = objectToDestory;
            fallingSpeed = speed;
            damageToCustomer = damageCustomer;
            damageToPlayer = damagePlayer;
            groundLayerMask = layerMask;
        }

        private void Update()
        {
            if (!startFalling) return;

            // ตรวจสอบการชนกับพื้น
            if (CheckGroundCollision())
            {
                Destroy(fallingObject); // ทำลายวัตถุเมื่อชนพื้น
            }
            else
            {
                // เคลื่อนที่ลงด้วยความเร็ว fallingSpeed
                transform.position += Vector3.down * fallingSpeed * Time.deltaTime;
            }
        }

        private bool CheckGroundCollision()
        {
            // ตรวจสอบว่ามี Collider ใดในเลเยอร์พื้นภายในรัศมีหรือไม่
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundDetectionRadius, groundLayerMask);

            return hitColliders.Length > 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Customer"))
            {
                var customer = other.GetComponent<CustomerStatus>();
                if (customer != null)
                {
                    customer.TakeDamageTimeCount(damageToCustomer);
                    Destroy(fallingObject); // ทำลายวัตถุเมื่อชนพื้น
                }
            }

            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerCombatSystem>();
                if (player != null)
                {
                    player.TakeDamage(damageToPlayer);
                    Destroy(fallingObject); // ทำลายวัตถุเมื่อชนพื้น
                }
            }

            if (other.CompareTag("Ground"))
            {
                Debug.LogError("Gounded");
                Destroy(fallingObject); // ทำลายวัตถุเมื่อชนพื้น
            }
        }

        private void OnDrawGizmosSelected()
        {
            // วาดวงกลมเพื่อแสดงรัศมีการตรวจจับใน Scene View
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, groundDetectionRadius);
        }

    }
}
