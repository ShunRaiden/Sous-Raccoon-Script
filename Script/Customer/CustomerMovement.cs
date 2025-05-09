using SousRaccoon.Kitchen;
using SousRaccoon.Manager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static SousRaccoon.Customer.CustomerStatus;

namespace SousRaccoon.Customer
{
    public class CustomerMovement : MonoBehaviour
    {
        public const float THINK_TIME_MIN = 2;
        public const float THINK_TIME_MAX = 4;
        public const float EATING_TIME = 7.5f;

        NavMeshAgent agent;
        StageManager stateManager;
        CustomerStatus status;
        Animator animator;
        Vector3 targetDestination;

        [SerializeField] public ActionState state;
        [SerializeField] float moveSpeed;
        [SerializeField] float hurtBuffSpeed;
        [SerializeField] float rotationSpeed;
        [SerializeField] float lockDistance;

        [SerializeField] float hurtBuffDuration;
        private Coroutine currentHurtBuffCoroutine;

        [SerializeField] Chair currentChair;
        [SerializeField] Table currentTable;
        private static object chairLock = new object();

        [SerializeField] GameObject eatProp;
        [SerializeField] GameObject orderProp;
        [SerializeField] Animator orderAnim;
        [SerializeField] bool fixSittingAnim;
        [SerializeField] float newTimeWaiting;

        [Header("Money")]
        [SerializeField] GameObject moneyDropUI;
        [SerializeField] TMP_Text moneyText;

        [HideInInspector] public bool isOverThinking = false;

        bool isSit = false; //Cheack Chair For Trash

        public enum ActionState
        {
            Idle,
            Walk,
            Sit,
            Menu,
            Wait,
            Eat,
            Getup,
            Die,
            Upset,
        }

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            status = GetComponent<CustomerStatus>();
            animator = GetComponentInChildren<Animator>();
            stateManager = FindAnyObjectByType<StageManager>();

            agent.speed = moveSpeed;

            // ในการจองเก้าอี้ใน Start()
            lock (chairLock)
            {
                currentChair = stateManager.GetAvailableChair(out var table);

                if (currentChair == null || stateManager.currentCustomerDieCount > 0)
                    StartIdle();
                else
                {
                    currentTable = table;
                    currentTable.OccupyChair(currentChair); // จองเก้าอี้
                    StartWalking(currentChair.walkInPositon.position);
                }
            }

            status.EnterUIState(UIState.normal);

        }

        // Update is called once per frame
        void Update()
        {
            if (state == ActionState.Walk)
            {
                RotateTowardsTarget();
            }

            switch (state)
            {
                case ActionState.Idle:
                    HandleIdle();
                    break;
                case ActionState.Walk:
                    HandleWalking();
                    break;
                case ActionState.Sit:
                    HandleSit();
                    break;
                case ActionState.Menu:
                    HandleMenu();
                    break;
                case ActionState.Wait:
                    HandleWait();
                    break;
                case ActionState.Eat:
                    HandleEat();
                    break;
                case ActionState.Getup:
                    HandleGetUp();
                    break;
                case ActionState.Die:
                    break;
                case ActionState.Upset:
                    HandleUpset();
                    break;
            }
        }

        private void RotateTowardsTarget()
        {
            // ตรวจสอบระยะห่างระหว่างตำแหน่งปัจจุบันและ targetDestination
            if (Vector3.Distance(transform.position, targetDestination) > lockDistance)
            {
                return;
            }

            Vector3 direction = (targetDestination - transform.position).normalized;

            // ล็อกให้หมุนเฉพาะแกน Y โดยตั้งค่า X และ Z ให้เป็น 0
            direction.y = 0f; // ทำให้ทิศทางเป็น 2D (แกน X และ Z เท่านั้น)

            if (direction != Vector3.zero)
            {
                // คำนวณการหมุนเฉพาะแกน Y
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // หมุนตัวละครอย่างนุ่มนวลด้วย Slerp
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }


        public void StartIdle()
        {
            agent.SetDestination(stateManager.customerWaitPos.position);
            state = ActionState.Idle; // Set state to Walk
            animator.SetBool("isIdle", false);

            status.EnterUIState(UIState.normal);
            status.StartHealthTimeCount();

            if (currentTable != null && currentChair != null)
            {
                currentTable.FreeChair(currentChair);
                currentChair.IsSitting = false;
                currentChair.customerStatus = null;
                currentChair.CustomerMovement = null;
            }

            currentChair = null;
            currentTable = null;
        }

        public void StartWalking(Vector3 destination)
        {
            status.EnterUIState(UIState.normal);

            targetDestination = destination;
            agent.SetDestination(targetDestination);
            state = ActionState.Walk; // Set state to Walk

            animator.SetBool("isIdle", false);
        }

        public void StartSit(Transform chairLocation)
        {
            status.EnterUIState(UIState.normal);

            agent.enabled = false;
            currentChair.IsSitting = true;
            currentChair.customerStatus = status;
            currentChair.CustomerMovement = this;

            transform.position = chairLocation.position;
            transform.rotation = chairLocation.rotation;

            StartCoroutine(StartSitting());
            isSit = true;
            state = ActionState.Sit;
        }

        public void StartMenu()
        {
            status.EnterUIState(UIState.thinking);

            if (isOverThinking) return;

            StartCoroutine(StartThinkMenu());
        }

        public void StartWait()
        {
            animator.Play("Wait");
            orderProp.SetActive(false);

            //Still Count Down Time
            status.EnterUIState(UIState.wait);
            state = ActionState.Wait;
        }

        public void StartEat()
        {
            //TODO : Stop  Count Down Time
            status.EnterUIState(UIState.getup);
            StartCoroutine(StartEatingFood());
        }

        public void StartGetUp()
        {
            status.EnterUIState(UIState.getup);

            StartCoroutine(StartGettingUp());
        }

        public void StartDead()
        {
            agent.enabled = false;

            if (eatProp != null)
                eatProp.SetActive(false);

            orderProp.SetActive(false);

            //TODO : Delete It if Have new Vignette
            StageManager.instance.hurtVignetteAnim.Play("HurtVignette");
            //
            StageManager.instance.UpdateWinRate();

            StopAllCoroutines();

            if (currentChair != null && currentTable != null)
            {
                currentTable.FreeChair(currentChair);
                string currentChairName = currentChair.chairName;
                currentTable.customerMenuDics.Remove(currentChairName);
                currentChair.IsSitting = false;
                currentChair.customerStatus = null;
                currentChair.CustomerMovement = null;
            }

            StartCoroutine(StartDying());
        }

        public void StartUpset()
        {
            StageManager.instance.hurtVignetteAnim.Play("HurtVignette");

            StageManager.instance.UpdateWinRate();

            if (currentChair != null && currentTable != null)
            {
                transform.position = currentChair.chairPosition.position;
                transform.rotation = currentChair.chairPosition.rotation;

                currentTable.FreeChair(currentChair);
                string currentChairName = currentChair.chairName;
                currentTable.customerMenuDics.Remove(currentChairName);
                currentChair.IsSitting = false;
                currentChair.customerStatus = null;
                currentChair.CustomerMovement = null;
            }

            StartCoroutine(StartGettingUpset());
        }

        // ปรับปรุงการตรวจสอบใน HandleWalking()
        private void HandleWalking()
        {
            if (stateManager.currentCustomerDieCount > 0 || currentChair == null)
            {
                StartIdle();
                return;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // ตรวจสอบว่า Agent อยู่ใกล้เก้าอี้ที่ถูกต้องหรือไม่ก่อนเข้าสู่ Sit
                    float distanceToChair = Vector3.Distance(transform.position, currentChair.chairPosition.position);
                    if (distanceToChair < 1f) // ปรับค่า threshold ตามต้องการ
                    {
                        StartSit(currentChair.chairPosition.transform);
                    }
                    else
                    {
                        // หากไม่ได้อยู่ใกล้เก้าอี้ ให้หาเก้าอี้ใหม่หรือลองเดินใหม่
                        StartWalking(currentChair.walkInPositon.position);
                    }
                }
            }
        }

        private void HandleIdle()
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                animator.SetBool("isIdle", true);
            }

            status.isWaiting = true;

            if (stateManager.currentCustomerDieCount > 0) return; //if someone dies, don't find Chair

            // ล็อกการจองเก้าอี้ เพื่อให้การจองเกิดขึ้นเพียงคนเดียวในเวลาเดียวกัน
            lock (chairLock)
            {
                currentChair = stateManager.GetAvailableChair(out var table);

                if (currentChair != null)
                {
                    currentTable = table;
                    currentTable.OccupyChair(currentChair);  // จองเก้าอี้
                    StartWalking(currentChair.walkInPositon.position);  // เดินไปยังตำแหน่งที่จอง
                    status.StopHealthTimeCount();
                    status.isWaiting = false;
                    status.UpdateHealthBar();
                }
            }
        }

        private void HandleSit()
        {

        }

        private void HandleMenu()
        {
            //Continue Count Time
            if (currentChair.IsTakeMenu && status.canGiveOrder)
                StartWait(); // Change UI to Wait Food
        }

        private void HandleWait()
        {
            if (status.htState == 2)
                animator.SetBool("LongWait", true);
            else if (status.htState == 0)
                animator.SetBool("LongWait", false);

            if (currentChair.IsGetFood)
            {
                StartEat();
            }
        }

        private void HandleEat()
        {

        }

        private void HandleGetUp()
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Destroy(gameObject);
            }
        }

        private void HandleUpset()
        {
            if (!agent.enabled) return;

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator StartThinkMenu()
        {
            // เปลี่ยนสถานะเป็น Menu
            state = ActionState.Menu;

            animator.Play("StartOrder");

            orderProp.SetActive(true);

            if (orderAnim != null)
                orderAnim.SetBool("isFinishOrder", false);

            yield return new WaitForSeconds(Random.Range(THINK_TIME_MIN, THINK_TIME_MAX));

            animator.SetBool("isFinishOrder", true);

            if (orderAnim != null)
                orderAnim.SetBool("isFinishOrder", true);

            yield return new WaitForSeconds(1f);

            animator.SetBool("isOrderLoop", true);

            if (orderAnim != null)
                orderAnim.SetBool("isOrderLoop", true);

            // ใช้ chairName เป็น Key ใน Dictionary
            string currentChairName = currentChair.chairName;

            // ตรวจสอบว่ามี Key (chairName) อยู่ใน customerMenuDics แล้วหรือไม่
            if (currentTable.customerMenuDics.ContainsKey(currentChairName))
            {
                Debug.LogWarning($"มีเมนูอยู่ใน {currentChairName} แล้ว");
            }
            else
            {
                // ถ้าไม่มี ก็ทำการเพิ่ม ItemDataBase เข้าไปใน Dictionary
                var menu = stateManager.GenerateMenu();
                currentTable.customerMenuDics.Add(currentChairName, menu);
                status.SetIcon(menu.order.FoodType.FoodIcon, menu.order.type == Data.Item.OrderSO.OrderType.Food);
                status.EnterUIState(UIState.order);
                status.canGiveOrder = true;
            }
        }

        private IEnumerator StartEatingFood()
        {
            state = ActionState.Eat;
            status.StopHealthTimeCount();
            animator.Play("Eat");

            if (eatProp != null)
                eatProp.SetActive(true);

            StageManager.instance.UpdateFoodDeliver();
            //TODO : Food++ On Cusotmer Head

            yield return new WaitForSeconds(EATING_TIME);

            if (eatProp != null)
                eatProp.SetActive(false);

            currentChair.DestroyFood();
            StartGetUp();
        }

        private IEnumerator StartDying()
        {
            animator.Play("Die");

            //float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(1.5f);

            AudioManager.instance.PlayStageSFXOneShot("Customer_Dying");
            var deadTransform = gameObject.transform;
            var dead = Instantiate(status.deadPref, deadTransform.transform.position, deadTransform.transform.rotation); // Spawn Dead Customer at Transform
            if (isSit && dead != null)
                dead.GetComponent<CustomerDead>().isChair = true;

            Destroy(gameObject);
        }

        private IEnumerator StartSitting()
        {
            float waitTime = 3f; // เวลาในการรอ 2 วินาที
            float elapsedTime = 0f; // ตัวนับเวลาที่ผ่านไป

            switch (currentChair.chairNumber)
            {
                case 0:
                case 3:
                    animator.Play("Sitting_R");
                    break;
                case 1:
                case 2:
                    animator.Play("Sitting_L");
                    break;
            }

            if (fixSittingAnim)
            {
                waitTime = newTimeWaiting;
            }

            // วนลูปจนกว่าเวลาที่รอจะครบ 2 วินาที
            while (elapsedTime < waitTime)
            {
                elapsedTime += Time.deltaTime; // เพิ่มเวลาที่ผ่านไปแต่ละเฟรม
                yield return null; // รอในแต่ละเฟรม
            }

            StartMenu();
        }

        private IEnumerator StartGettingUp()
        {
            switch (currentChair.chairNumber)
            {
                case 0:
                case 3:
                    animator.Play("GetUp_R");
                    break;
                case 1:
                case 2:
                    animator.Play("GetUp_L");
                    break;
            }

            yield return null;
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            var moneyReward = status.CalculateReward();

            StageManager.instance.AddMoney(moneyReward);

            AudioManager.instance.PlayStageSFXOneShot("CashMoney");

            StageManager.instance.UpdateWinRate();

            moneyText.text = $"{moneyReward}";
            moneyDropUI.SetActive(true);

            transform.position = currentChair.chairPosition.position;
            transform.rotation = currentChair.chairPosition.rotation;

            currentTable.FreeChair(currentChair);
            currentChair.IsSitting = false;
            currentChair.customerStatus = null;
            currentChair.CustomerMovement = null;

            agent.enabled = true;
            agent.SetDestination(stateManager.customerEndPoint.position);
            state = ActionState.Getup;

            animator.SetBool("isIdle", false);
            animator.Play("Walk");
        }

        private IEnumerator StartGettingUpset()
        {
            state = ActionState.Upset;

            if (eatProp != null)
                eatProp.SetActive(false);

            orderProp.SetActive(false);

            switch (currentChair.chairNumber)
            {
                case 0:
                case 3:
                    animator.Play("GetUp_R");
                    break;
                case 1:
                case 2:
                    animator.Play("GetUp_L");
                    break;
            }

            yield return null;
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            agent.enabled = true;
            agent.SetDestination(stateManager.customerEndPoint.position);
            animator.Play("Walk_UpSet");
        }

        public void PlayHurtAnimation()
        {
            if (state == ActionState.Idle || state == ActionState.Walk)
            {

                animator.Play("Hurt_Stand");
                ApplyHurtSpeedBuff();
            }
            else
            {
                animator.Play("Hurt_Sit");
            }
        }

        public void ApplyHurtSpeedBuff()
        {
            // ถ้ามีบัพเก่าให้ยกเลิกก่อน
            if (currentHurtBuffCoroutine != null)
            {
                StopCoroutine(currentHurtBuffCoroutine);
            }

            currentHurtBuffCoroutine = StartCoroutine(SpeedBuffCoroutine());
        }

        private IEnumerator SpeedBuffCoroutine()
        {
            float elapsedTime = 0f; // เวลาเริ่มต้น
            agent.speed = hurtBuffSpeed; // เพิ่มความเร็ว

            while (elapsedTime < hurtBuffDuration)
            {
                elapsedTime += Time.deltaTime; // เพิ่มเวลาโดยใช้ deltaTime
                yield return null; // รอเฟรมถัดไป
            }

            agent.speed = moveSpeed; // คืนค่าความเร็วปกติ
            currentHurtBuffCoroutine = null; // รีเซ็ต Coroutine
        }
    }
}