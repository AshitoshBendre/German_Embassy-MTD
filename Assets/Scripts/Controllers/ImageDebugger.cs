using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Helpers
{
    public class ImageDebuggerUI : MonoBehaviour
    {
        // Singleton so our static class can easily find it
        public static ImageDebuggerUI Instance;

        [Header("UI References")]
        [SerializeField] private TMP_Text debugText;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Settings")]
        [SerializeField] private int maxLines = 30; // Prevents the text box from lagging the game
        [SerializeField] private bool dontDestroyOnLoad = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject.transform.root);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (debugText != null) debugText.text = "ImageHelper On-Screen Debugger";
        }

        public void AddLog(string prefix, string message, string colorHex)
        {
            if (debugText == null) return;
            debugText.text = string.Empty;
            // Append the new log
            debugText.text += $"\n<color={colorHex}>{prefix} {message}</color>";

            // Prune old lines so the string doesn't get massive and cause lag
            string[] lines = debugText.text.Split('\n');
            if (lines.Length > maxLines)
            {
                debugText.text = string.Join("\n", lines, lines.Length - maxLines, maxLines);
            }

            // Force the scroll view to snap to the bottom
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}