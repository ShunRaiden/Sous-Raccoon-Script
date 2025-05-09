using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data.Item
{
    [CreateAssetMenu(fileName = "AllItemData", menuName = "GameData/AllItemData")]
    public class AllItemDataEachList : ScriptableObject
    {
        public List<ItemListInState> itemListInStates = new List<ItemListInState>();

        public List<NPCSpawnSet> nPCSpawnSets = new List<NPCSpawnSet>();

        public List<MonsterStatDataBase> monsterStatDataBases = new List<MonsterStatDataBase>();
    }
}