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
    
    public void setTextToSpeech(bool value){
        textToSpeechEnabled = value;
    }
    public void setFiftyMoveRule(bool value){
        fiftyMoveRuleEnabled = value;
    }
    public void setAnimationTime(float value, int buttonIndex){
        animationTime = value;
        foreach(Button button in animationSpeedButtons){
            button.interactable = true;
        }
        animationSpeedButtons[buttonIndex].interactable=false;
    }

}
