using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIPlayerPanel : MonoBehaviour
    {
        /// <summary>
        /// Outdate Script Not use for now
        /// </summary>

        [Header("Stun")]
        [SerializeField] private Image gaugeStunIcon;
        [SerializeField] private List<Sprite> gaugeSprite = new();
        public Image GaugeStunIcon => gaugeStunIcon;
        public Image gaugeStunIconWolrdSpace;

        // Start is called before the first frame update
        void Start()
        {
            gaugeStunIcon.fillAmount = 0;
        }

        public void SetStunIcon(int index)
        {
            gaugeStunIcon.sprite = gaugeSprite[index];
        }

        public void UpdateStunIcon(float rate)
        {
            gaugeStunIcon.fillAmount = rate;
            gaugeStunIconWolrdSpace.fillAmount = rate;
        }
    }
}

