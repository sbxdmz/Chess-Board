using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwapper : MonoBehaviour
{
    public GameObject[] ModelList;
    public void SetModel(int index){
        for(int i = 0; i < ModelList.Length; i++){
            if(index == i){
                ModelList[i].SetActive(true);
                
            }
            else{
                ModelList[i].SetActive(false);
            }
        }

    }
}
