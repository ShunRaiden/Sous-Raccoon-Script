using TMPro;
using UnityEngine;

namespace SousRaccoon.UI
{
    public class UIAddMoney : MonoBehaviour
    {
        public int amount;
        public TMP_Text moneyText;
        // Start is called before the first frame update
        void Start()
        {
            moneyText = GetComponent<TMP_Text>();
            moneyText.text = amount.ToString();
            Destroy(gameObject, 3f);
        }
    }
}

