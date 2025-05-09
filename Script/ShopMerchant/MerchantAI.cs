using SousRaccoon.Manager;
using UnityEngine;
using UnityEngine.AI;

namespace SousRaccoon.Kitchen
{
    public class MerchantAI : MonoBehaviour
    {
        public ShopMerchantManager shopMerchantManager;

        public Transform targetDestination;
        public Transform targetDirection;

        public GameObject shopIcon;

        public bool canOpenShop = false;

        private NavMeshAgent agent;
        private Animator animator;

        private bool hasReachedDestination = false;
        private float rotationSpeed = 2.0f; // ปรับความเร็วการหมุน

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();

            StartWalking("Walk");
        }

        void Update()
        {
            if (!hasReachedDestination && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    ReachDestination();
                }
            }

            if (hasReachedDestination)
            {
                RotateTowardsTarget();
            }
        }

        public void StartWalking(string targetAnim)
        {
            agent.SetDestination(targetDestination.position);
            animator.Play(targetAnim);
            hasReachedDestination = false;
        }

        private void ReachDestination()
        {
            shopIcon.SetActive(true);
            hasReachedDestination = true;
            animator.Play("Idle"); // เปลี่ยนเป็นแอนิเมชัน Idle
            canOpenShop = true;
        }

        private void RotateTowardsTarget()
        {
            Vector3 direction = targetDirection.position - transform.position;
            direction.y = 0; // ล็อกแกน Y เพื่อป้องกันการหมุนขึ้นลง

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
