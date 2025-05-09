using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace SousRaccoon.Monster
{
    public class MonsterSpawnerStatus : MonsterStatus
    {
        [Header("Spawner Health UI")]
        [SerializeField] GameObject healthBarBG;
        [SerializeField] Image healthBar;
        [SerializeField] OffScreenMarker offScreenMarker;

        [Header("Spawner Settings")]
        [SerializeField] private List<GameObject> minionPrefabList;
        [SerializeField] private Transform spawnPoint; // ตำแหน่งเกิดลูกน้อง

        [Header("VFX")]
        [SerializeField] private VisualEffect waterSplash;
        [SerializeField] private float playRate;

        private Animator animator;
        private Coroutine minionSpawnCoroutine;

        public float spawnTiming;

        public bool isMinionSpawning;
        [SerializeField] int minSpawnMinionQuantity;
        [SerializeField] int maxSpawnMinionQuantity;

        [SerializeField] float minNextMinionSpawnTime;
        [SerializeField] float maxNextMinionSpawnTime;

        [SerializeField] float spawnTimePerMonster;

        [SerializeField] BoxCollider hitBox;

        [Header("Monster Running Area")]
        public Vector3 maxArea;
        public Vector3 minArea;

        bool hasOpenMouth;
        public bool hasSpawnToday;

        protected override void Start()
        {
            base.Start();
            animator = GetComponentInChildren<Animator>();
            hitBox = GetComponent<BoxCollider>();
            hitBox.enabled = false;
            hasOpenMouth = false;
            offScreenMarker.enabled = false;
            waterSplash.playRate = playRate;
        }

        public void SetSpawnerStat()
        {
            currentHealth = maxHealth;
            healthBar.fillAmount = 1;
        }

        public void StartSpawnMinion()
        {
            if (!isMinionSpawning)
                StartCoroutine(OnSpawning());
        }

        public void StopSpawnMinion()
        {
            if (!hasSpawnToday) return;

            hitBox.enabled = false;

            if (minionSpawnCoroutine != null)
            {
                StopCoroutine(minionSpawnCoroutine);
                minionSpawnCoroutine = null;
            }

            healthBarBG.SetActive(false);
            animator.Play("Dying");
            isMinionSpawning = false;
            offScreenMarker.enabled = false;
        }

        public override void TakeDamage(int damage)
        {
            if (isDead) return;

            int previousHealth = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnTakeDamageVFX();

            healthBar.fillAmount = (float)currentHealth / maxHealth; //แก้ให้เป็น float

            if (currentHealth <= 0)
            {
                healthBarBG.SetActive(false);
                StageManager.instance.UpdateMonsterDefeated();
                Die();
            }
        }

        public override void Die()
        {
            if (!isDead)
            {
                isDead = true;
                StopSpawnMinion();
            }
        }

        IEnumerator OnSpawning()
        {
            isMinionSpawning = true;
            SetSpawnerStat();
            StageManager.instance.cameraMovement.LookAtTarget(transform);
            animator.Play("Spawn");
            hasOpenMouth = true;
            isDead = false;
            AudioManager.instance.PlayStageSFXOneShot("Whale_Spwan");
            AudioManager.instance.PlayStageSFXOneShot("Whale_WaterSpawn");
            yield return new WaitForSeconds(1.25f);
            AudioManager.instance.PlayStageSFXOneShot("whale_SpawnSplash");
            yield return new WaitForSeconds(0.75f);
            StageManager.instance.cameraMovement.LookAtPlayer();
            yield return new WaitForSeconds(spawnTiming - 2.5f);
            offScreenMarker.enabled = true;
            hitBox.enabled = true;
            healthBarBG.SetActive(true);

            minionSpawnCoroutine = StartCoroutine(MinionSpawning()); //เรียก StartCoroutine
        }

        IEnumerator MinionSpawning()
        {
            while (!isDead)
            {
                //yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Customer").Length > 0);
                if (!hasOpenMouth)
                {
                    animator.Play("OpenMouth");

                    yield return new WaitForSeconds(2f); //ใช้ WaitForSeconds
                }

                int spawnCount = Random.Range(minSpawnMinionQuantity, maxSpawnMinionQuantity);

                for (int i = 0; i < spawnCount; i++)
                {
                    if (StageManager.instance.isGameWin)
                    {
                        yield break;
                    }

                    SpawnMinion(); //เรียกฟังก์ชัน Spawn

                    yield return new WaitForSeconds(spawnTimePerMonster); //ใช้ WaitForSeconds
                }

                yield return new WaitForSeconds(1f);
                hasOpenMouth = false;
                animator.Play("CloseMouth");

                float nextCycleTime = Random.Range(minNextMinionSpawnTime, maxNextMinionSpawnTime);
                yield return new WaitForSeconds(nextCycleTime); //ใช้ WaitForSeconds
            }
        }

        void SpawnMinion()
        {
            Debug.Log("Spawn Minion");
            var minionPref = minionPrefabList[Random.Range(0, minionPrefabList.Count)];
            TryGetMonsterStat(minionPref.name, out var hp, out var damageTC, out var damageTP);
            var minion = Instantiate(minionPref, spawnPoint.position, spawnPoint.rotation);

            var minionStat = minion.GetComponent<MinionBasicStatus>();

            if (minionStat != null)
                minionStat.SetStatus(hp, damageTC, damageTP, StageManager.instance.spawnerManager.healingDeadMultiply, maxArea, minArea);
        }

        private void TryGetMonsterStat(string name, out int hp, out float dmToC, out int dmToP)
        {
            hp = 0;
            dmToC = 0;
            dmToP = 0;

            foreach (var datas in StageManager.instance.spawnerManager.monsterStatData.monsterBaseStat)
            {
                if (datas.name == name)
                {
                    hp = datas.MaxHealth;
                    dmToC = datas.DamageToCustomer;
                    dmToP = datas.DamageToPlayer;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (maxArea == Vector3.zero || minArea == Vector3.zero) return;

            // กำหนดสีของ Gizmos
            Gizmos.color = Color.yellow;

            // วาดกรอบพื้นที่ที่มอนสเตอร์วิ่งได้
            Vector3 topLeft = new Vector3(minArea.x, 0, maxArea.z);
            Vector3 topRight = new Vector3(maxArea.x, 0, maxArea.z);
            Vector3 bottomLeft = new Vector3(minArea.x, 0, minArea.z);
            Vector3 bottomRight = new Vector3(maxArea.x, 0, minArea.z);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
}
