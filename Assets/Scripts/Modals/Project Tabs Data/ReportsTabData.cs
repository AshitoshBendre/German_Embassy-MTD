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

    public bool IsInvalid() => textData.Count==0 || string.IsNullOrWhiteSpace(titleText);

}
