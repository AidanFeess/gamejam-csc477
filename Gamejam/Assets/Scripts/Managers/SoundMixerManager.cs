using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    // Math equation below converts linear interpolation of volume to logarithmic interpolation
    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }
}
