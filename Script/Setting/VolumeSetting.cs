using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider voiceSlider;

    private const float audioSetIndex = 0.1f; // Step for Increase and Decrease

    public void StartSetVolume()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetAllVolumes();
        }
    }

    private void AdjustVolume(Slider slider, Action setVolumeAction, bool increase)
    {
        slider.value = Mathf.Clamp(slider.value + (increase ? audioSetIndex : -audioSetIndex), 0f, 1f);
        setVolumeAction.Invoke();
    }

    public void IncreaseMasterVolume() => AdjustVolume(masterSlider, SetMasterVolume, true);
    public void DecreaseMasterVolume() => AdjustVolume(masterSlider, SetMasterVolume, false);

    public void IncreaseMusicVolume() => AdjustVolume(musicSlider, SetMusicVolume, true);
    public void DecreaseMusicVolume() => AdjustVolume(musicSlider, SetMusicVolume, false);

    public void IncreaseSFXVolume() => AdjustVolume(soundSlider, SetSoundVolume, true);
    public void DecreaseSFXVolume() => AdjustVolume(soundSlider, SetSoundVolume, false);

    public void IncreaseVoiceVolume() => AdjustVolume(voiceSlider, SetVoiceVolume, true);
    public void DecreaseVoiceVolume() => AdjustVolume(voiceSlider, SetVoiceVolume, false);

    public void SetMasterVolume()
    {
        SetVolume("master", masterSlider.value, "masterVolume");
    }

    public void SetMusicVolume()
    {
        SetVolume("music", musicSlider.value, "musicVolume");
    }

    public void SetSoundVolume()
    {
        SetVolume("SFX", soundSlider.value, "SFXVolume");
    }

    public void SetVoiceVolume()
    {
        SetVolume("voice", voiceSlider.value, "voiceVolume");
    }

    private void SetVolume(string parameterName, float sliderValue, string playerPrefKey)
    {
        audioMixer.SetFloat(parameterName, Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat(playerPrefKey, sliderValue);
    }

    private void SetAllVolumes()
    {
        SetMasterVolume();
        SetMusicVolume();
        SetSoundVolume();
        SetVoiceVolume();
    }

    public void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        soundSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        voiceSlider.value = PlayerPrefs.GetFloat("voiceVolume", 0.5f);

        SetAllVolumes();
    }
}
