using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "SceneListData", menuName = "GameData/SceneList")]
    public class StageListDataSO : ScriptableObject
    {
        public List<StageList> stageLists = new();
    }

    [System.Serializable]
    public class StageList
    {
        public List<string> sceneNameList = new();
    }
}