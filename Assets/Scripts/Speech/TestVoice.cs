// **************************************************************************
// Unity Text To Speech V2 ZJP. Test Voice
// **************************************************************************
using UnityEngine;

public class TestVoice : MonoBehaviour
{
    public int numVoice;
    public int voiceOK;
    public string voiceName;
    public VoiceManager vm;

    public string textesay;

    void Start()
    {
        vm = GetComponent<VoiceManager>();

        voiceOK = vm.Init();

        if (voiceOK != 1)
        {
            voiceName = vm.VoiceNames[1];
            Debug.Log("Voice number " + vm.VoiceNumber + " " + voiceName);
            textesay = "Hello. My name is Anna, and i am a woman. It's nice to meet you. Have a nice day and goodbye.";
            vm.Say(textesay);
            //vm.SayEX("Funny, it's the same voice again", 1 + 8);

            //textesay = "<voice required='Name=Bernard'> bonjour, je suis Bernard. Léger, là-bas, expliqué <voice required='Name=Juliette'> Et moi, je me nome Juliette.";
            //vm.Say(textesay);
        }
    }

    void Update()
    {
        if (voiceOK != 1 && vm.Status(0) == 2) // a speech is running
        {
            Debug.Log(" Total Stream  > " + vm.Status(2));
            Debug.Log(" Actual stream <<<<<<<<<<<<<<<<<<<<<<<<<<<<<> " + vm.Status(3));
            Debug.Log(" Position of the actual spoken word in the actual stream > " + vm.Status(1));
        }
    }

}