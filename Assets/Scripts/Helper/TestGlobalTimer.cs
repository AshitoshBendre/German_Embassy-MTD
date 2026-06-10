using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGlobalTimer : MonoBehaviour
{
    private void Start()
    {
        GlobalTimer.Start(2f, () => Debug.Log("Fast Timer Completed"));
        GlobalTimer.Start(2f, () => Debug.Log("Fast Timer 2 Completed"));
        GlobalTimer.Start(5f, () => Debug.Log("Medium Timer Completed"));
        GlobalTimer.Start(10f, () => Debug.Log("long Timer Completed"));
    }
}
