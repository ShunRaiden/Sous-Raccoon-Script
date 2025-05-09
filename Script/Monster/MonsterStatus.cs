using SousRaccoon.Customer;
using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace SousRaccoon.Monster
{
    public abstract class MonsterStatus : MonoBehaviour
    {
        [Header("Stats")]
        public bool isDead;
        public int maxHealth;
        public int currentHealth;
        public float monsterDamageToCustomer; // ความเสียหายที่มอนสเตอร์ทำต่อ Customer
        public int monsterDamageToPlayer;    // ความเสียหายที่มอนสเตอร์ทำต่อ Player
        public float healRadius;// รัศมีในการฮีล
        [SerializeField] protected float healAmount;// ปริมาณการฮีล

        [Header("Health UI")]
        public List<GameObject> healthIconIndex = new();
        public GameObject healthIconPref;
        public Transform container;

        [Header("VFX")]
        [SerializeField] protected GameObject deadVFXBomb;
        [SerializeField] protected GameObject deadVFXBubble;
        [SerializeField] protected VisualEffect takeDamageVFX;
        [SerializeField] protected SkinnedMeshRenderer baseMeshRenderer;
        [SerializeField] protected Material baseMat;
        [SerializeField] protected Material takeDamageMat;
        [SerializeField] protected float matDuration;

        protected Coroutine takeDamageCoroutine;

        protected virtual void Start()
        {
            StageManager.instance.EventOnGameEnd += Die;
        }

        private void OnDestroy()
        {
            StageManager.instance.EventOnGameEnd -= Die;
        }

        public virtual void SetStatus(int setMaxHealth,
                              float setMonsterDamageToCustomer,
                              int setMonsterDamageToPlayer,
                              float healingDeadMultiply)
        {
            maxHealth = setMaxHealth;
            healAmount = maxHealth * healingDeadMultiply;
            monsterDamageToCustomer = setMonsterDamageToCustomer;
            monsterDamageToPlayer = setMonsterDamageToPlayer;

            InitializeHealthIcons();
        }

        protected void InitializeHealthIcons()
        {
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject heartIcon = Instantiate(healthIconPref, container);
                healthIconIndex.Add(heartIcon);
            }
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            int previousHealth = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnTakeDamageVFX();

            for (int i = previousHealth - 1; i >= currentHealth; i--)
            {
                if (i >= 0 && i < healthIconIndex.Count)
                {
                    healthIconIndex[i].SetActive(false);
                }
            }

            if (currentHealth <= 0)
            {
                StageManager.instance.UpdateMonsterDefeated();
                Die();
            }
        }

        public void OnTakeDamageVFX()
        {
            if (takeDamageCoroutine != null)
            {
                StopCoroutine(takeDamageCoroutine);
            }

            // เริ่ม Coroutine ใหม่และเก็บ reference ไว้
            if (takeDamageMat != null)
                takeDamageCoroutine = StartCoroutine(TakeDamageEffect());

            takeDamageVFX.gameObject.SetActive(true);
            takeDamageVFX.Stop();
            takeDamageVFX.Play();
        }

        IEnumerator TakeDamageEffect()
        {
            baseMeshRenderer.material = takeDamageMat;
            yield return new WaitForSeconds(matDuration);
            baseMeshRenderer.material = baseMat;
        }

        public virtual void Die()
        {
            isDead = true;
            HealingDead();
            StartCoroutine(OnDyingVFX());
        }

        protected virtual void HealingDead()
        {
            // ค้นหา Collider ที่อยู่ในรัศมี
            Collider[] colliders = Physics.OverlapSphere(transform.position, healRadius);

            bool isheal = false;

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Customer"))
                {
                    // เรียกฟังก์ชัน Heal ใน Object เป้าหมาย
                    CustomerStatus customerHealth = collider.GetComponent<CustomerStatus>();

                    if (customerHealth != null)
                    {
                        customerHealth.Heal(healAmount);
                        isheal = true;
                    }
                }
            }

            if (isheal)
                AudioManager.instance.PlayStageSFXOneShot("Player_Heal_Detect_2");
        }

        protected IEnumerator OnDyingVFX()
        {
            yield return new WaitForSeconds(0.9f);

            if (deadVFXBomb != null)
                deadVFXBomb.SetActive(true);

            yield return new WaitForSeconds(0.15f);

            if (deadVFXBubble != null)
                deadVFXBubble.SetActive(true);
        }

        void OnDrawGizmosSelected()
        {
            // แสดงรัศมีการฮีลใน Scene View
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healRadius);
        }
    }
}
