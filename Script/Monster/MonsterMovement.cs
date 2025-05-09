using SousRaccoon.Customer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static SousRaccoon.Customer.CustomerStatus;

namespace SousRaccoon.Monster
{
    public abstract class MonsterMovement : MonoBehaviour
    {
        protected NavMeshAgent agent;
        [SerializeField] protected Transform currentTarget;
        [SerializeField] protected Transform barricadeTarget; // �������������Ѻ������� Barricade
        protected Animator animator;

        [Header("Movement Settings")]
        public float rotationSpeed = 5f;
        [SerializeField] protected float lockDistance = 2f;

        [Header("Action Settings")]
        public float attackRange = 2f;
        public float extraAttackRange = 0.5f;
        public float detectBarricadeRange = 3f; // �������е�Ǩ�Ѻ Barricade
        public float updateTargetTime = 0.5f;

        [Header("Animation Time")]
        public float attackChargeTime;
        public float attackRecoveryTime;
        public float dyingTime;

        [Header("Stun Settings")]
        public float stunTime;

        protected List<Transform> targetPoint = new();
        public bool isDead;
        protected bool isStartAttact;

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();

            // �������������������·ء � �Թҷ�
            StartCoroutine(UpdateTargetRoutine());
        }

        protected virtual void Update()
        {
            if (currentTarget != null && !isDead)
                RotateTowardsTarget();
        }

        protected virtual IEnumerator UpdateTargetRoutine()
        {
            while (!isDead)
            {
                FindBarricadeTarget(); // ���� Barricade ��͹
                if (barricadeTarget == null) // �������� Barricade �������ö������ ������ Customer
                {
                    FindClosestTarget();
                }
                yield return new WaitForSeconds(updateTargetTime); // ���������������ء � 0.5 �Թҷ�
            }
        }

        protected virtual void FindBarricadeTarget()
        {
            GameObject[] barricades = GameObject.FindGameObjectsWithTag("Barricade");
            Transform closestBarricade = null;
            float shortestDistance = detectBarricadeRange;

            foreach (GameObject obj in barricades)
            {
                if (obj == null || !obj.activeInHierarchy)
                    continue;

                var barricadeStatus = obj.GetComponent<BarricadeStatus>();
                if (barricadeStatus == null || !barricadeStatus.canTakeDamage)
                    continue;

                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestBarricade = obj.transform;
                }
            }

            if (closestBarricade != null)
            {
                barricadeTarget = closestBarricade;
                currentTarget = barricadeTarget; // ����¹�������价�� Barricade
            }
            else
            {
                barricadeTarget = null; // ����� Barricade ���������
            }
        }

        protected virtual bool CheckForTarget()
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestTarget();
            }

            return currentTarget != null;
        }

        protected virtual void FindClosestTarget()
        {
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Customer");
            Transform closestTarget = null;
            float shortestPathDistance = Mathf.Infinity;

            foreach (GameObject targetObject in potentialTargets)
            {
                if (targetObject == null || !targetObject.activeInHierarchy)
                    continue;

                var customerStatus = targetObject.GetComponent<CustomerStatus>();
                if (customerStatus == null || customerStatus.currentState == UIState.getup)
                    continue;

                Transform target = targetObject.transform;
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(target.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathDistance = GetPathDistance(path);
                    if (pathDistance < shortestPathDistance)
                    {
                        shortestPathDistance = pathDistance;
                        closestTarget = target;
                    }
                }
            }

            if (barricadeTarget == null) // �������� Barricade ������� ��������¹��������� Customer
            {
                currentTarget = closestTarget;
            }
        }

        protected float GetPathDistance(NavMeshPath path)
        {
            float distance = 0f;
            if (path.corners.Length < 2)
                return distance;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return distance;
        }

        protected virtual void RotateTowardsTarget()
        {
            if (Vector3.Distance(transform.position, currentTarget.position) > lockDistance)
                return;

            Vector3 direction = (currentTarget.position - transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        protected virtual void StartIdle() { }

        protected virtual void StartChase() { }

        protected virtual void StartAttack() { }

        public virtual void StartStun() { }

        public virtual void StartDie() { }

        protected virtual void HandleIdle()
        {
            if (CheckForTarget())
                StartChase();
        }

        protected virtual void HandleChase()
        {
            if (CheckForTarget())
            {
                if (currentTarget != null)
                {
                    agent.SetDestination(currentTarget.position);

                    if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                        StartAttack();
                }
                else
                {
                    StartIdle();
                }
            }
        }

        protected virtual void HandleAttack()
        {
            if (isStartAttact)
                agent.SetDestination(transform.position);
        }

        protected virtual void HandleNoCustomer()
        {
            if (CheckForTarget())
                StartChase();
        }

        protected virtual IEnumerator AttackRecovery() { return null; }

        protected virtual IEnumerator StartDying()
        {
            isDead = true;
            yield return new WaitForSeconds(dyingTime);
            Destroy(gameObject);
        }
    }
}

