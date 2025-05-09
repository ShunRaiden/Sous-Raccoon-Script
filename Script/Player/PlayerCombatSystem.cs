using SousRaccoon.Customer;
using SousRaccoon.Lobby;
using SousRaccoon.Monster;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static SousRaccoon.Customer.CustomerStatus;

namespace SousRaccoon.Player
{
    public class PlayerCombatSystem : MonoBehaviour
    {
        [Header("Stun Rate")]
        public int stunRate;
        public int currentStunRate;
        private int stunStage;
        public float stunTimeMax;
        public float stunTimer;
        private Coroutine stunResetCoroutine;

        [Space(3)]
        [Header("Attack Direction")]
        //public float minimumDistance = .5f;
        public float coneRadiusClose = 2f;
        public float coneRadiusFar = 5f;
        public float coneAngle = 45f;
        public LayerMask targetLayer;

        [Space(3)]
        [Header("Attack Damage")]
        public int playerDamageToMonster;
        public int runStagePlayerDamageToMonster;
        [SerializeField] private int currentAttackDamage;
        public float playerDamageToCustomer;
        public int playerTimesToStun;

        [Space(3)]
        [Header("Attack Cooldown")]
        public float attackSpeed;
        public float attackSpeedRunStage;
        [SerializeField] float currentAttackSpeed;

        [Space(3)]
        [Header("Roll Dodge")]
        public bool canRollDodge = false;
        public float dodgeTime;

        [Space(3)]
        [Header("UI")]
        public GameObject gaugeStunBG;
        public Image gaugeStunIconWolrdSpace;

        public float damageDelayAnimation;
        public float attackCooldownTime = 2f; // ���� Cooldown 㹡������
        public bool isAttackOnCooldown = false; // ��Ǩ�ͺ�������㹪�ǧ Cooldown �������
        private float attackCooldownTimer = 0f; // ����ùѺ��������Ѻ Cooldown

        PlayerHealingDanceSystem playerHealingDanceSystem;
        PlayerAnimatorManager playerAnimatorManager;
        PlayerAudioManager playerAudioManager;
        PlayerInputManager playerInputManager;
        PlayerVFX playerVFX;

        private void Start()
        {
            playerHealingDanceSystem = GetComponent<PlayerHealingDanceSystem>();
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            playerAudioManager = GetComponent<PlayerAudioManager>();
            playerInputManager = GetComponent<PlayerInputManager>();
            playerVFX = GetComponent<PlayerVFX>();

            currentStunRate = stunRate;

            currentAttackSpeed = attackSpeed;
            currentAttackDamage = playerDamageToMonster;
        }

        // �ѧ��ѹ Update ����Ѻ�Ѻ���Ңͧ Cooldown
        private void Update()
        {
            if (isAttackOnCooldown)
            {
                // Ŵ���Ңͧ Cooldown ��� deltaTime
                attackCooldownTimer -= Time.deltaTime;

                // ������ҹѺ�����ѧ������� ���¡��ԡʶҹ� Cooldown
                if (attackCooldownTimer <= 0f)
                {
                    isAttackOnCooldown = false;
                }
            }

            if (playerInputManager.isInvisble && canRollDodge)
            {
                if (dodgeTime > 0)
                {
                    dodgeTime -= Time.deltaTime;
                }
                else
                {
                    playerInputManager.isInvisble = false;
                }
            }
        }

        // �ѧ��ѹ����
        public void PerformConeAttack()
        {
            playerAnimatorManager.Attack(); // Attack Animation

            Collider[] targetsInRangeFar = Physics.OverlapSphere(transform.position, coneRadiusFar, targetLayer);

            bool isHit = false;

            foreach (Collider target in targetsInRangeFar)
            {
                // �� closestPoint �������������ش� Collider
                Vector3 closestPoint = target.ClosestPoint(transform.position);

                // �ӹǳ�ǡ�����ȷҧ�ҡ����Ф���ѧ closestPoint
                Vector3 directionToTarget = (closestPoint - transform.position).normalized;

                // �ӹǳ��������ҧ˹�Ңͧ����ФáѺ��ȷҧ�ͧ�������
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                // �ӹǳ���з��١��ͧ
                float distanceToTarget = Vector3.Distance(transform.position, closestPoint);

                // ��Ǩ�ͺʶҹ��������
                if (target.CompareTag("Customer"))
                {
                    var customer = target.GetComponent<CustomerStatus>();
                    if (customer.isDead || customer.isUpset || customer.currentState == UIState.getup) continue;
                }
                else if (target.CompareTag("Enemy"))
                {
                    var monster = target.GetComponent<MonsterMovement>();

                    var coinMonst = target.GetComponent<MonsterCoinDropperStatus>();

                    if (monster != null)
                    {
                        if (monster.isDead) continue;
                    }

                    if (coinMonst != null)
                    {
                        if (coinMonst.isDead) continue;
                    }
                }

                // ��Ǩ�ͺ��������������Ѻ�������
                bool inCloseRange = distanceToTarget <= coneRadiusClose && angleToTarget <= coneAngle / 2;
                bool inFarRange = distanceToTarget <= coneRadiusFar && angleToTarget <= coneAngle / 2;

                if (inCloseRange || (inFarRange && (target.CompareTag("Enemy") || target.CompareTag("Dummy"))))
                {
                    isHit = true;
                    StartCoroutine(DelayedAttack(target));
                    if (inFarRange) Debug.Log("Far Attack");
                }
            }

            //������§੾�е͹��ⴹ
            if (isHit)
                playerAudioManager.PlayerHitBySFX();

            // �������ùѺ��������Ѻ Cooldown
            isAttackOnCooldown = true;
            attackCooldownTimer = attackCooldownTime - (currentAttackSpeed + attackSpeedRunStage);
        }

        private IEnumerator DelayedAttack(Collider target)
        {
            float elapsedTime = 0f;

            while (elapsedTime < damageDelayAnimation)
            {
                elapsedTime += Time.deltaTime;
                yield return null; // �������Ѵ�
            }

            if (target.gameObject.tag == "Customer")
            {
                var customer = target.GetComponent<CustomerStatus>();

                if (customer != null)
                {
                    customer.TakeDamageTimeCount(playerDamageToCustomer);
                }

                customer.OnTakeDamageSFX();
            }
            else if (target.gameObject.tag == "Enemy")
            {
                var monster = target.GetComponent<MonsterStatus>();
                if (monster != null)
                {
                    monster.TakeDamage(currentAttackDamage);
                }

                var coinDropper = target.GetComponent<MonsterCoinDropperStatus>();
                if (coinDropper != null)
                {
                    coinDropper.TakeDamage();
                }
            }
            else if (target.gameObject.tag == "Dummy")
            {
                var dummy = target.GetComponent<TrainingDummy>();

                if (dummy != null)
                {
                    Vector3 hitDirection = (dummy.transform.position - transform.position).normalized;
                    dummy.Hit(hitDirection);
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (playerInputManager.isStunning || playerInputManager.isInvisble) return;

            playerHealingDanceSystem.StopDanceHealing();
            currentStunRate -= damage;
            playerAnimatorManager.Stun();
            gaugeStunBG.SetActive(true);

            float rateUI = (float)(stunRate - currentStunRate) / stunRate;
            StartCoroutine(UpdateStunIconSmooth(rateUI, 0.1f));

            playerAudioManager.PlayerTakeDamageSFX();

            if (currentStunRate <= 0)
            {
                playerInputManager.isStunning = true;
                playerAudioManager.StopRunSFX();
                StartCoroutine(StunningAnimation());
            }

            // ��������������� Coroutine ����Ѻ������� stunRate
            if (stunResetCoroutine != null)
            {
                StopCoroutine(stunResetCoroutine);
            }
            stunResetCoroutine = StartCoroutine(ResetStunRateAfterDelay(5f));
        }

        private IEnumerator ResetStunRateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            currentStunRate = stunRate;
            StartCoroutine(UpdateStunIconSmooth(0f, 0.5f));
        }

        IEnumerator StunningAnimation()
        {
            playerAudioManager.PlayerStunSFX();
            playerAnimatorManager.Stunning(true); // �Դʶҹ� Stunning � Animator
            playerVFX.SetStun(true);

            stunTimer = 0f;
            float uiTime = stunTimeMax;

            playerInputManager.isStunning = true; // �Դʶҹ� Stunning �ͧ Player Input

            while (stunTimer < stunTimeMax)
            {
                stunTimer += Time.deltaTime;
                uiTime -= Time.deltaTime;
                UpdateStunIcon(uiTime / stunTimeMax);
                yield return null;
            }

            gaugeStunBG.SetActive(false);
            playerAnimatorManager.Stunning(false); // �Դʶҹ� Stunning � Animator
            playerVFX.SetStun(false);

            // �����͹����ѹ stunning ����͹
            yield return null;
            float animationLength = playerAnimatorManager.animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            stunStage = 0;
            currentStunRate = stunRate; // ���絤�� stunRate
            playerInputManager.isStunning = false; // �Դʶҹ� Stunning �ͧ Player Input
        }

        private IEnumerator UpdateStunIconSmooth(float targetRate, float duration)
        {
            float currentFill = gaugeStunIconWolrdSpace.fillAmount;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                UpdateStunIcon(Mathf.Lerp(currentFill, targetRate, elapsed / duration));
                yield return null;
            }

            if (targetRate <= 0f)
            {
                gaugeStunBG.SetActive(false);
            }

            UpdateStunIcon(targetRate);
        }

        public void UpdateStunIcon(float rate)
        {
            gaugeStunIconWolrdSpace.fillAmount = rate;
        }

        public void SetAttackSpeedBuff(float buffValue)
        {
            currentAttackSpeed = attackSpeed + buffValue;
        }

        public void ResetAttackSpeed()
        {
            currentAttackSpeed = attackSpeed;
        }

        public void SetAttackDamageBuff(int buffValue)
        {
            currentAttackDamage = playerDamageToMonster + buffValue;
        }

        public void ResetAttackDamage()
        {
            currentAttackDamage = playerDamageToMonster;
        }

        public void SetRunStageStat()
        {
            currentAttackDamage = playerDamageToMonster + runStagePlayerDamageToMonster;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, coneRadiusClose);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, coneRadiusFar);
        }
    }
}
