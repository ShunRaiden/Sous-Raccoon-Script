using SousRaccoon.Customer;
using SousRaccoon.Monster;
using System.Collections;
using UnityEngine;

public class MonsterGiverMovement : MonsterMovement
{
    [SerializeField] MonsterGiverStatus status;

    public bool canTakeDamage;

    public float waittingTime;
    public float currentWaittingTime;

    public float eattingTime;

    public float comeOutTime;

    public float hideDetectRange;
    public bool isHide;

    public enum MonsterActionState
    {
        Idle,
        Chase,
        Attack,
        Hide,
        Eat,
        ComeOut,
    }

    public MonsterActionState state;

    protected override void Start()
    {
        base.Start();
        status = GetComponent<MonsterGiverStatus>();
        currentWaittingTime = waittingTime;
        isDead = false;
        isHide = false;
        StartIdle();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        if (status.player != null && !isHide && Vector3.Distance(transform.position, status.player.transform.position) <= hideDetectRange)
        {
            isHide = true;
            StartHide();
        }

        switch (state)
        {
            case MonsterActionState.Idle:
                canTakeDamage = true;
                HandleIdle();
                break;
            case MonsterActionState.Chase:
                canTakeDamage = false;
                HandleChase();
                break;
            case MonsterActionState.Hide:
                HandleRequest();
                canTakeDamage = false;
                break;
            case MonsterActionState.Eat:
            case MonsterActionState.ComeOut:
                canTakeDamage = true;
                break;
            case MonsterActionState.Attack:
                canTakeDamage = true;
                HandleAttack();
                break;
        }
    }

    protected override void StartIdle()
    {
        if (isDead) return;

        animator.Play("Idle");
        state = MonsterActionState.Idle;
        agent.SetDestination(transform.position);

        isHide = false;
    }

    protected override void StartChase()
    {
        if (isDead || currentTarget == null) return;

        state = MonsterActionState.Chase;

        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            agent.SetDestination(currentTarget.position);
            StartAttack();
        }
        else
        {
            animator.Play("Run");
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

    public void StartHide()
    {
        if (isDead) return;

        animator.Play("Hide");
        state = MonsterActionState.Hide;
        agent.SetDestination(agent.transform.position);

        StopAllCoroutines();

        isStartAttact = false;
        currentTarget = null;
        barricadeTarget = null;
        currentWaittingTime = waittingTime + stunTime;

        StartCoroutine(Hiding());
    }

    public void StartRequesting()
    {
        if (isDead) return;

        status.GenerateIngredientRequest();
    }

    public void StartEatting()
    {
        if (isDead) return;

        animator.Play("ComeOut");
        state = MonsterActionState.Eat;
        StartCoroutine(CommingOut());
    }

    public void HandleRequest()
    {
        if (isDead || !isHide) return;

        if (currentWaittingTime > 0)
        {
            // ลดเวลาโดยอิงกับ deltaTime
            currentWaittingTime -= Time.deltaTime;

            status.waittingTimeImage.fillAmount = currentWaittingTime / waittingTime;

            // ป้องกันไม่ให้ค่าติดลบ
            if (currentWaittingTime < 0)
            {
                currentWaittingTime = 0;
            }
        }
        else
        {
            status.canTakeIGD = false;
            status.requestPanel.SetActive(false);
            animator.Play("ComeOut");
            state = MonsterActionState.ComeOut;
            StartCoroutine(CommingOut());
        }
    }

    IEnumerator Hiding()
    {
        yield return new WaitForSeconds(stunTime);
        if (!isDead)
            StartRequesting();
    }

    IEnumerator CommingOut()
    {
        yield return new WaitForSeconds(comeOutTime);

        StartCoroutine(UpdateTargetRoutine());

        if (state == MonsterActionState.Eat)
        {
            StartCoroutine(Eatting());
        }
        else
        {
            StartIdle();
        }
    }

    IEnumerator Eatting()
    {
        animator.Play("Eat");
        yield return new WaitForSeconds(eattingTime);

        Destroy(status.igdCurrent);
        status.igdCurrent = null;

        if (!isDead)
            StartIdle();
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

        yield return new WaitForSeconds(attackRecoveryTime);
        isStartAttact = false;
        StartIdle();
    }
}
