using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SousRaccoon.Player
{
    public class PlayerAudioManager : MonoBehaviour
    {
        AudioManager audioManager;

        [Header("Run SFX")]
        public AudioSource run_AudioSource;
        public AudioSource jump_AudioSource;
        [SerializeField] float fadeDuration;
        bool isPlay = false;

        [Header("Run SFX")]
        public AudioController attackSFXList;
        private List<AsyncOperationHandle<AudioClip>> attackSFXHandles = new List<AsyncOperationHandle<AudioClip>>();

        private void Start()
        {
            audioManager = AudioManager.instance;
        }

        private void OnDestroy()
        {
            audioManager.ReleaseAudio(attackSFXList);
        }

        public void PlayRunSFX()
        {
            if (!run_AudioSource.isPlaying)
            {
                run_AudioSource.Play();
                isPlay = true;
            }
        }
        public void StopRunSFX()
        {
            if (isPlay)
            {
                isPlay = false;
                StartCoroutine(FadeToStop());
            }
        }

        public void ForceStopSunSFX()
        {
            run_AudioSource.Stop();
        }

        private IEnumerator FadeToStop()
        {
            // Fade Out เพลงปัจจุบัน
            float startVolume = run_AudioSource.volume;
            float defalutVolume = run_AudioSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                run_AudioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            run_AudioSource.Stop();
            run_AudioSource.volume = defalutVolume;
        }

        public void PlayRollingSFX()
        {
            audioManager.PlayOneShotSFX("Rolling_Player");
        }

        public void PlayJumpSFX()
        {
            jump_AudioSource.PlayOneShot(jump_AudioSource.clip);
        }

        public async void PlayRandomAttackSFX()
        {
            if (attackSFXList == null)
            {
                attackSFXList = await audioManager.GetAudio("PlayerAttack");
            }

            var cliplist = attackSFXList.TryGetAudioClipList();
            var clip = cliplist[Random.Range(0, cliplist.Count)];
            audioManager.s_AudioSource.PlayOneShot(clip);
        }

        public void PlayerHitBySFX()
        {
            audioManager.PlayOneShotSFX("HitBy_NPC");
        }

        public void PlayerHealDetectSFX()
        {
            audioManager.PlayStageSFXOneShot("Player_Heal_Detect_2");
        }

        public void PlayerTakeDamageSFX()
        {
            audioManager.PlayStageSFXOneShot("Raccoon_Take_Damage");
        }

        public void PlayerStunSFX()
        {
            audioManager.PlayStageSFXOneShot("Raccoon_Stun");
        }
    }
}

