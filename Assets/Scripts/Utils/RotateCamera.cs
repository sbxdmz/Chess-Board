using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public bool whiteSide = true;
    public GameObject whiteSideLabels;
    public GameObject blackSideLabels;
    float velocity = 0;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)){
            ToggleSide();
        }
        float targetAngle = whiteSide ? 0 : 180;
        float camRotation = Camera.main.transform.localEulerAngles.y;
        Camera.main.transform.localEulerAngles = new Vector3(90, Mathf.SmoothDamp(camRotation, targetAngle, ref velocity, 0.1f), 0);
    }
    
    public void ToggleSide(){
        whiteSide = !whiteSide;
        whiteSideLabels.SetActive(whiteSide);
        blackSideLabels.SetActive(!whiteSide);
    }


}