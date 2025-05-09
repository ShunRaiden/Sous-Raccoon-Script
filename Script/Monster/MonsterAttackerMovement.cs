using SousRaccoon.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

namespace SousRaccoon.Monster
{
    public class MonsterAttackerMovement : MonsterMovement
    {
        [SerializeField] MonsterAttackerStatus status;
        [SerializeField] float dashSpeed = 10f;
        [SerializeField] float dashDistance = 5f;
        [SerializeField] LayerMask wallLayer;

        private Vector3 dashDirection;
        private bool isDashing = false;
        private float dashTimer;
        private bool canDamage = true;

        public GameObject chargeVFX;
        public GameObject waterRush;
        public VisualEffect waterFoam;

        public enum MonsterActionState
        {
            Idle,
            Chase,
            Attack,
            Stun,
        }
        public MonsterActionState state;

        protected override void Start()
        {
            base.Start();
            status = GetComponent<MonsterAttackerStatus>();
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
            }
        }

        protected override void HandleChase()
        {
            if (CheckForTarget())
            {
                if (currentTarget != null && (currentTarget.CompareTag("Player") || currentTarget.CompareTag("Barricade")))
                {
                    agent.SetDestination(currentTarget.position);

                    float distanceToTarget = Vector3.Distance(agent.transform.position, currentTarget.position);

                    // ตรวจสอบว่าเป้าหมายอยู่ในระยะโจมตี
                    if (distanceToTarget <= attackRange)
                    {
                        // ตรวจสอบว่ามีเส้นทางตรงถึงเป้าหมายหรือไม่
                        if (!HasObstacleBetween(agent.transform.position, currentTarget.position))
                        {
                            // หยุด NavMeshAgent และเริ่มโจมตี
                            agent.ResetPath();
                            StartAttack();
                            return;
                        }
                    }
                }
                else
                {
                    StartIdle();
                }
            }
            else
            {
                StartIdle();
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

        public override void StartDie()
        {
            if (isDead) return;

            StopAllCoroutines();
            agent.ResetPath();
            agent.enabled = false;

            chargeVFX.SetActive(false);

            animator.Play("Dying");

            StartCoroutine(StartDying());
        }

        private bool HasObstacleBetween(Vector3 startPosition, Vector3 targetPosition)
        {
            NavMeshHit hit;
            bool hasObstacle = NavMesh.Raycast(startPosition, targetPosition, out hit, NavMesh.AllAreas);

            // วาดเส้นสีเขียวถ้าไม่มีสิ่งกีดขวาง และสีแดงถ้ามี
            Debug.DrawLine(startPosition, targetPosition, hasObstacle ? Color.red : Color.green, 0.1f);

            return hasObstacle;
        }

        protected override void HandleAttack()
        {
            //Not to do anything
        }

        protected override void HandleIdle()
        {
            if (CheckForTarget())
            {
                StartChase(); // ¶éÒÁÕà»éÒËÁÒÂ¡çàÃÔèÁäÅèÅèÒ
            }
        }

        protected override void RotateTowardsTarget()
        {
            if (state == MonsterActionState.Attack)
                return;

            base.RotateTowardsTarget();
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

        #region Attack

        protected override void StartAttack()
        {
            if (isStartAttact || isDead) return;

            agent.SetDestination(agent.transform.position);
            dashDirection = (currentTarget.position - transform.position).normalized;
            transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));

            StartCoroutine(StartCharging());
            state = MonsterActionState.Attack;
        }

        private IEnumerator StartCharging()
        {
            if (isDead) yield break;

            animator.Play("ChargeAttack");

            Vector3 povitRaycast = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            float adjustedDashDistance = dashDistance;
            if (Physics.Raycast(povitRaycast, dashDirection, out RaycastHit hit, dashDistance, wallLayer))
            {
                adjustedDashDistance = hit.distance;
            }

            float scaleFactor = adjustedDashDistance / dashDistance;
            waterRush.transform.localScale = new Vector3(waterRush.transform.localScale.x, waterRush.transform.localScale.y, scaleFactor);
            waterFoam.SetVector3("FoamPosition", new Vector3(0, 0, 6 * scaleFactor));
            chargeVFX.SetActive(true);

            yield return new WaitForSeconds(attackChargeTime); // รอเวลาชาร์จ

            chargeVFX.SetActive(false);
            dashTimer = 0f;
            isDashing = true;
            Dash();
        }

        private void Dash()
        {
            if (!isDashing || isDead) return;

            StartCoroutine(Dashing());
        }

        private IEnumerator Dashing()
        {
            agent.enabled = false; // ปิด NavMeshAgent ชั่วคราว
            dashTimer = 0f; // ตั้งค่าตัวจับเวลาการพุ่ง
            canDamage = true; // อนุญาตให้ทำดาเมจระหว่างการพุ่ง
            isDashing = true; // กำลังพุ่ง
            animator.Play("Attack");

            float adjustedDashDistance = dashDistance; // ระยะพุ่งที่ปรับตามสิ่งกีดขวาง
            Vector3 povitRaycast = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            // ใช้ Raycast เพื่อตรวจสอบสิ่งกีดขวางในระยะพุ่ง
            if (Physics.Raycast(povitRaycast, dashDirection, out RaycastHit hit, dashDistance, wallLayer))
            {
                // ลดระยะพุ่งหากพบสิ่งกีดขวาง
                adjustedDashDistance = hit.distance;
            }

            // พุ่งในทิศทางที่กำหนด จนกว่าจะครบระยะที่ตั้งไว้
            while (dashTimer < adjustedDashDistance / dashSpeed)
            {
                if (isDead) yield break;

                float dashStep = dashSpeed * Time.deltaTime;
                transform.position += dashDirection * dashStep; // ขยับตำแหน่ง
                dashTimer += Time.deltaTime; // อัปเดตตัวจับเวลา

                yield return null; // รอเฟรมถัดไป
            }

            // จบการพุ่ง
            isDashing = false;
            canDamage = false; // หยุดทำดาเมจ
            agent.enabled = true; // เปิด NavMeshAgent กลับมา
            animator.SetTrigger("Recovery"); // เปลี่ยนกลับไปที่แอนิเมชัน Recovery

            if (isDead) yield break;

            // เรียก Recovery หลังจากพุ่งเสร็จ
            StartCoroutine(Recovery());
        }

        private IEnumerator Recovery()
        {
            yield return new WaitForSeconds(attackRecoveryTime);
            state = MonsterActionState.Idle;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDashing && canDamage && other.CompareTag("Player"))
            {
                // ทำดาเมจใส่ Player
                var player = other.GetComponent<PlayerCombatSystem>();
                if (player != null)
                {
                    player.TakeDamage(status.monsterDamageToPlayer);
                }
                canDamage = false; // ให้ Player โดนดาเมจได้ครั้งเดียวต่อ Dash
            }

            if (isDashing && canDamage && other.CompareTag("Barricade") && barricadeTarget != null)
            {
                // ทำดาเมจใส่ Barricade
                var barricade = barricadeTarget.GetComponent<BarricadeStatus>();
                if (barricade != null)
                {
                    barricade.TakeDamage(status.monsterDamageToPlayer);
                }
                canDamage = false; // ให้ Barricade โดนดาเมจได้ครั้งเดียวต่อ Dash
            }
        }

        #endregion
    }
}
