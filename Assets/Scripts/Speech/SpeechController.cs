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

            Debug.Log("Voice number " + _voiceManager.VoiceNumber + " " + _voiceName);
        }
    }

    void Update()
    {
        if (_voiceStatus != 1 && _voiceManager.Status(0) == 2) // a speech is running
        {
            Debug.Log(" Total Stream  > " + _voiceManager.Status(2));
            Debug.Log(" Actual stream <<<<<<<<<<<<<<<<<<<<<<<<<<<<<> " + _voiceManager.Status(3));
            Debug.Log(" Position of the actual spoken word in the actual stream > " + _voiceManager.Status(1));
        }
    }

    public void SpeakText(string text)
    {
        _voiceStatus = _voiceManager.Init();

        if (_voiceStatus != 1)
        {
            Debug.Log("Voice number " + _voiceManager.VoiceNumber + " " + _voiceName);

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
