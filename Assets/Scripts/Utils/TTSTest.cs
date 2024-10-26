using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeechLib;
public class TTSTest : MonoBehaviour
{
    SpVoice voice = new SpVoice();
    void Start()
    {
        voice.Speak("Bishop B5", SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
