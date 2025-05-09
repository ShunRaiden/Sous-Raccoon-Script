using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MinionBaseStatus : MonsterStatus
    {
        [Header("Running Area")]
        [SerializeField] protected Vector3 maxArea;
        [SerializeField] protected Vector3 minArea;

        public void SetStatus(int setMaxHealth,
              float setMonsterDamageToCustomer,
              int setMonsterDamageToPlayer,
              float healingDeadMultiply,
              Vector3 maxAreas,
              Vector3 minAreas)
        {
            maxHealth = setMaxHealth;
            healAmount = maxHealth * healingDeadMultiply;
            monsterDamageToCustomer = setMonsterDamageToCustomer;
            monsterDamageToPlayer = setMonsterDamageToPlayer;

            maxArea = maxAreas;
            minArea = minAreas;
            //InitializeHealthIcons();
        }

        public Vector3 RandomMove()
        {
            float randomX = Random.Range(minArea.x, maxArea.x);
            float randomZ = Random.Range(minArea.z, maxArea.z);
            return new Vector3(randomX, 0f, randomZ);
        }
    }
}
