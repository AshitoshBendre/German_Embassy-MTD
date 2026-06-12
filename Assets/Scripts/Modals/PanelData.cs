using System;

[Serializable]
public class PanelData
{
    public string imageURL;
    public string mapURL;
    public string titleText;

    public bool IsInvalid() => string.IsNullOrWhiteSpace(imageURL) || string.IsNullOrWhiteSpace(titleText);
}
