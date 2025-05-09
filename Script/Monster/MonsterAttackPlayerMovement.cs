using SousRaccoon.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SousRaccoon.Monster
{
    public class MonsterAttackPlayerMovement : MonsterMovement
    {
        [SerializeField] MonsterBasicStatus status;

        [Header("Dodge Settings")]
        public float dodgeChance = 0.3f; // 30% chance to dodge
        public float dodgeDistance = 2f; // Distance to move during dodge
        public float dodgeSpeed = 5f;    // Speed of the dodge movement

        public GameObject dodgeText;

        public bool isDodging = false;

        public enum MonsterActionState
        {
            Idle,
            Chase,
            Attack,
            Stun,
            NoCustomer,
            Dodge,
        }
        public MonsterActionState state;

        protected override void HandleChase()
        {
            // àÃÕÂ¡ãªé¿Ñ§¡ìªÑ¹ CheckForTarget ·ÕèÊ×º·Í´ÁÒ¨Ò¡ MonsterMovement
            if (CheckForTarget())
            {
                // µÃÇ¨ÊÍº currentTarget ÇèÒà»ç¹ Player ËÃ×ÍäÁè
                if (currentTarget != null && currentTarget.CompareTag("Player"))
                {
                    agent.SetDestination(currentTarget.position);

                    float distanceToTarget = Vector3.Distance(agent.transform.position, currentTarget.position);

                    if (distanceToTarget <= attackRange)
                    {
                        StartAttack();
                    }
                }
                else
                {
                    StartIdle(); // ¶éÒ currentTarget äÁèãªè Player
                }
            }
            else
            {
                StartIdle(); // ¶éÒäÁèÁÕà»éÒËÁÒÂãËéËÂØ´
            }
        }

        protected override void StartAttack()
        {
            if (isStartAttact || isDead) return;

            StartCoroutine(AttackRecovery());
        }

        public void StartDodge()
        {
            if (isDodging) return;

            isDodging = true;
            state = MonsterActionState.Dodge;

            Vector3 dodgeDirection = transform.forward; // ถอยหลัง

            // Start the dodge movement coroutine
            StartCoroutine(DodgeMovement(dodgeDirection));
        }

        protected override void HandleIdle()
        {
            if (CheckForTarget())
            {
                StartChase(); // ¶éÒÁÕà»éÒËÁÒÂ¡çàÃÔèÁäÅèÅèÒ
            }
        }

        protected override void FindClosestTarget()
        {
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");
            Transform closestTarget = null;
            float shortestPathDistance = Mathf.Infinity;

            foreach (GameObject targetObject in potentialTargets)
            {
                if (targetObject == null || !targetObject.activeInHierarchy)
                    continue; // ¢éÒÁà»éÒËÁÒÂ·Õè¶Ù¡·ÓÅÒÂËÃ×ÍäÁèà»Ô´ãªé§Ò¹

                PlayerInputManager player = targetObject.GetComponent<PlayerInputManager>();
                if (player == null || player.isStunning)
                    continue; // ¢éÒÁà»éÒËÁÒÂ·ÕèÁÕÊ¶Ò¹Ðà»ç¹ getup

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

            currentTarget = closestTarget; // µÑé§à»éÒËÁÒÂ·Õèã¡Åé·ÕèÊØ´
        }

        public bool ShouldDodge()
        {
            // Simple probability check
            return Random.value < dodgeChance;
        }

        protected override IEnumerator AttackRecovery()
        {
            isStartAttact = true;
            animator.SetTrigger("Attack");
            state = MonsterActionState.Attack;

            yield return new WaitForSeconds(attackChargeTime);

            // àªç¤ÇèÒ currentTarget ÂÑ§ÍÂÙèËÃ×ÍäÁè
            if (currentTarget != null)
            {
                var distanceToTarget = Vector3.Distance(agent.transform.position, currentTarget.position);

                var player = currentTarget.GetComponent<PlayerCombatSystem>();

                if (player != null && distanceToTarget <= attackRange + extraAttackRange)
                {
                    player.TakeDamage(status.monsterDamageToPlayer);
                }
            }

            yield return new WaitForSeconds(attackRecoveryTime);

            isStartAttact = false;

            PlayerInputManager playerStunCheck = null;
            if (currentTarget != null)
                playerStunCheck = currentTarget.GetComponent<PlayerInputManager>();

            // ËÅÑ§¨Ò¡â¨ÁµÕàÊÃç¨ãËéàªç¤ÇèÒÅÙ¡¤éÒ¶Ù¡·ÓÅÒÂËÃ×ÍäÁè
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || playerStunCheck.isStunning)
            {
                PlayerInputManager currentPlayer = currentTarget.GetComponent<PlayerInputManager>();

                FindClosestTarget(); // ¤é¹ËÒà»éÒËÁÒÂãËÁè

                if (currentTarget == null || currentPlayer.isStunning)
                {
                    StartIdle(); // ¶éÒäÁèÁÕà»éÒËÁÒÂãËÁèãËéàÃÔèÁ Idle
                }
                else
                {
                    StartChase(); // ¶éÒÁÕà»éÒËÁÒÂãËÁèãËéàÃÔèÁ Chase
                }
            }
            else
            {
                StartChase(); // ¶éÒÂÑ§ÁÕà»éÒËÁÒÂÍÂÙèãËéäÅèÅèÒµèÍ
            }
        }

        private IEnumerator DodgeMovement(Vector3 dodgeDirection)
        {
            // Temporarily disable NavMeshAgent to allow manual movement
            agent.enabled = false;
            dodgeText.SetActive(true);

            Vector3 dodgeTarget = transform.position + dodgeDirection * dodgeDistance;
            float dodgeStartTime = Time.time;

            while (Time.time < dodgeStartTime + dodgeDistance / dodgeSpeed)
            {
                transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, dodgeSpeed * Time.deltaTime);
                yield return null;
            }

            animator.Play("Idle");

            yield return new WaitForSeconds(0.5f);

            // Re-enable the agent and return to the previous state
            dodgeText.SetActive(false);
            agent.enabled = true;
            isDodging = false;
            StartChase(); // Return to chase or other state as needed
        }
    }
}
