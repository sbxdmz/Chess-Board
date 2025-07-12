using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceMover : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 vel;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, SettingsManager.main.animationTime);
    }

    
    public void SetTargetPosition(Vector3 newTarget){
        targetPosition = newTarget;
        transform.position += Vector3.up * 0.1f; 
    }
}
