using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Data
{
    [CreateAssetMenu(fileName = "AudioList", menuName = "GameData/AudioList")]
    public class AudioDataSO : ScriptableObject
    {
        public List<AudioClip> musicClips;
        public List<AudioClip> soundClips;
        public List<AudioClip> voiceClips;

        public List<AudioClip> attackClips;

        public AudioClip GetAudioClip(string audioName)
        {
            // ��� List �������� List ����
            List<AudioClip> allClips = new List<AudioClip>();
            allClips.AddRange(musicClips);
            allClips.AddRange(soundClips);
            allClips.AddRange(voiceClips);
            allClips.AddRange(attackClips);

            // ���� AudioClip ����������͹�Ѻ audioName
            AudioClip clip = allClips.Find(audioClip => audioClip.name == audioName);

            if (clip != null)
            {
                return clip;
            }
            else
            {
                Debug.LogWarning("Audio clip not found: " + audioName);
                return null;
            }
        }
    }
}
