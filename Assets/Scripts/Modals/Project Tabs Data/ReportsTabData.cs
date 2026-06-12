using System;
using System.Collections.Generic;
[Serializable]
public class ReportsTabData
{
    // Lower Rect
    public List<ReportData> reportDatas;
    //public float idleTimeout = 10f;
}

[Serializable]
public class ReportData
{
    public List<String> textData;
    public string titleText;

    public bool IsInvalid()
    {
        if (string.IsNullOrWhiteSpace(titleText))
            return true;

        if (textData == null || textData.Count == 0)
            return true;

        foreach (string text in textData)
        {
            if (!string.IsNullOrWhiteSpace(text))
                return false;
        }

        return true;
    }
}
