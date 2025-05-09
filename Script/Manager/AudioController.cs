using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] List<AudioClip> clipList;

    public AudioClip TryGetAudioClip()
    {
        return clip;
    }

    public List<AudioClip> TryGetAudioClipList()
    {
        return clipList;
    }
}
