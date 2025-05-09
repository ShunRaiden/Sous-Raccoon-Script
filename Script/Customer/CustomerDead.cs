using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Customer
{
    public class CustomerDead : MonoBehaviour
    {
        [Header("Time")]
        // Clean up Time
        public float maxCleanUpTime;
        public float currentCleanUpTime;
        public float countPerTimes = .1f; // damage dealt per second

        public Image cleanUpBar;

        public GameObject trashPref;
        public GameObject cleanUpVFX;
        public bool isChair = false;

        bool completeCleanUp = false;

        private void Start()
        {
            StageManager.instance.currentCustomerDieCount++;
        }

        public void CleanUp()
        {
            if (cleanUpBar != null)
            {
                cleanUpVFX.SetActive(true);
                currentCleanUpTime += countPerTimes; // Decrease health over time
                cleanUpBar.fillAmount = currentCleanUpTime / maxCleanUpTime; // Update the fill amount of the health bar

                if (currentCleanUpTime >= maxCleanUpTime)
                {
                    if (!completeCleanUp)
                    {
                        completeCleanUp = true;
                        //Spwan Trash
                        var deadTransform = gameObject.transform;
                        var trash = Instantiate(trashPref, deadTransform.transform.position, deadTransform.transform.rotation);

                        if (isChair && trash != null)
                            trash.GetComponent<CustomerTrash>().SetPos();

                        StopCleanUp();
                        Destroy(gameObject);
                    }
                }
            }
        }

        public void StopCleanUp()
        {
            if (cleanUpVFX != null)
                cleanUpVFX.SetActive(false);
        }
    }
}

