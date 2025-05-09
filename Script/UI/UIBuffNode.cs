using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIBuffNode : MonoBehaviour
    {
        public Image Icon;
        public Image bgCountDown;

        public void OutputBuff(float timer)
        {
            bgCountDown.fillAmount = timer;
        }
    }

}
