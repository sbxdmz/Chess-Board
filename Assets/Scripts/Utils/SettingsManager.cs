using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager main;
    public bool textToSpeechEnabled;
    public bool fiftyMoveRuleEnabled;
    public float animationTime = 0.5f;
    public Button[] animationSpeedButtons;
    void Awake(){
        main = this;
    }

    private void Start()
    {
        setAnimationTime(1);
    }
    public void setTextToSpeech(bool value){
        textToSpeechEnabled = value;
    }
    public void setFiftyMoveRule(bool value){
        fiftyMoveRuleEnabled = value;
    }
    public void setAnimationTime(int buttonIndex){
        switch(buttonIndex){
            case 0: animationTime = 0f; break;
            case 1: animationTime = 0.1f; break;
            case 2: animationTime = 0.3f; break;
            case 3: animationTime = 0.5f; break;
            default: animationTime = -1; break;
        }
        foreach(Button button in animationSpeedButtons){
            button.interactable = true;
        }
        animationSpeedButtons[buttonIndex].interactable=false;
    }

}
