using SousRaccoon.Customer;
using SousRaccoon.Data;
using SousRaccoon.Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class SpawnerManager : MonoBehaviour
    {
        public NPCSpawnSet npcSpawnSet;
        public MonsterStatDataBase monsterStatData;
        public SpawnTimingDataBase spawnTimingData;

        [Header("Monster Spawner")]
        public MonsterSpawnerStatus monsterSpawner;
        public bool hasMonsSpawnerEvent;

        [Header("Customer Spawn Point")]
        [SerializeField] Transform customerSpawnPoint;

        [Header("Customer Spawn Timing")]
        public float minNextCustomerSpawnTime;
        public float maxNextCustomerSpawnTime;
        public float spawnTimePerCustomer = 1;

        [Header("Customer Spawn Quantity")]
        public int minSpawnCustomerQuantitySet;
        public int maxSpawnCustomerQuantitySet;

        [Header("Monster Spawn Timing")]
        public float minNextMonsterSpawnTime;
        public float maxNextMonsterSpawnTime;
        public float spawnTimePerMonster = 1;

        [Header("Monster Spawn Quantity")]
        public int minSpawnMonsterQuantitySet;
        public int maxSpawnMonsterQuantitySet;

        [Header("Monster Spawn Point")]
        public List<Transform> monsterFarSpawnPoint;
        [SerializeField] List<Transform> monsterCloseSetSpawnPoint;
        [SerializeField] List<Transform> monsterInRestaurantSpawnPoint;
        [SerializeField] List<Transform> monsterSinisterSpawnPoint;
        private Dictionary<int, List<Transform>> spawnPointDict;

        [SerializeField] List<Transform> monsterCoinDropperSpawnPoint;

        [Header("Perk")]
        public int customerMoneyDropMax;
        public int customerMoneyDropMid;
        public int customerMoneyDropMin;

        public int customerBlockTimes;

        [Header("CoinDrop Spawn Timing")]
        [SerializeField] float minNextMonsterCoinSpawnTime;
        [SerializeField] float maxNextMonsterCoinSpawnTime;
        [SerializeField] float currentMinNextMonsterCoinSpawnTime;
        [SerializeField] float currentMaxNextMonsterCoinSpawnTime;
        [SerializeField] float nextMonsterSpawnerSpawnTime;

        [Header("Monster Healing Dead Multiply")]
        public float healingDeadMultiply;

        [Header("Manager")]
        [SerializeField][Range(0, 100)] private float difficultyMultiplier;
        [SerializeField] private float delayMonsterSpawnTime;
        [SerializeField] private bool isCustomerSpawning = true;
        [SerializeField] private bool isMonsterSpawning = true;
        [SerializeField] private bool isMonsterSpawnerSpawning = true;
        [SerializeField] private bool isMonsterCoinSpawning = true;

        [Header("Change Phase")]
        //[SerializeField] private Animator animatorClosingSoon;
        [SerializeField] private Light mainLight;
        public List<string> dayColorList = new List<string>();
        public List<float> shadowStrenghList = new List<float>();
        public List<float> intensityLightList = new List<float>();
        public float duration = 1.0f;
        private float lerpTime = 0f;
        private Color startColor;
        private Color endColor;

        [Header("Endless")]
        public GameObject clockPanel;
        public float targetClockRotation;
        public float winClockRotation;
        public float timePhaseCount;
        public float dayPhase;
        public float noonPhase;
        public float eveningPhase;

        [SerializeField] private GlobalVolumeControl volumeControl;
        [SerializeField] private GameObject PointLight_Night;
        [SerializeField] private GameObject pointLight_Day;
        [SerializeField] private Animator pointLihgt_Day_Anim; // Duration of pointlight fade is 0.5s
        private bool isNight = false;

        public enum TimePhase { Day, Noon, Evening, Night, Storm, Ended }
        public TimePhase currentPhase = TimePhase.Day;

        private Coroutine vibePhaseCoroutine;
        //Check Closing Soon
        bool isEveningPhase = false;
        bool isNightPhase = false;

        private void Start()
        {
            dayPhase = RunStageManager.instance.dayPhaseTime;
            noonPhase = RunStageManager.instance.noonPhaseTime;
            eveningPhase = RunStageManager.instance.eveningPhaseTime;

            SetUpSpawnTiming();
            AdjustSpawnTimings();
            isCustomerSpawning = true;
            isMonsterSpawning = true;
            isMonsterCoinSpawning = true;

            spawnPointDict = new Dictionary<int, List<Transform>>()
            {
                { 0, monsterFarSpawnPoint },
                { 1, monsterCloseSetSpawnPoint },
                { 2, monsterInRestaurantSpawnPoint },
                { 3, monsterSinisterSpawnPoint },
            };

            currentMaxNextMonsterCoinSpawnTime = maxNextMonsterCoinSpawnTime;
            currentMinNextMonsterCoinSpawnTime = minNextMonsterCoinSpawnTime;
        }

        private void Update()
        {
            if (!StageManager.instance.isGameStart || StageManager.instance.isTutorial) return;

            timePhaseCount += Time.deltaTime;

            if (timePhaseCount <= eveningPhase)
            {
                float t = timePhaseCount / eveningPhase; // คำนวณค่าระหว่าง 0-1
                float lerpAngle = Mathf.Lerp(0, targetClockRotation, t); // คำนวณมุม Lerp
                clockPanel.transform.rotation = Quaternion.Euler(0, 0, lerpAngle); // หมุนวัตถุ
            }

            if (StageManager.instance.isGameWin)
            {
                float duration = 1f; // ระยะเวลาที่ต้องการให้เสร็จ
                float lerpSpeed = Mathf.Abs(targetClockRotation - winClockRotation) / duration;
                targetClockRotation = Mathf.MoveTowards(targetClockRotation, winClockRotation, Time.deltaTime * lerpSpeed);
                clockPanel.transform.rotation = Quaternion.Euler(0, 0, targetClockRotation);
            }

            switch (currentPhase)
            {
                case TimePhase.Day:
                    if (timePhaseCount > dayPhase)
                    {
                        TransitionToPhase(TimePhase.Noon);
                    }
                    break;

                case TimePhase.Noon:
                    if (timePhaseCount > noonPhase)
                    {
                        TransitionToPhase(TimePhase.Evening);
                    }

                    if (monsterSpawner.isMinionSpawning && timePhaseCount + 20f > eveningPhase)
                    {
                        monsterSpawner.StopSpawnMinion();
                    }
                    break;

                case TimePhase.Evening:
                    if (timePhaseCount > eveningPhase)
                    {
                        TransitionToPhase(TimePhase.Night);
                    }
                    break;

                case TimePhase.Night:
                    break;

                case TimePhase.Ended:
                    break;
            }

            if (!isCustomerSpawning && !isNightPhase)
            {
                StartSpawnCustomer();
            }

            if (!isMonsterSpawning && !StageManager.instance.isGameWin)
            {
                StartSpawnMonster();
            }

            if (!isMonsterCoinSpawning && !isNightPhase)
            {
                StartSpawnMonsterCoin();
            }
        }

        private void SetUpSpawnTiming()
        {
            minNextCustomerSpawnTime = spawnTimingData.minNextCustomerSpawnTime;
            maxNextCustomerSpawnTime = spawnTimingData.maxNextCustomerSpawnTime;
            spawnTimePerCustomer = spawnTimingData.spawnTimePerCustomer;
            minSpawnCustomerQuantitySet = spawnTimingData.minSpawnCustomerQuantitySet;
            maxSpawnCustomerQuantitySet = spawnTimingData.maxSpawnCustomerQuantitySet;
            minNextMonsterSpawnTime = spawnTimingData.minNextMonsterSpawnTime;
            maxNextMonsterSpawnTime = spawnTimingData.maxNextMonsterSpawnTime;
            spawnTimePerMonster = spawnTimingData.spawnTimePerMonster;
            minSpawnMonsterQuantitySet = spawnTimingData.minSpawnMonsterQuantitySet;
            maxSpawnMonsterQuantitySet = spawnTimingData.maxSpawnMonsterQuantitySet;
        }

        private void TransitionToPhase(TimePhase nextPhase)
        {
            currentPhase = nextPhase;
            StartPhase(currentPhase);
        }

        private void StartPhase(TimePhase phase)
        {
            switch (phase)
            {
                case TimePhase.Day:
                    Debug.Log("Starting Day Phase");
                    // ใส่ Logic การเปลี่ยนเป็น Day Phase
                    break;

                case TimePhase.Noon:
                    Debug.Log("Starting Noon Phase");
                    StartNoonPhase();
                    break;

                case TimePhase.Evening:
                    Debug.Log("Starting Evening Phase");
                    isEveningPhase = true;
                    StartEveningPhase();
                    break;

                case TimePhase.Night:
                    Debug.Log("Starting Night Phase");
                    if (isNightPhase) return;
                    isNightPhase = true;
                    EndDayPhase();
                    break;
                case TimePhase.Ended:
                    break;

            }
        }

        public void StartSpawning()
        {
            StageManager.instance.isGameStart = true;
            AudioManager.instance.PlayMusic("BG-StartState");

            if (StageManager.instance.isTutorial) return;

            StartCoroutine(StartStageSpawning());
        }

        private void StopAllSpawning()
        {
            StopAllCoroutines();
            isCustomerSpawning = false;
            isMonsterSpawning = false;
            isMonsterSpawnerSpawning = false;
        }

        private void StartNoonPhase()
        {
            StartChangingVibe(dayColorList[0], dayColorList[1], shadowStrenghList[0], shadowStrenghList[1], intensityLightList[0], intensityLightList[1]);
            volumeControl.StartBlendTo((int)currentPhase);

            if (monsterSpawner != null && hasMonsSpawnerEvent)
                monsterSpawner.StartSpawnMinion();
        }

        private void StartEveningPhase()
        {
            StartChangingVibe(dayColorList[1], dayColorList[2], shadowStrenghList[1], shadowStrenghList[2], intensityLightList[1], intensityLightList[2]);
            volumeControl.StartBlendTo((int)currentPhase);

            if (monsterSpawner != null && hasMonsSpawnerEvent)
                monsterSpawner.StartSpawnMinion();
        }

        private void EndDayPhase()
        {
            isNight = true;

            StartChangingVibe(dayColorList[2], dayColorList[3], shadowStrenghList[2], shadowStrenghList[3], intensityLightList[2], intensityLightList[3]);
            SpawnCustomer();
            AdjustSpawnTimings();
            volumeControl.StartBlendTo((int)currentPhase);
        }

        private void AdjustSpawnTimings()
        {
            float percentage = (100 - difficultyMultiplier) / 100;
            float reductionFactor = Mathf.Pow(percentage, RunStageManager.instance.daysCount); // ลดเวลา % ต่อรอบ
            minNextCustomerSpawnTime = spawnTimingData.minNextCustomerSpawnTime * reductionFactor;
            maxNextCustomerSpawnTime = spawnTimingData.maxNextCustomerSpawnTime * reductionFactor;
            minNextMonsterSpawnTime = spawnTimingData.minNextMonsterSpawnTime * reductionFactor;
            maxNextMonsterSpawnTime = spawnTimingData.maxNextMonsterSpawnTime * reductionFactor;
        }

        public void StartSpawnCustomer()
        {
            StartCoroutine(CustomerSpawning());
        }

        private void StartSpawnMonster()
        {
            StartCoroutine(MonsterSpawning());
        }

        private void StartSpawnMonsterCoin()
        {
            StartCoroutine(MonsterCoinSpawning());
        }

        private void StartChangingVibe(string startColorHex, string endColorHex, float startShadow, float endShadow, float startIntensity, float endIntensity)
        {
            if (vibePhaseCoroutine != null)
            {
                StopCoroutine(vibePhaseCoroutine);
            }
            vibePhaseCoroutine = StartCoroutine(ChangingVibePhase(startColorHex, endColorHex, startShadow, endShadow, startIntensity, endIntensity));
        }

        IEnumerator ChangingVibePhase(string startColorHex, string endColorHex, float startShadow, float endShadow, float startIntensity, float endIntensity)
        {
            Debug.Log("Start Changing Vibe");

            if (!ColorUtility.TryParseHtmlString(startColorHex, out startColor))
            {
                Debug.LogWarning($"Invalid color hex: {startColorHex}, using default white.");
                startColor = Color.white;
            }

            if (!ColorUtility.TryParseHtmlString(endColorHex, out endColor))
            {
                Debug.LogWarning($"Invalid color hex: {endColorHex}, using default white.");
                endColor = Color.white;
            }

            if (mainLight == null)
            {
                Debug.LogError("mainLight is null. Stopping Coroutine.");
                yield break;
            }

            if (duration <= 0f)
            {
                Debug.LogError("Duration is invalid. Stopping Coroutine.");
                yield break;
            }

            lerpTime = 0f;
            float changeColorTime = Time.time + duration;

            if (pointLight_Day != null && isNight)
            {
                pointLihgt_Day_Anim.Play("FadeLight");
            }

            while (Time.time < changeColorTime)
            {
                lerpTime = (Time.time - (changeColorTime - duration)) / duration;
                mainLight.color = Color.Lerp(startColor, endColor, lerpTime);
                mainLight.shadowStrength = Mathf.Lerp(startShadow, endShadow, lerpTime);
                mainLight.intensity = Mathf.Lerp(startIntensity, endIntensity, lerpTime);
                yield return null;
            }

            if (PointLight_Night != null && isNight)
                PointLight_Night.SetActive(true);

            if (pointLight_Day != null && isNight)
            {
                pointLight_Day.SetActive(false);
            }

            Debug.Log("End Changing Vibe");

            /*if (isEveningPhase)
            {
                animatorClosingSoon.Play("ClosingSoon");
            }*/
        }

        IEnumerator StartStageSpawning()
        {
            hasMonsSpawnerEvent = npcSpawnSet.hasMonsSpawnerEvent;
            monsterSpawner.hasSpawnToday = hasMonsSpawnerEvent;
            StartSpawnCustomer();

            yield return new WaitForSeconds(delayMonsterSpawnTime);

            StartSpawnMonster();
            StartSpawnMonsterCoin();
        }

        IEnumerator CustomerSpawning()
        {
            isCustomerSpawning = true;

            // Randomize the number of customers to spawn in this cycle
            int spawnCount = UnityEngine.Random.Range(minSpawnCustomerQuantitySet, maxSpawnCustomerQuantitySet);

            for (int i = 0; i < spawnCount; i++)
            {
                if (StageManager.instance.isGameEnd)
                {
                    yield break;
                }
                SpawnCustomer();

                // Replace WaitForSeconds with time-based check
                float nextSpawnTime = Time.time + spawnTimePerCustomer;
                while (Time.time < nextSpawnTime)
                {
                    yield return null; // Wait until the next frame to check again
                }
            }

            // Wait for the next spawn cycle using time-based check
            float nextCycleTime = Time.time + UnityEngine.Random.Range(minNextCustomerSpawnTime, maxNextCustomerSpawnTime);
            while (Time.time < nextCycleTime)
            {
                yield return null; // Wait until the next frame to check again
            }

            isCustomerSpawning = false;
        }

        IEnumerator MonsterSpawning()
        {
            isMonsterSpawning = true;

            int spawnCount = UnityEngine.Random.Range(minSpawnMonsterQuantitySet, maxSpawnMonsterQuantitySet);

            for (int i = 0; i < spawnCount; i++)
            {
                if (StageManager.instance.isGameWin)
                {
                    yield break;
                }

                SpawnMonster();

                float nextSpawnTime = Time.time + spawnTimePerMonster;
                while (Time.time < nextSpawnTime)
                {
                    yield return null; // Wait until the next frame to check again
                }
            }

            float nextCycleTime = Time.time + UnityEngine.Random.Range(minNextMonsterSpawnTime, maxNextMonsterSpawnTime);
            while (Time.time < nextCycleTime)
            {
                yield return null; // Wait until the next frame to check again
            }

            isMonsterSpawning = false;
        }

        IEnumerator MonsterCoinSpawning()
        {
            isMonsterCoinSpawning = true;

            if (StageManager.instance.isGameWin)
            {
                yield break;
            }

            SpawnMonsterCoinDropper();
            float nextCycleTime = Time.time + UnityEngine.Random.Range(currentMinNextMonsterCoinSpawnTime, currentMaxNextMonsterCoinSpawnTime);

            while (Time.time < nextCycleTime)
            {
                yield return null; // Wait until the next frame to check again
            }

            isMonsterCoinSpawning = false;
        }

        private void SpawnCustomer()
        {
            StageManager.instance.customerSpawnCount++;

            // Instantiate customer at the spawn point
            var customer = Instantiate(npcSpawnSet.customersSetPref[UnityEngine.Random.Range(0, npcSpawnSet.customersSetPref.Count)],
                                       customerSpawnPoint.position,
                                       customerSpawnPoint.rotation);
            customer.GetComponent<CustomerStatus>().blockTimes = customerBlockTimes;
            customer.GetComponent<CustomerStatus>().SetMoneyDropRunStage(customerMoneyDropMax, customerMoneyDropMid, customerMoneyDropMin);
        }

        private void SpawnMonster()
        {
            // Get monster type and corresponding monster prefab list
            var monsterPref = npcSpawnSet.GetRandomMonsterType(out var monsterType);

            // ค้นหา spawn point list สำหรับ monster type ปัจจุบัน
            List<Transform> spawnPointList = null;

            // ลองหาจาก dictionary
            if (spawnPointDict.ContainsKey(monsterType) && spawnPointDict[monsterType].Count > 0)
            {
                spawnPointList = spawnPointDict[monsterType];
            }
            else
            {
                // ถ้าไม่พบ spawn point สำหรับ monster type ปัจจุบัน
                // ลองหา spawn point จาก monsterTypeDict ที่มีข้อมูล
                foreach (var entry in spawnPointDict)
                {
                    if (entry.Value.Count > 0) // หากพบ spawn point ที่มีข้อมูล
                    {
                        spawnPointList = entry.Value;
                        monsterType = entry.Key; // ใช้ monsterType ของ spawn point ที่เลือก
                        break; // ออกจากลูปเมื่อเจอ
                    }
                }
            }

            // ถ้าพบ spawnPointList ที่มีข้อมูล
            if (spawnPointList != null && spawnPointList.Count > 0)
            {
                var monsterSpawnPoint = spawnPointList[UnityEngine.Random.Range(0, spawnPointList.Count)];

                var monsPref = monsterPref[UnityEngine.Random.Range(0, monsterPref.Count)];

                TryGetMonsterStat(monsPref.name, out var hp, out var dmToC, out var dmToP);

                // Instantiate Monster at the spawn point
                var monst = Instantiate(monsPref,
                                        monsterSpawnPoint.position,
                                        monsterSpawnPoint.rotation);

                var monStat = monst.GetComponent<MonsterStatus>();
                monStat.SetStatus(hp, dmToC, dmToP, healingDeadMultiply);
            }
            else
            {
                // ถ้าไม่พบ spawn point เลย (กรณีนี้จะไม่เกิดขึ้นหากข้อมูลถูกตั้งค่าให้ถูกต้อง)
                Debug.LogWarning("No valid spawn points available for any monster type.");
            }
        }

        public void SpawnMonsterCoinDropper()
        {
            var monsterPref = npcSpawnSet.GetRandomCoinDropperType();

            if (monsterCoinDropperSpawnPoint != null && monsterCoinDropperSpawnPoint.Count > 0)
            {
                var monsterSpawnPoint = monsterCoinDropperSpawnPoint[UnityEngine.Random.Range(0, monsterCoinDropperSpawnPoint.Count)];

                // Instantiate Monster at the spawn point
                Instantiate(monsterPref,
                            monsterSpawnPoint.position,
                            monsterSpawnPoint.rotation);
            }
        }

        private void TryGetMonsterStat(string name, out int hp, out float dmToC, out int dmToP)
        {
            hp = 0;
            dmToC = 0;
            dmToP = 0;

            foreach (var datas in monsterStatData.monsterBaseStat)
            {
                if (datas.name == name)
                {
                    hp = datas.MaxHealth;
                    dmToC = datas.DamageToCustomer;
                    dmToP = datas.DamageToPlayer;
                }
            }
        }

        public void SetCoinDropperSpawnTime(float min, float max)
        {
            var minTime = minNextMonsterCoinSpawnTime - min;
            var maxTime = maxNextMonsterCoinSpawnTime - max;
            currentMinNextMonsterCoinSpawnTime = minTime;
            currentMaxNextMonsterCoinSpawnTime = maxTime;
        }
    }
}