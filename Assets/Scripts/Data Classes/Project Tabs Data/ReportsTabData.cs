using System;
using System.Collections.Generic;
[Serializable]
public class ReportsTabData
{
    // Lower Rect
    public List<ReportData> reportDatas;
}

[Serializable]
public class ReportData
{
    public string pdfURl;
    public string titleText;

    public bool IsInvalid() => string.IsNullOrWhiteSpace(pdfURl) || string.IsNullOrWhiteSpace(titleText);

}
