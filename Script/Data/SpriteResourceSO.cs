using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "SpriteResource", menuName = "GameData/SpriteResource")]
    public class SpriteResourceSO : ScriptableObject
    {
        public List<MapSprite> mapIconSprite = new List<MapSprite>();
        public List<Sprite> stageIconSprite = new List<Sprite>();
    }
}

[System.Serializable]
public class MapSprite
{
    public List<Sprite> mapIcon = new List<Sprite>();
}