using System;
using UnityEngine;

public class ScreenSaverMonitor : MonoBehaviour
{
    [Header("Settings")]
    public float idleTimeout = 15f;

    private float _lastInputTime;
    private Vector3 _LastMousePosition;

    private bool _isMediaPlaying =false;
    private bool _isScreensaverActive =false;

    public event Action OnScreensaverActivated;
    public event Action OnScreensaverDeactivated;
    
    private void Start()
    {
        WakeUp();
        _LastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        // If media is playing, do not process idle logic
        if (_isMediaPlaying) return;

        if (DetectGlobalInput()) // Checking Click Interaction or Draging
        {
            WakeUp();
            if (_isScreensaverActive)
            { 
                DeactivateScreenSaver(); // Disable Screensaver if active
            }
          
        }

        if(!_isScreensaverActive &&(Time.unscaledTime - _lastInputTime)> idleTimeout)
        {
            ActivateScreenSaver();
        }
    }

    private bool DetectGlobalInput()
    {
        if (Input.anyKey)
        {
            return true;
        }

        if(Input.mousePosition != _LastMousePosition)
        {
            _LastMousePosition= Input.mousePosition;
            return true;
        }
        return false;
    }

    private void ActivateScreenSaver()
    {
        _isScreensaverActive = true;
        OnScreensaverActivated?.Invoke();
    }

    private void DeactivateScreenSaver()
    {
        _isScreensaverActive = false;
        OnScreensaverDeactivated?.Invoke();
    }

    private void WakeUp()
    {
        _lastInputTime = Time.unscaledTime;
    }

    public void SetMediaPlayingState(bool isPlaying)
    {
        _isMediaPlaying=isPlaying;

        if (!isPlaying)
        {
            // If media Stops we still reset the timer to avoid the screensaver to show up if media stops
            WakeUp();
        }
        else if(_isScreensaverActive) // Safety Callback, where if the screensaver is active and showhow the video is played it will deactive the ScreenSaver
        {
            DeactivateScreenSaver();
        }
    }
}
