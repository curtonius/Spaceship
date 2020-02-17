using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager current;

    SoundManager()
    {
        current = this;
    }

    public List<AudioClip> Sounds = new List<AudioClip>();
    private Dictionary<string, AudioSource> channels = new Dictionary<string, AudioSource>();
    private Dictionary<string, bool> soundsBeingFaded = new Dictionary<string, bool>();

    private void Start()
    {
        foreach(AudioClip audioClip in Sounds)
        {
            AudioSource channel = gameObject.AddComponent<AudioSource>();
            channel.playOnAwake = false;
            channel.clip = audioClip;
            channels.Add(audioClip.name, channel);
        }
    }

    public void PlaySound(string soundName)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].Play();
    }
    public void StopSound(string soundName)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].Stop();
    }
    public void PauseSound(string soundName)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].Pause();
    }

    public void ChangeVolume(string soundName, float setToAmount)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].volume = setToAmount;
    }
    public void ChangePitch(string soundName, float setToAmount)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].pitch = setToAmount;
    }
    public void ChangePan(string soundName, float setToAmount)
    {
        if (channels.ContainsKey(soundName))
            channels[soundName].panStereo = setToAmount;
    }

    public bool IsSoundPlaying(string soundName)
    {
        if (!channels.ContainsKey(soundName))
            return false;
        else
            return channels[soundName].isPlaying;
    }

    public void Fade(string soundName1, string soundName2, float fadeSpeed, float volume)
    {
        if (channels.ContainsKey(soundName1) && !soundsBeingFaded.ContainsKey(soundName1))
        {
            soundsBeingFaded.Add(soundName1, true);
            StartCoroutine(FadeIn(soundName1, fadeSpeed, volume));
        }
        if (channels.ContainsKey(soundName2) && !soundsBeingFaded.ContainsKey(soundName2))
        {
            soundsBeingFaded.Add(soundName2, true);
            StartCoroutine(FadeOut(soundName2, fadeSpeed));
        }
    }
    public void FadeInAlone(string soundName, float fadeSpeed, float volume)
    {
        if (channels.ContainsKey(soundName) && !soundsBeingFaded.ContainsKey(soundName))
        {
            soundsBeingFaded.Add(soundName, true);
            StartCoroutine(FadeIn(soundName, fadeSpeed, volume));
        }
    }
    public void FadeOutAlone(string soundName, float fadeSpeed)
    {
        if (channels.ContainsKey(soundName) && !soundsBeingFaded.ContainsKey(soundName))
        {
            soundsBeingFaded.Add(soundName, true);
            StartCoroutine(FadeOut(soundName, fadeSpeed));
        }
    }

    IEnumerator FadeIn(string soundName, float fadeSpeed, float volume)
    {
        channels[soundName].volume = 0;
        float t = 0;

        while(t < fadeSpeed)
        {
            t += Time.deltaTime;
            channels[soundName].volume = volume * (t / fadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        soundsBeingFaded.Remove(soundName);
        yield return null;
    }
    IEnumerator FadeOut(string soundName, float fadeSpeed)
    {
        float volume = channels[soundName].volume;
        float t = fadeSpeed;

        while (t > 0)
        {
            t -= Time.deltaTime;
            channels[soundName].volume = volume * (t / fadeSpeed);
            yield return new WaitForEndOfFrame();
        }
        soundsBeingFaded.Remove(soundName);
        yield return null;
    }
}
