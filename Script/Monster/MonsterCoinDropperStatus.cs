using SousRaccoon.Manager;
using TMPro;
using UnityEngine;

namespace SousRaccoon.Monster
{
    public abstract class MonsterCoinDropperStatus : MonoBehaviour
    {
        [Header("Stat")]
        public float maxHealthTimePoint;
        public float outOfTime;
        protected float currentHealthTimePoint;
        public bool isDead = false;

        [Header("Coin")]
        public int coinDropAmount;
        [SerializeField] private GameObject coinDropUI;
        [SerializeField] private TMP_Text coinDropText;

        protected bool hasOutOfTime = false;

        private void Start()
        {
            StageManager.instance.EventOnGameEnd += Die;
        }

        private void OnDestroy()
        {
            StageManager.instance.EventOnGameEnd -= Die;
        }

        public virtual void TakeDamage()
        {
            //TODO : This is Shit of Code because in future if project have coin dropper hp more than 1 or player damage
            RunStageManager.instance.AddCoin(coinDropAmount);
            coinDropText.text = coinDropAmount.ToString();
            coinDropUI.SetActive(true);
        }

        public virtual void Die() { }
    }
}
