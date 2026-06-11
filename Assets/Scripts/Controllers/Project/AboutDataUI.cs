using System.Collections;
using TMPro;
using UnityEngine;

public class AboutDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    public void Initialize(string data)
    {
        text.text = data;
    }
}