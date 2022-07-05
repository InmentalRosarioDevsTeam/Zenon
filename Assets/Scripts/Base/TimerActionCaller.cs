using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerActionCaller : MonoBehaviour
{
    public UnityEvent onTime;
    public float time = 3;

    private void OnEnable()
    {
        Invoke("OnTime", time);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void OnTime()
    {
        if(onTime!=null)
            onTime.Invoke();
        
    }
}
