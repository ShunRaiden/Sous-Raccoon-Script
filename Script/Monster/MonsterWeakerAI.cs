using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SousRaccoon.Monster
{
    public class MonsterWeakerAI : MonsterCoinDropperStatus
    {
        [Header("Weaker Stat")]
        [SerializeField] private float idleTime;
        [SerializeField] private float dyingTime;

        [SerializeField] private float walkRadius;

        Animator animator;
        NavMeshAgent agent;
        Vector3 targetDirection;

        [Header("Coin Drop")]
        public GameObject coinDropVFX;
        public GameObject bombVFX;
        public GameObject birdModel;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();

            currentHealthTimePoint = maxHealthTimePoint;
            StartIdle();

            AudioManager.instance.PlayStageSFXOneShot("Bird_Spawn_Sound");
        }

        private void Update()
        {
            if (isDead) return;

            currentHealthTimePoint -= Time.deltaTime;

            if (currentHealthTimePoint < outOfTime && !hasOutOfTime)
            {
                //Play Fade Animation (Shader)
                Debug.LogWarning($"{nameof(gameObject)} has Fading Time");
            }

            if (currentHealthTimePoint <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void StartIdle()
        {
            Debug.LogWarning($"{nameof(gameObject)} has Idle");
            StartCoroutine(OnIdle());
        }

        public void StartWalk()
        {
            Debug.LogWarning($"{nameof(gameObject)} has Walking");
            StartCoroutine(OnWalking());
        }

        public override void TakeDamage()
        {
            base.TakeDamage();
            StopAllCoroutines();
            Die();
        }

        public override void Die()
        {
            StopAllCoroutines();
            isDead = true;
            agent.SetDestination(transform.position);
            agent.enabled = false;
            birdModel.SetActive(false);
            bombVFX.SetActive(true);
            coinDropVFX.SetActive(true);
            AudioManager.instance.PlayStageSFXOneShot("CoinDroppedSFX");
            Destroy(gameObject, 3);
        }

        public void RandomWalk()
        {
            // �������˹��ͺ�������� walkRadius
            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;  // �������˹觢ͧ����ͧ������������ͺ � 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                targetDirection = hit.position;
            }
            else
            {
                Debug.Log("�������ö�������˹��Թ�� �ͧ����");
            }
        }

        IEnumerator OnWalking()
        {
            //Walk Animation 
            animator.Play("Walk");
            RandomWalk();

            while (Vector3.Distance(transform.position, targetDirection) > 0.5f)
            {
                yield return null; // �ͨ����Ҩж֧ʶҹ�
            }

            StartIdle();
        }

        IEnumerator OnIdle()
        {
            //Idle Animation
            var num = Random.Range(0, 1);

            if (num == 0)
                animator.Play("Idle1");
            else
                animator.Play("Idle2");

            yield return new WaitForSeconds(idleTime);

            StartWalk();
        }
    }
}
