using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeHide : MonoBehaviour
{
    public CanvasGroup cg;
    private void Awake()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void reset()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
