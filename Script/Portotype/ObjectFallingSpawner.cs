using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class ObjectFallingSpawner : MonoBehaviour
    {
        [SerializeField] Vector2 spawnZoneX; // �ͺࢵ�ǹ͹
        [SerializeField] Vector2 spawnZoneZ; // �ͺࢵ���֡
        [SerializeField] float spawnHeight; // �����٧����������

        [SerializeField] FallingObject fallingObjectPrefab; // Prefab �ͧ�ѵ�ط�赡

        [SerializeField] float damageToCustomer; // ��������зӡѺ�١���
        [SerializeField] int damageToPlayer; // ��������зӡѺ������

        [SerializeField] float timeSpawnDuration = 10f; // �������ҷ����Դ�ѵ��
        [SerializeField] float spawnTimePerObject = 1f; // ���ҷ������ҧ�ѵ�����Ъ��

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
            // ��Ǩ�ͺ���������������ش������ҧ�ѵ��
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
                // �������˹觡���Դ㹢ͺࢵ����˹�
                float randomX = Random.Range(spawnZoneX.x, spawnZoneX.y);
                float randomZ = Random.Range(spawnZoneZ.x, spawnZoneZ.y);
                Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

                // ���ҧ�ѵ�بҡ Prefab
                var fallingObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
                fallingObject.SetFallingObject(damageToCustomer, damageToPlayer);

                // ���������ҧ�ѵ�ت�鹶Ѵ�
                yield return new WaitForSeconds(spawnTimePerObject);
                hasSpawning = false;
            }
        }

        IEnumerator SpawnDurationTimer()
        {
            yield return new WaitForSeconds(timeSpawnDuration);
            StopSpawnObjectFalling(); // ��ش������ҧ��ѧ�������
        }
    }

}