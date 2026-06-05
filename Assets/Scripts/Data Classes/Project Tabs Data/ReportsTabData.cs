using System;
using System.Collections.Generic;

public class ReportsTabData
{
    // Lower Rect
    public List<ReportData> reportDatas { get; set; }
}

[Serializable]
public class ReportData
{
    public string pdfURl { get; set; }
    public string titleText { get; set; }

    public bool IsInvalid() => string.IsNullOrWhiteSpace(pdfURl) || string.IsNullOrWhiteSpace(titleText);

}
