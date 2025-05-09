using SousRaccoon.Data.Item;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class HelperGiverAI : MonoBehaviour
    {
        [SerializeField] ChefAI chefAI;

        public int levelGiver = 0;

        public float currentItemPerTime;

        public List<IngredientBox> ingredientBoxList;
        private Dictionary<string, Transform> stationTransformDict = new Dictionary<string, Transform>();

        [SerializeField] private IngredientSO _currentIGDType;
        [SerializeField] private int _currentIGDAmount;

        [SerializeField] private Transform spawnPosition;
        [SerializeField] private Transform trashPosition;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;

        [SerializeField] private GameObject currentIGDModel;
        [SerializeField] private Transform spawnIGDPos;

        public bool hasMovingIGD;
        private bool isDeliver = false;

        [Header("UI")]
        public GameObject giverPanel;
        public Image igdIcon;
        public TMP_Text igdAmountText;

        public enum GiverState
        {
            Idle,
            GetIngredient,
            DeliverIngredient,
            CheckLeftoverIngredient,
            DropIngredient,
            ReturnToSpawn
        }

        public GiverState state = GiverState.Idle;

        // ใช้ Property เพื่อตรวจจับการเปลี่ยนแปลงค่า
        private IngredientSO CurrentIGDType
        {
            get => _currentIGDType;
            set
            {
                _currentIGDType = value;
                UpdateGiverUI();
            }
        }

        private int CurrentIGDAmount
        {
            get => _currentIGDAmount;
            set
            {
                _currentIGDAmount = value;
                UpdateGiverUI();
            }
        }

        void Start()
        {
            chefAI = FindAnyObjectByType<ChefAI>();

            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // ค้นหาและเพิ่ม IngredientBox ทั้งหมดในฉากเข้าไปใน List
            ingredientBoxList = new List<IngredientBox>(FindObjectsOfType<IngredientBox>());

            foreach (var box in ingredientBoxList)
            {
                stationTransformDict[box.ingredientItem.ingredient.IngredientName] = box.transform;
            }

            UpdateGiverUI(); // เรียกครั้งแรกเพื่อให้ UI อยู่ในสถานะที่ถูกต้องตอนเริ่มเกม
        }

        void Update()
        {
            if (!chefAI.hasGetIngredients && isDeliver)
            {
                StopAllCoroutines();

                if (CurrentIGDAmount > 0)
                {
                    MoveToTrash();
                }
                else
                {
                    CurrentIGDType = null;
                    MoveToSpawn();
                }
            }

            if (state == GiverState.Idle)
            {
                if (chefAI.hasGetIngredients && !isDeliver)
                {
                    isDeliver = true;
                    MoveToIGDBox();
                    state = GiverState.GetIngredient;
                }
            }

            if (state == GiverState.GetIngredient && hasMovingIGD)
            {
                if (stationTransformDict.TryGetValue(chefAI.igdNeedType.IngredientName, out Transform station))
                {
                    agent.SetDestination(station.position);
                }
            }
        }

        public void SetStatus(float moveSpeed, float itemPerTime, Transform spawnPos, Transform trashPos, bool hasMoveIngredient)
        {
            agent.speed = moveSpeed;
            currentItemPerTime = itemPerTime;
            spawnPosition = spawnPos;
            trashPosition = trashPos;
            hasMovingIGD = hasMoveIngredient;
        }

        private void MoveToIGDBox()
        {
            if (stationTransformDict.TryGetValue(chefAI.igdNeedType.IngredientName, out Transform station))
            {
                agent.SetDestination(station.position);
                state = GiverState.GetIngredient;
                StartCoroutine(CollectIngredientRoutine(station));
            }
            else
            {
                state = GiverState.ReturnToSpawn;
            }
        }

        private IEnumerator CollectIngredientRoutine(Transform station)
        {
            if (hasMovingIGD)
            {
                float distanceToTarget = Vector3.Distance(agent.transform.position, station.position);

                // ตรวจสอบว่าเป้าหมายอยู่ในระยะโจมตี
                while (distanceToTarget > agent.stoppingDistance + 1f)
                {
                    distanceToTarget = Vector3.Distance(agent.transform.position, station.position);
                    yield return null;
                }
            }
            else
            {
                while (!HasReachedDestination(station))
                {
                    yield return null;
                }
            }

            CurrentIGDType = chefAI.igdNeedType;

            while (CurrentIGDAmount < chefAI.igdNeedAmount)
            {
                CurrentIGDAmount++;

                if (CurrentIGDAmount < chefAI.igdNeedAmount)
                    yield return new WaitForSeconds(currentItemPerTime);
            }

            MoveToChef();
        }

        private void MoveToChef()
        {
            state = GiverState.DeliverIngredient;
            agent.SetDestination(chefAI.transform.position);
            StartCoroutine(DeliverIngredientRoutine());
        }

        private IEnumerator DeliverIngredientRoutine()
        {
            while (!HasReachedDestination(chefAI.transform))
            {
                yield return null;
            }

            while (CurrentIGDAmount > 0 && !chefAI.CheckIfIngredientsComplete())
            {
                chefAI.ReceiveIngredient(CurrentIGDType, 1, out bool _);
                CurrentIGDAmount--;

                if (!chefAI.CheckIfIngredientsComplete())
                {
                    yield return new WaitForSeconds(currentItemPerTime);
                }
            }

            if (CurrentIGDAmount > 0)
            {
                MoveToTrash();
            }
            else
            {
                CurrentIGDType = null;
                MoveToSpawn();
            }
        }

        private void MoveToTrash()
        {
            agent.SetDestination(trashPosition.position);
            state = GiverState.DropIngredient;
            StartCoroutine(DiscardIngredientRoutine());
        }

        private IEnumerator DiscardIngredientRoutine()
        {
            while (!HasReachedDestination(trashPosition))
            {
                yield return null;
            }

            CurrentIGDType = null;
            CurrentIGDAmount = 0;
            MoveToSpawn();
        }

        private void MoveToSpawn()
        {
            StopAllCoroutines();
            agent.SetDestination(spawnPosition.position);
            isDeliver = false;
            state = GiverState.Idle;
        }

        private bool HasReachedDestination(Transform target)
        {
            return !agent.pathPending
                   && agent.remainingDistance <= agent.stoppingDistance
                   && agent.velocity.sqrMagnitude < 0.01f;
        }

        // **Optimized UI Update**
        private void UpdateGiverUI()
        {
            if (CurrentIGDAmount > 0 && CurrentIGDType != null)
            {
                giverPanel.SetActive(true);
                igdAmountText.text = CurrentIGDAmount.ToString();
                igdIcon.sprite = CurrentIGDType.IngredientIcon;

                if (currentIGDModel == null)
                {
                    currentIGDModel = Instantiate(_currentIGDType.IngredientPref, spawnIGDPos);
                }
            }
            else
            {
                giverPanel.SetActive(false);

                if (currentIGDModel != null)
                {
                    Destroy(currentIGDModel);
                    currentIGDModel = null;
                }
            }
        }
    }
}
