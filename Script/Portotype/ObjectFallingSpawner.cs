using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class ObjectFallingSpawner : MonoBehaviour
    {
        [SerializeField] Vector2 spawnZoneX; // ขอบเขตแนวนอน
        [SerializeField] Vector2 spawnZoneZ; // ขอบเขตแนวลึก
        [SerializeField] float spawnHeight; // ความสูงที่เริ่มตก

        [SerializeField] FallingObject fallingObjectPrefab; // Prefab ของวัตถุที่ตก

        [SerializeField] float damageToCustomer; // ดาเมจที่จะทำกับลูกค้า
        [SerializeField] int damageToPlayer; // ดาเมจที่จะทำกับผู้เล่น

        [SerializeField] float timeSpawnDuration = 10f; // ระยะเวลาที่จะเกิดวัตถุ
        [SerializeField] float spawnTimePerObject = 1f; // เวลาที่จะสร้างวัตถุแต่ละชิ้น

        bool isSpawn = false;
        bool hasSpawning = false;

        private void Start()
        {
            StageManager.instance.OnChefRageEvent += StartSpawnObjectFalling;
        }

        private void OnDestroy()
        {
            StageManager.instance.OnChefRageEvent -= StartSpawnObjectFalling;
        }

        void Update()
        {
            // ตรวจสอบเพื่อเริ่มหรือหยุดการสร้างวัตถุ
            if (isSpawn && !IsInvoking(nameof(ObjectFallingSpawning)))
            {
                if (hasSpawning) return;
                StartCoroutine(ObjectFallingSpawning());
            }
        }

        public void StartSpawnObjectFalling()
        {
            StopAllCoroutines();
            isSpawn = true;
            hasSpawning = false;
            StartCoroutine(SpawnDurationTimer());
        }

        public void StopSpawnObjectFalling()
        {
            isSpawn = false;
            StopAllCoroutines();
        }

        IEnumerator ObjectFallingSpawning()
        {
            while (isSpawn)
            {
                hasSpawning = true;
                // สุ่มตำแหน่งการเกิดในขอบเขตที่กำหนด
                float randomX = Random.Range(spawnZoneX.x, spawnZoneX.y);
                float randomZ = Random.Range(spawnZoneZ.x, spawnZoneZ.y);
                Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

                // สร้างวัตถุจาก Prefab
                var fallingObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
                fallingObject.SetFallingObject(damageToCustomer, damageToPlayer);

                // รอเวลาสร้างวัตถุชิ้นถัดไป
                yield return new WaitForSeconds(spawnTimePerObject);
                hasSpawning = false;
            }
        }

        IEnumerator SpawnDurationTimer()
        {
            yield return new WaitForSeconds(timeSpawnDuration);
            StopSpawnObjectFalling(); // หยุดการสร้างหลังหมดเวลา
        }
    }

}