using System.Collections;
using TMPro;
using UnityEngine;

public class FactRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _leftText;
    [SerializeField] private TMP_Text _rightText;

    public void Initialize(string left, string right)
    {
        if (_leftText != null)
        {
            _leftText.text = left;
        }
        else
        {
            Debug.LogWarning("[FactRowUI] Left text component is missing on prefab.");
        }

        if (_rightText != null)
        {
            _rightText.text = right;
        }
        else
        {
            Debug.LogWarning("[FactRowUI] Right text component is missing on prefab.");
        }
    }
}