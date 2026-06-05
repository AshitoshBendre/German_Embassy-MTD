using System;

[Serializable]
public class PanelData
{
    public string imageURL { get; set;  }
    public string titleText { get; set; }

    public bool IsInvalid() => string.IsNullOrWhiteSpace(imageURL) || string.IsNullOrWhiteSpace(titleText);
}
