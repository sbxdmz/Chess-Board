using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTeam : MonoBehaviour
{
    public Sprite[] whiteSprites; 
    public Sprite[] blackSprites; 
    public Image[] renderers;

    public void SwitchToTeam(TeamColor team){
        if (team == TeamColor.Black){
            transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else{
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        for(int i = 0; i < renderers.Length; i++){
            if(team == TeamColor.White){
                renderers[i].sprite = whiteSprites[i]; 
            }
            else{
                renderers[i].sprite = blackSprites[i]; 
            }
        }
    }
}
