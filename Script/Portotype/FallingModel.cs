using SousRaccoon.Customer;
using SousRaccoon.Player;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class FallingModel : MonoBehaviour
    {
        GameObject fallingObject;
        float fallingSpeed;
        float damageToCustomer; // ��������зӡѺ�١���
        int damageToPlayer; // ��������зӡѺ������

        float groundDetectionRadius = 0.5f; // ����ա�õ�Ǩ�Ѻ���
        LayerMask groundLayerMask; // ������������Ǩ�Ѻ���

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

            // ��Ǩ�ͺ��ê��Ѻ���
            if (CheckGroundCollision())
            {
                Destroy(fallingObject); // ������ѵ������ͪ����
            }
            else
            {
                // ����͹���ŧ���¤������� fallingSpeed
                transform.position += Vector3.down * fallingSpeed * Time.deltaTime;
            }
        }

        private bool CheckGroundCollision()
        {
            // ��Ǩ�ͺ����� Collider ���������������������������
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
                    Destroy(fallingObject); // ������ѵ������ͪ����
                }
            }

            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerCombatSystem>();
                if (player != null)
                {
                    player.TakeDamage(damageToPlayer);
                    Destroy(fallingObject); // ������ѵ������ͪ����
                }
            }

            if (other.CompareTag("Ground"))
            {
                Debug.LogError("Gounded");
                Destroy(fallingObject); // ������ѵ������ͪ����
            }
        }

        private void OnDrawGizmosSelected()
        {
            // �Ҵǧ��������ʴ�����ա�õ�Ǩ�Ѻ� Scene View
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, groundDetectionRadius);
        }

    }
}
