using SousRaccoon.Customer;
using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MonsterBasicMovement : MonsterMovement
    {
        [SerializeField] MonsterBasicStatus status;

        public enum MonsterActionState
        {
            Idle,
            Chase,
            Attack,
            Stun,
            NoCustomer,
        }
        public MonsterActionState state;

        protected override void Start()
        {
            base.Start();
            status = GetComponent<MonsterBasicStatus>();
            targetPoint = StageManager.instance.spawnerManager.monsterFarSpawnPoint;
            isDead = false;
            StartIdle();
        }

        protected override void Update()
        {
            base.Update();

            if (isDead) return;

            switch (state)
            {
                case MonsterActionState.Idle:
                    HandleIdle();
                    break;
                case MonsterActionState.Chase:
                    HandleChase();
                    break;
                case MonsterActionState.Stun:
                    break;
                case MonsterActionState.Attack:
                    HandleAttack();
                    break;
                case MonsterActionState.NoCustomer:
                    HandleNoCustomer();
                    break;
            }
        }

        protected override void StartIdle()
        {
            if (isDead) return;

            animator.Play("Idle");
            state = MonsterActionState.Idle;
            agent.SetDestination(transform.position);
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
            agent.SetDestination(agent.transform.position);

            StopAllCoroutines();
            StartCoroutine(Stuning());
        }

        IEnumerator Stuning()
        {
            yield return new WaitForSeconds(stunTime);
            if (!isDead)
                StartIdle();
        }

        //HandleIdle()
        //HandleChase()
        //HandleAttack()
        //HandleNoCustomer()

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

            yield return new WaitForSeconds(attackRecoveryTime);
            isStartAttact = false;
            StartIdle();
        }
        //StartDying()
    }
}
