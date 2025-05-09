using SousRaccoon.Customer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace SousRaccoon.Player
{
    public class PlayerHealingDanceSystem : MonoBehaviour
    {
        PlayerVFX playerVFX;
        PlayerInputManager inputManager;
        PlayerAnimatorManager animatorManager;
        PlayerAudioManager playerAudioManager;

        public float healingRadius = 2f; // รัศมีสำหรับการ Heal
        public LayerMask targetLayer; // Layer ของNPC(ลูกค้า)
        public float healingRate; // Per Second

        public float currentHealRange;
        public float currentHealRate;

        public float runStageHealRate;
        public float scenarioHealRate;

        [SerializeField] private VisualEffect healAreaVFX;

        bool isHealing = false;

        private HashSet<CustomerStatus> customersInRange = new HashSet<CustomerStatus>();

        private void Start()
        {
            playerVFX = GetComponent<PlayerVFX>();
            inputManager = GetComponent<PlayerInputManager>();
            animatorManager = GetComponent<PlayerAnimatorManager>();
            playerAudioManager = GetComponent<PlayerAudioManager>();

            currentHealRate = healingRate;
            currentHealRange = healingRadius;

            isHealing = false;

            HealVFXRatioCalculateValues();
        }

        private void Update()
        {
            if (inputManager.isHealAction)
            {
                DetectCustomersInRange();
            }
        }

        private void DetectCustomersInRange()
        {
            // หาผู้เล่นทั้งหมดที่อยู่ในรัศมีการ Healing
            Collider[] targetInRange = Physics.OverlapSphere(transform.position, currentHealRange, targetLayer);

            HashSet<CustomerStatus> newCustomersInRange = new HashSet<CustomerStatus>();

            foreach (Collider target in targetInRange)
            {
                if (target.CompareTag("Customer"))
                {
                    var customer = target.GetComponent<CustomerStatus>();
                    if (customer != null)
                    {
                        newCustomersInRange.Add(customer);
                    }
                }
            }

            // หาผู้ที่เข้ามาใหม่ และผู้ที่ออกไป
            foreach (var customer in customersInRange)
            {
                if (!newCustomersInRange.Contains(customer))
                {
                    customersInRange.Remove(customer);
                }
            }

            foreach (var customer in newCustomersInRange)
            {
                if (!customersInRange.Contains(customer))
                {
                    customersInRange.Add(customer);
                }
            }
        }

        public void PerformDanceHealing()
        {
            if (isHealing) return;

            isHealing = true;
            animatorManager.Dance(true);
            playerVFX.SetHealing(true);
            HealVFXRatioCalculateValues();
            StartCoroutine(DancingHealing());
        }

        public void StopDanceHealing()
        {
            isHealing = false;
            inputManager.isHealAction = false;
            animatorManager.Dance(false);
            playerVFX.SetHealing(false);
        }

        IEnumerator DancingHealing()
        {
            float timePassed = 0f;

            while (inputManager.isHealAction)
            {
                timePassed += Time.deltaTime;

                if (timePassed >= 1f)
                {
                    bool hasHeal = false;

                    foreach (var customer in customersInRange)
                    {
                        customer.Heal(currentHealRate);
                        hasHeal = true;
                    }

                    if (hasHeal)
                        playerAudioManager.PlayerHealDetectSFX();

                    timePassed = 0f;
                }

                yield return null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healingRadius);
        }

        public void ResultHealRate()
        {
            currentHealRate = healingRate + runStageHealRate + scenarioHealRate;
        }

        public void SetHealRateBuff(float buffValue)
        {
            currentHealRate = healingRate + buffValue;
        }

        public void ResetHealRate()
        {
            currentHealRate = healingRate;
        }

        public void SetHealRangeBuff(float buffValue)
        {
            currentHealRange = healingRadius + buffValue;

            HealVFXRatioCalculateValues();
        }

        public void ResetHealRange()
        {
            currentHealRange = healingRadius;

            HealVFXRatioCalculateValues();
        }

        public void HealVFXRatioCalculateValues()
        {
            float ratioTinArea = 1f;
            float ratioMainArea = 0.8f;
            float ratioHealRange = 0.4f;

            var tinRadius = (currentHealRange / ratioHealRange) * ratioTinArea;
            var mainRadius = (currentHealRange / ratioHealRange) * ratioMainArea;

            healAreaVFX.SetFloat("TinRadius", tinRadius);
            healAreaVFX.SetFloat("MainRadius", mainRadius);
        }
    }
}
