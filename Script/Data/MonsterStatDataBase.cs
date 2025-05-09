using SousRaccoon.Monster;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "GameData/MonsterData")]
    public class MonsterStatDataBase : ScriptableObject
    {
        [Header("Base Stat")]
        public List<MonsterBaseStat> monsterBaseStat = new();
    }

    [System.Serializable]
    public class MonsterBaseStat
    {
        public string name;

        public int MaxHealth;

        public float DamageToCustomer;

        public int DamageToPlayer;
    }
}