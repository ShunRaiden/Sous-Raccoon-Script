using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace SousRaccoon.Player
{
    public class PlayerVFX : MonoBehaviour
    {
        public GameObject raccoonMarker;
        [SerializeField] private VisualEffect heal;
        [SerializeField] private VisualEffect stun;

        [SerializeField] private List<Animator> slashAnim = new();
        int slashIndex = 0;

        [SerializeField] private GameObject landingDust;
        [SerializeField] private VisualEffect walkDust;

        public bool forcedHideMarker = false;

        public void SetHealing(bool isHeal)
        {
            heal.gameObject.SetActive(isHeal);
        }

        public void SetStun(bool isStun)
        {
            stun.gameObject.SetActive(isStun);
        }

        public void SetMarker(bool isJump)
        {
            if (!forcedHideMarker)
                raccoonMarker.SetActive(isJump);
        }

        public void SpawnLandingDust()
        {
            Instantiate(landingDust, transform.position, transform.rotation);
        }

        public void OnWalkVFX()
        {
            walkDust.gameObject.SetActive(true);
        }

        public void OnStopWalkVFX()
        {
            StartCoroutine(StopWalkingVFX());
        }

        IEnumerator StopWalkingVFX()
        {
            walkDust.Stop();
            yield return new WaitForSeconds(0.2f);
            walkDust.gameObject.SetActive(false);
        }

        public void OnSlash()
        {
            if (slashIndex > 2)
            {
                slashIndex = 0;
            }

            slashAnim[slashIndex].SetTrigger("Slash");
            slashIndex++;
        }
    }
}