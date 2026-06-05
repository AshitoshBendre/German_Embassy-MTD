using System;
using System.Collections.Generic;

[Serializable]
public class AboutTabData
{
    // Upper Rect
    public string imageURL;

    // Lower Rect
    public string factsTitle;
    public List<FactsData> factsDatas;
}

[Serializable]
public class FactsData 
{
    public string leftText;
    public string rightText;

    public bool IsInvalid() => string.IsNullOrWhiteSpace(leftText) || string.IsNullOrWhiteSpace(rightText);
}
