using SousRaccoon.Customer;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static SousRaccoon.Customer.CustomerStatus;

namespace SousRaccoon.Monster
{
    public class MonsterHidingMovement : MonsterMovement
    {
        [SerializeField] MonsterHidingStatus status;

        public bool canTakeDamage;

        public enum MonsterActionState
        {
            Spawn,
            Idle,
            Chase,
            Attack,
            Stun,
        }
        public MonsterActionState state;

        protected override void Start()
        {
            base.Start();
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            status = GetComponent<MonsterHidingStatus>();
            isDead = false;
            StartSpawn();
        }

        protected override void Update()
        {
            base.Update();

            if (isDead) return;

            switch (state)
            {
                case MonsterActionState.Spawn:
                    canTakeDamage = false;
                    break;
                case MonsterActionState.Idle:
                    canTakeDamage = true;
                    HandleIdle();
                    break;
                case MonsterActionState.Chase:
                    canTakeDamage = false;
                    HandleChase();
                    break;
                case MonsterActionState.Stun:
                    // หลัง Stun ให้ state ถูกอัปเดตผ่าน Coroutine (Stuning)
                    break;
                case MonsterActionState.Attack:
                    canTakeDamage = true;
                    HandleAttack();
                    break;
            }
        }

        protected override IEnumerator UpdateTargetRoutine()
        {
            yield return null;
        }

        protected override void StartIdle()
        {
            if (isDead) return;

            animator.Play("Idle");
            state = MonsterActionState.Idle;
            agent.SetDestination(transform.position);
        }

        public void StartSpawn()
        {
            state = MonsterActionState.Spawn;
            StartCoroutine(Spawning());
        }

        protected override void StartChase()
        {
            if (isDead || currentTarget == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > attackRange)
            {
                animator.Play("Run");
                agent.SetDestination(currentTarget.position);
                state = MonsterActionState.Chase;
            }
            else
            {
                StartAttack();
            }
        }

        protected override void StartAttack()
        {
            if (isStartAttact || isDead) return;

            StartCoroutine(AttackRecovery());
        }

        public override void StartDie()
        {
            if (isDead) return;

            StopAllCoroutines();
            agent.ResetPath();
            agent.enabled = false;

            animator.Play("Dying");
            StartCoroutine(StartDying());
        }

        public override void StartStun()
        {
            if (isDead) return;

            isStartAttact = false;
            animator.Play("Stun");
            state = MonsterActionState.Stun;
            FindClosestTarget();

            StopAllCoroutines();
            StartCoroutine(Stuning());
        }

        protected override void HandleIdle()
        {
            if (CheckForTarget())
            {
                StartChase();
            }
        }

        IEnumerator Stuning()
        {
            yield return new WaitForSeconds(0.8f);
            canTakeDamage = false;
            yield return new WaitForSeconds(stunTime - 0.8f);

            if (!isDead)
            {
                state = MonsterActionState.Idle;
                StartIdle();
            }
        }

        IEnumerator Spawning()
        {
            animator.Play("Spawn");
            yield return new WaitForSeconds(1f);

            if (CheckForTarget())
            {
                if (currentTarget != null)
                {
                    StartChase();
                }
                else
                {
                    StartIdle();
                }
            }
            else
            {
                StartIdle();
            }
        }

        protected override IEnumerator AttackRecovery()
        {
            isStartAttact = true;
            animator.SetTrigger("Attack");
            state = MonsterActionState.Attack;
            Debug.LogError("State Attack");

            yield return new WaitForSeconds(attackChargeTime);

            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                var customer = currentTarget.GetComponent<CustomerStatus>();

                if (customer != null && distanceToTarget <= attackRange + extraAttackRange)
                {
                    customer.TakeDamageTimeCount(status.monsterDamageToCustomer); // Replace with appropriate damage value
                    customer.OnTakeDamageSFX();
                }
            }

            yield return new WaitForSeconds(1.8f);
            canTakeDamage = false;
            yield return new WaitForSeconds(attackRecoveryTime - 1.8f);

            // บังคับให้หาเป้าหมายใหม่หลังจากโจมตี
            FindClosestTarget();

            isStartAttact = false;
            StartIdle();
        }

        protected override bool CheckForTarget()
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestTarget();
            }

            Debug.Log($"Current Target: {currentTarget}");
            return currentTarget != null;
        }

        protected override void FindClosestTarget()
        {
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Customer");
            Transform closestTarget = null;
            Transform closestDifferentTarget = null;
            float shortestPathDistance = Mathf.Infinity;
            float shortestDifferentPathDistance = Mathf.Infinity;

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

                    // Condition 1: Closest target (if no current target)
                    if (pathDistance < shortestPathDistance)
                    {
                        shortestPathDistance = pathDistance;
                        closestTarget = target;
                    }

                    // Condition 2: Closest target that is not the current target
                    if (currentTarget != null && target != currentTarget && pathDistance < shortestDifferentPathDistance)
                    {
                        shortestDifferentPathDistance = pathDistance;
                        closestDifferentTarget = target;
                    }
                }
            }

            // Prioritize targets based on conditions
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = closestTarget; // Condition 1
            }
            else if (closestDifferentTarget != null)
            {
                currentTarget = closestDifferentTarget; // Condition 2
            }
        }
    }
}
