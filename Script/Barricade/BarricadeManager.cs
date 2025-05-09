using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class BarricadeManager : MonoBehaviour
    {
        public List<BarricadeStatus> barricadeList;

        public void SetUpBarricade(int maxHP, float repairTime)
        {
            foreach (var barricade in barricadeList)
            {
                barricade.gameObject.SetActive(true);
                barricade.SetStat(maxHP, repairTime);
            }
        }
    }
}
