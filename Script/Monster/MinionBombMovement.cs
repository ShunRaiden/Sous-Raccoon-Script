using SousRaccoon.Customer;
using SousRaccoon.Manager;
using SousRaccoon.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SousRaccoon.Monster
{
    public class MinionBombMovement : MinionBaseMovement
    {
        [SerializeField] MinionBombStatus status;
        public enum MonsterActionState
        {
            Spawn,
            RunAway,
            Chase,
            Attack,
        }
        public MonsterActionState state;
        public GameObject bombCharge;
        public GameObject bombExplode;
        public GameObject model;

        protected override void Start()
        {
            base.Start();
            status = GetComponent<MinionBombStatus>();
            isDead = false;
            state = MonsterActionState.Spawn;
            StartSpawn();
        }

        protected override void Update()
        {
            base.Update();

            if (isDead) return;

            switch (state)
            {
                case MonsterActionState.RunAway:
                    HandleRunAway();
                    break;
                case MonsterActionState.Chase:
                    HandleChase();
                    break;
                case MonsterActionState.Attack:
                    HandleAttack();
                    break;
            }
        }

        protected override void StartRunAway()
        {
            if (isDead) return;

            animator.Play("Run");
            state = MonsterActionState.RunAway;
            currenRunAwayPos = status.RandomMove();
            agent.SetDestination(currenRunAwayPos);
        }

        protected override void StartChase()
        {
            if (isDead || currentTarget == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > attackRange)
                animator.Play("Run");

            state = MonsterActionState.Chase;
        }

        protected override void StartAttack()
        {
            if (isStartAttact || isDead) return;

            StartCoroutine(AttackRecovery());
        }

        private void HandleRunAway()
        {
            if (CheckForTarget())
            {
                StartChase();
                return;
            }

            // เพิ่มเวลานับถอยหลัง
            runAwayTimer += Time.deltaTime;

            float distanceToTarget = Vector3.Distance(transform.position, currenRunAwayPos);

            if (distanceToTarget <= agent.stoppingDistance || runAwayTimer >= runAwayInterval)
            {
                currenRunAwayPos = status.RandomMove();
                agent.SetDestination(currenRunAwayPos);
                runAwayTimer = 0f; // รีเซ็ต Timer
            }
        }

        protected override void HandleChase()
        {
            if (CheckForTarget())
            {
                if (currentTarget != null)
                {
                    var player = currentTarget.GetComponent<PlayerInputManager>();
                    if (player != null && player.isStunning)
                    {
                        StartRunAway();
                    }
                    else
                    {
                        agent.SetDestination(currentTarget.position);

                        if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                            StartAttack();
                    }
                }
                else
                {
                    StartRunAway();
                }
            }
        }

        protected override IEnumerator AttackRecovery()
        {
            isStartAttact = true;
            bombCharge.SetActive(true);
            animator.SetTrigger("Attack");
            state = MonsterActionState.Attack;

            AudioManager.instance.PlayStageSFXOneShot("Minion_bomb_scream");

            yield return new WaitForSeconds(attackChargeTime);

            // หาทุกสิ่งที่อยู่ในระยะระเบิด
            Collider[] hitObjects = Physics.OverlapSphere(transform.position, attackRange + extraAttackRange);

            foreach (Collider hit in hitObjects)
            {
                if (hit.CompareTag("Player"))
                {
                    var player = hit.GetComponent<PlayerCombatSystem>();
                    if (player != null)
                    {
                        player.TakeDamage(status.monsterDamageToPlayer);
                    }
                }
                else if (hit.CompareTag("Customer"))
                {
                    var customer = hit.GetComponent<CustomerStatus>();
                    if (customer != null)
                    {
                        customer.TakeDamageTimeCount(status.monsterDamageToCustomer);
                        customer.OnTakeDamageSFX();
                    }
                }
                else if (hit.CompareTag("Barricade"))
                {
                    var barricade = hit.GetComponent<BarricadeStatus>();
                    if (barricade != null)
                    {
                        barricade.TakeDamage(status.monsterDamageToPlayer);
                    }
                }
            }

            AudioManager.instance.PlayStageSFXOneShot("Minion_bomb_explosion");

            status.isDead = true;
            isStartAttact = false;
            model.SetActive(false);
            bombExplode.SetActive(true);
            Destroy(gameObject, 1.5f);
        }

        protected override void FindClosestTarget()
        {
            // สมมติว่า Player มีแค่ตัวเดียว
            GameObject playerObject = GameObject.FindWithTag("Player");

            if (playerObject == null || !playerObject.activeInHierarchy)
            {
                currentTarget = null; // ถ้าไม่มี Player หรือ Player ไม่ Active
                return;
            }

            PlayerInputManager player = playerObject.GetComponent<PlayerInputManager>();

            if (player == null || player.isStunning)
            {
                currentTarget = null; // ถ้า Player กำลังติดสถานะ Stun หรือไม่มี Component
                return;
            }

            Transform target = playerObject.transform;
            NavMeshPath path = new NavMeshPath();

            if (agent.CalculatePath(target.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                currentTarget = target; // ถ้าหาเส้นทางได้สำเร็จ
            }
            else
            {
                currentTarget = null; // ถ้าหาเส้นทางไม่ได้
            }

            if (barricadeTarget == null) // ถ้าไม่มี Barricade ให้โจมตี ค่อยเปลี่ยนเป้าหมายเป็น Customer
            {
                currentTarget = playerObject.transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attackRange + extraAttackRange);
        }
    }
}
