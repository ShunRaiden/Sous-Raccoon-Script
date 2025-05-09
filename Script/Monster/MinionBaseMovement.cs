using System.Collections;
using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MinionBaseMovement : MonsterMovement
    {
        [SerializeField] float spawnTiming;
        [SerializeField] Collider baseCollider;

        protected float runAwayTimer = 0f;
        [SerializeField] protected float runAwayInterval = 2f; // ตั้งค่าให้สุ่มใหม่ทุก 2 วินาที
        protected Vector3 currenRunAwayPos;

        protected bool finishSpawn = false;

        protected override void Update()
        {
            if (currentTarget != null && finishSpawn && !isDead)
                RotateTowardsTarget();
        }

        protected virtual void StartRunAway()
        {

        }

        protected void StartSpawn()
        {
            animator.Play("Spawn");
            agent.SetDestination(transform.position);
            StartCoroutine(OnSpawning());
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

        IEnumerator OnSpawning()
        {
            yield return new WaitForSeconds(spawnTiming);
            finishSpawn = true;
            baseCollider.enabled = true;
            StartRunAway();
        }
    }
}