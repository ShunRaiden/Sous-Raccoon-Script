using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "NPCSpawnSet", menuName = "Restaurant/NPCSet")]
    public class NPCSpawnSet : ScriptableObject
    {
        public List<GameObject> customersSetPref;

        public List<GameObject> monsterFarSetPref;
        public List<GameObject> monsterCloseSetPref;
        public List<GameObject> monsterInRestaurantSetPref;
        public List<GameObject> monsterSinisterSetPref;

        public List<GameObject> monsterCoinDropper;

        public bool hasMonsSpawnerEvent;

        private Dictionary<int, List<GameObject>> monsterTypeDict;

        private void OnEnable()
        {
            // Initialize the dictionary
            monsterTypeDict = new Dictionary<int, List<GameObject>>();

            // Create a list of all the monster lists
            List<List<GameObject>> monsterSets = new List<List<GameObject>>()
            {
                monsterFarSetPref,
                monsterCloseSetPref,
                monsterInRestaurantSetPref,
                monsterSinisterSetPref
            };

            // Loop through each list and add it to the dictionary if it's not null and not empty
            for (int i = 0; i < monsterSets.Count; i++)
            {
                if (monsterSets[i] != null && monsterSets[i].Count > 0)
                {
                    monsterTypeDict.Add(i, monsterSets[i]);
                    Debug.Log($"Added key {i} to monsterTypeDict.");
                }
                else
                {
                    Debug.LogWarning($"Monster set {i} is empty or null, skipping...");
                }
            }
        }

        public List<GameObject> GetRandomMonsterType(out int monsterType)
        {
            if (monsterTypeDict.Count == 0)
            {
                monsterType = -1;
                return null;
            }

            // สุ่มค่าคีย์จาก Keys ของ Dictionary
            var keys = new List<int>(monsterTypeDict.Keys);
            monsterType = keys[Random.Range(0, keys.Count)];

            // คืนค่ารายการ Monster
            return monsterTypeDict[monsterType];
        }

        public GameObject GetRandomCoinDropperType()
        {
            if (monsterCoinDropper.Count == 0)
            {
                return null;
            }

            return monsterCoinDropper[Random.Range(0, monsterCoinDropper.Count)];
        }
    }

}

