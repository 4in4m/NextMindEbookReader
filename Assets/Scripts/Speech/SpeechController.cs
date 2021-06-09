using UnityEngine;

public class SpeechController : MonoBehaviour
{
    private int _numVoice;
    private int _voiceStatus;
    private string _voiceName;

    private VoiceManager _voiceManager;

    private void Start()
    {
        _voiceManager = FindObjectOfType<VoiceManager>();

        _voiceStatus = _voiceManager.Init();

        if (_voiceStatus != 1)
        {
            _voiceName = _voiceManager.VoiceNames[1];
        }
    }

    public void SpeakText(string text)
    {
        _voiceStatus = _voiceManager.Init();

        if (_voiceStatus != 1)
        {
            _voiceManager.Say(text);
        }
    }

    public void StopSpeaking()
    {
        if (_voiceStatus != 1)
        {
            _voiceStatus = 1;
            _voiceManager.StopAndClose();
        }
    }
}
