using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SousRaccoon.Manager
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager instance { get { return _instance; } }
        private static AudioManager _instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject); // Destroy the new instance, not this script
                return;
            }

            _instance = this;
        }
        #endregion

        //public AudioDataSO audioData;

        [Header("------- Audio Source -------")]
        public AudioSource m_AudioSource;
        public AudioSource s_AudioSource;
        public AudioSource v_AudioSource;

        [Header("------- Audio Clip -------")]
        public AudioClip v_Dialouge;
        public AudioClip s_ClickButton;
        public AudioClip s_HoverButton;
        public Dictionary<string, AudioClip> s_StageAudio = new Dictionary<string, AudioClip>();

        public Transform audioParent;

        private Dictionary<AudioController, AsyncOperationHandle<GameObject>> spawnedAudioHandles = new();
        private AsyncOperationHandle<AudioClip> bgmHandle;

        [SerializeField] private float fadeDuration = 1f; // �������ҷ����㹡�� Fade

        private async void Start()
        {
            AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>("ClickButton");
            await handle.Task; // �������Ŵ���稡�͹

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                s_ClickButton = handle.Result;

                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError($"[PlayOneShotSFX] ��Ŵ ClickButton ��������!");
            }


        }

        public async void PlayMusic(string clipName)
        {
            // ������ŧ���ѧ���������� Fade 价�� "BG-StartState"
            if (m_AudioSource.isPlaying)
            {
                Addressables.Release(bgmHandle);
                FadeToMusic(clipName);
            }
            else
            {
                // ��Ŵ�ŧ
                bgmHandle = Addressables.LoadAssetAsync<AudioClip>(clipName);
                await bgmHandle.Task;
                if (bgmHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // ��駤�� AudioClip ��� m_AudioSource
                    m_AudioSource.clip = bgmHandle.Result;
                    m_AudioSource.Play();
                }
                else
                {
                    Debug.LogError($"Cant Find {clipName}");
                }
            }
        }

        // ��������¹�ŧ
        public async void FadeToMusic(string addressableKey)
        {
            float startVolume = m_AudioSource.volume;

            // Fade Out
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                m_AudioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                await Task.Yield();
            }

            // ��Ŵ�ŧ
            bgmHandle = Addressables.LoadAssetAsync<AudioClip>(addressableKey);
            await bgmHandle.Task;
            if (bgmHandle.Status != AsyncOperationStatus.Succeeded) return;

            // ����¹�ŧ
            m_AudioSource.Stop();
            m_AudioSource.clip = bgmHandle.Result;
            m_AudioSource.Play();

            // Fade In
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                m_AudioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
                await Task.Yield();
            }
            m_AudioSource.volume = startVolume;
        }

        public async void PlaySoundButtonClick()
        {
            if (s_ClickButton == null)
            {
                AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>("ClickButton");
                await handle.Task; // �������Ŵ���稡�͹

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    s_ClickButton = handle.Result;
                    s_AudioSource.PlayOneShot(s_ClickButton);

                    // ��������§��蹨���͹ Release
                    await Task.Delay(Mathf.CeilToInt(s_ClickButton.length * 1000));
                    Addressables.Release(handle);
                }
                else
                {
                    Debug.LogError($"[PlayOneShotSFX] ��Ŵ ClickButton ��������!");
                }
            }
            else
            {
                s_AudioSource.PlayOneShot(s_ClickButton);
            }
        }

        public async void PlaySoundButtonHover()
        {
            if (s_HoverButton == null)
            {
                AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>("HoverButton");
                await handle.Task; // �������Ŵ���稡�͹

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    s_HoverButton = handle.Result;
                    s_AudioSource.PlayOneShot(s_HoverButton);

                    // ��������§��蹨���͹ Release
                    await Task.Delay(Mathf.CeilToInt(s_HoverButton.length * 1000));
                    Addressables.Release(handle);
                }
                else
                {
                    Debug.LogError($"[PlayOneShotSFX] ��Ŵ ClickButton ��������!");
                }
            }
            else
            {
                s_AudioSource.PlayOneShot(s_HoverButton);
            }
        }

        public async void PlayOneShotSFX(string clipName)
        {
            AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(clipName);
            await handle.Task; // �������Ŵ���稡�͹

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AudioClip clip = handle.Result;
                s_AudioSource.PlayOneShot(clip);

                // ��������§��蹨���͹ Release
                await Task.Delay(Mathf.CeilToInt(clip.length * 1000));
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError($"[PlayOneShotSFX] ��Ŵ {clipName} ��������!");
            }
        }

        public void PlayOneShotVoice(string clipName)
        {
            v_AudioSource.PlayOneShot(Addressables.LoadAssetAsync<AudioClip>(clipName).Result);
        }

        public void StopMusic()
        {
            m_AudioSource.Stop();
        }

        public void StopSFX()
        {
            s_AudioSource.Stop();
        }

        public void StopVoice()
        {
            v_AudioSource.Stop();
        }

        public async Task LoadStageAudio()
        {
            AsyncOperationHandle<IList<AudioClip>> handle = Addressables.LoadAssetsAsync<AudioClip>("InStageSFX", null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var clip in handle.Result)
                {
                    if (!s_StageAudio.ContainsKey(clip.name))
                    {
                        s_StageAudio.Add(clip.name, clip);
                    }
                }
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError("[LoadStageAudio] ��Ŵ InStageSFX ��������!");
            }
        }

        public void PlayStageSFXOneShot(string clipName)
        {
            var clip = GetStageAudio(clipName);
            if (clip != null)
                s_AudioSource.PlayOneShot(clip);
        }

        private AudioClip GetStageAudio(string clipName)
        {
            if (s_StageAudio.TryGetValue(clipName, out var clip))
            {
                return clip;
            }

            Debug.LogError($"[GetStageAudio] ��辺 AudioClip ���� {clipName}");
            return null;
        }

        public async Task<AudioController> GetAudio(string clipName)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(clipName, audioParent);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject asset = handle.Result;
                AudioController audioController = asset.GetComponent<AudioController>();

                if (audioController != null)
                {
                    spawnedAudioHandles[audioController] = handle;
                    return audioController; // �觡�Ѻ AudioController
                }
                else
                {
                    Debug.LogError($"[GetAudio] {clipName} ����� AudioController!");
                    Addressables.Release(handle);
                }
            }
            else
            {
                Debug.LogError($"[GetAudio] ��Ŵ {clipName} ��������!");
            }

            return null; // ���������� �觤�� null ��Ѻ
        }

        public void ReleaseAudio(AudioController audioController)
        {
            if (audioController == null)
            {
                Debug.LogError("audioController == null");
                return;
            }

            if (spawnedAudioHandles.TryGetValue(audioController, out var handle))
            {
                spawnedAudioHandles.Remove(audioController);
                Addressables.Release(handle);

                if (audioController.gameObject != null)
                    Destroy(audioController.gameObject);
            }
            else
            {
                Debug.LogWarning("[ReleaseAudio] AudioController ������ Dictionary!");
            }
        }
    }
}
