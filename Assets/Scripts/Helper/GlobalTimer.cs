using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// This Global Timer Utilizes the Coroutines with The Null Monobehaviour Injection Hack
/// We Initialize the Null Monobehaviour which is required for the Coroutines, as Coroutines
/// Needs to be Tethered to an active GameObject in the scene via a Monobehaviour
/// </summary>
public static class GlobalTimer
{
    /// We cannot Add MonoBehaviour Component Directly via gameobject.AddComponent<MonoBehaviour>()
    /// That's why we need a empty holder monobehaviour inherited class as a workaround
    private class TimerRunner : MonoBehaviour { }

    private static TimerRunner _runner;

    private static void Initialize()
    {
        if (_runner == null)
        {
            GameObject runnerObject = new GameObject("[GlobalTimer_Runner]");
            _runner = runnerObject.AddComponent<TimerRunner>();
        }

    }

    public static Coroutine Start(float duration, Action onComplete)
    {
        Initialize();
        return _runner.StartCoroutine(CountdownRoutine(duration, onComplete));
    }

    /// <summary>
    /// This stops the given Coroutine 
    /// But Not Recommended As it needs to lookup in the active coroutines and find the given one to stop 
    /// [ O (n) ] as internally it loops to active coroutines 
    /// </summary>
    /// <param name="timerCoroutine"></param>
    public static void Stop(Coroutine timerCoroutine)
    {
        if(_runner !=null && timerCoroutine!= null)
        {
            _runner.StopCoroutine(timerCoroutine);
        }
    }

    private static IEnumerator CountdownRoutine(float duration, Action onComplete)
    {
        float remaintingTime = duration;
        while (remaintingTime > 0f)
        {
            remaintingTime -= Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
    }
}