using UnityEngine;
using UnityLibrary;

[RequireComponent(typeof(AudioSource))]
public class SpeechController : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void SpeakText(string text)
    {
        Speech.instance.Say(text, TTSCallback);
    }

    public void StopSpeaking()
    {
        audioSource.Stop();

        Speech.instance.Stop();
    }

    private void TTSCallback(string message, AudioClip audio)
    {
        audioSource.clip = audio;
        audioSource.Play();
    }
}
