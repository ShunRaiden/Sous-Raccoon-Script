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
            // สุ่มตำแหน่งรอบตัวในระยะ walkRadius
            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;  // เพิ่มตำแหน่งของตัวเองเพื่อให้อยู่รอบ ๆ 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                targetDirection = hit.position;
            }
            else
            {
                Debug.Log("ไม่สามารถสุ่มตำแหน่งเดินได้ ลองใหม่");
            }
        }

        IEnumerator OnWalking()
        {
            //Walk Animation 
            animator.Play("Walk");
            RandomWalk();

            while (Vector3.Distance(transform.position, targetDirection) > 0.5f)
            {
                yield return null; // รอจนกว่าจะถึงสถานี
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
