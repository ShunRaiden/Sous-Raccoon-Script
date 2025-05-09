using SousRaccoon.Customer;
using System.Collections;
using UnityEngine;


namespace SousRaccoon.Monster
{
    public class MinionBasicMovement : MinionBaseMovement
    {
        [SerializeField] MinionBasicStatus status;

        public enum MonsterActionState
        {
            Spawn,
            RunAway,
            Chase,
            Attack,
        }
        public MonsterActionState state;

        protected override void Start()
        {
            base.Start();
            status = GetComponent<MinionBasicStatus>();
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
                    agent.SetDestination(currentTarget.position);

                    if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                        StartAttack();
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
            animator.SetTrigger("Attack");
            state = MonsterActionState.Attack;

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

            if (barricadeTarget != null)
            {
                var barricade = barricadeTarget.GetComponent<BarricadeStatus>();
                if (barricade != null)
                {
                    barricade.TakeDamage(status.monsterDamageToPlayer);
                }
            }

            //AudioManager.instance.PlayStageSFXOneShot("Minion_Basic_Attack");

            yield return new WaitForSeconds(attackRecoveryTime);
            isStartAttact = false;
            StartRunAway();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
