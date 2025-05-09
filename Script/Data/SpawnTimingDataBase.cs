using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "SpawnTiming", menuName = "GameData/SpawnTimingData")]
    public class SpawnTimingDataBase : ScriptableObject
    {
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
    }
}
