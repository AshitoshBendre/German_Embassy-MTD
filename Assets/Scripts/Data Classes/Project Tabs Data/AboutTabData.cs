using System;
using System.Collections.Generic;

[Serializable]
public class AboutTabData
{
    // Upper Rect
    public string imageURL {  get; set; }

    // Lower Rect
    public string factsTitle { get; set; }
    public List<FactsData> factsDatas { get; set; }
}

[Serializable]
public class FactsData 
{
    public string leftText { get; set; }
    public string rightText { get; set; }

    public bool IsInvalid() => string.IsNullOrWhiteSpace(leftText) || string.IsNullOrWhiteSpace(rightText);
}
