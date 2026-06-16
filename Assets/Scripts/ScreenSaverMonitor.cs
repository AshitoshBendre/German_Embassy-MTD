using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ScreenSaverConfig
{
    public float idleTimeout;
}

public class ScreenSaverMonitor : MonoBehaviour
{
    [Header("Settings")]
    public float idleTimeout = 15f;
    public string configFileName = "ScreenSaverConfig.json";

    private Vector3 _lastMousePosition;
    private bool _isMediaPlaying = false;

    // Track the active coroutine from the GlobalTimer so we can stop/reset it
    private Coroutine _idleTimerCoroutine;

    private void Start()
    {
        LoadConfig();
        _lastMousePosition = Input.mousePosition;

        // Start the initial countdown
        WakeUp();
    }

    private void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, configFileName);

        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                ScreenSaverConfig configData = JsonUtility.FromJson<ScreenSaverConfig>(jsonString);

                if (configData != null && configData.idleTimeout > 0)
                {
                    idleTimeout = configData.idleTimeout;
                    Debug.Log($"<color=#00FF00>[ScreenSaver]</color> Loaded timeout from JSON: {idleTimeout} seconds.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=#FF0000>[ScreenSaver]</color> Failed to read JSON config: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"<color=#FFA500>[ScreenSaver]</color> No JSON found at {filePath}. Using default timeout: {idleTimeout} seconds.");
        }
    }

    private void Update()
    {
        // If media is playing, ignore all idle logic completely
        if (_isMediaPlaying) return;

        // If the user moves the mouse or clicks, reset the timer
        if (DetectGlobalInput())
        {
            WakeUp();
        }
    }

    private bool DetectGlobalInput()
    {
        if (Input.anyKey)
        {
            return true;
        }

        if (Input.mousePosition != _lastMousePosition)
        {
            _lastMousePosition = Input.mousePosition;
            return true;
        }

        return false;
    }

    private void WakeUp()
    {
        // 1. Stop the currently running timer (if there is one)
        if (_idleTimerCoroutine != null)
        {
            GlobalTimer.Stop(_idleTimerCoroutine);
        }

        // 2. Start a fresh timer. If it reaches the end, it will trigger RestartCurrentScene
        if (!_isMediaPlaying)
        {
            _idleTimerCoroutine = GlobalTimer.Start(idleTimeout, RestartCurrentScene);
        }
    }

    public void SetMediaPlayingState(bool isPlaying)
    {
        _isMediaPlaying = isPlaying;

        if (!isPlaying)
        {
            // Media stopped, start tracking idle time again
            WakeUp();
        }
        else
        {
            // Media started, stop the idle timer so it doesn't trigger in the background
            if (_idleTimerCoroutine != null)
            {
                GlobalTimer.Stop(_idleTimerCoroutine);
                _idleTimerCoroutine = null;
            }
        }
    }

    private void RestartCurrentScene()
    {
        Debug.Log("<color=#00FFFF>[ScreenSaver]</color> Idle timeout reached. Restarting Scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}