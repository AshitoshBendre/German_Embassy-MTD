using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleUIScreen : MonoBehaviour, IUIScreen
{
    [Header("Hide Only When Leaving Via Back")]
    [SerializeField] private List<GameObject> hideOnBack = new();
    [SerializeField] private List<GameObject> showOnBack = new();

    public void Show(bool instant = false)
    {
        gameObject.SetActive(true);
    }

    public void Hide(Action onComplete = null, bool instant = false)
    {
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public float GetTransitionDuration(bool instant = false) => 0f;

    public void SetInteractable(bool interactable) { }


    public void OnBackNavigation()
    {
        foreach (var obj in hideOnBack)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (var obj in showOnBack)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}