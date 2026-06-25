using UnityEngine;
using UnityEngine.UI;

public class ButtonExtension : MonoBehaviour
{
    // Call this from a standard, parameterless UnityEvent
    public void TriggerClick(Button targetButton)
    {
        if (targetButton != null && targetButton.interactable)
        {
            targetButton.onClick.Invoke();
        }
    }
}