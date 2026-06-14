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
    public string pdfURL;
    public string titleText; // Added titleText

    // Validate that neither the URL nor the title are missing
    public bool IsInvalid() => string.IsNullOrWhiteSpace(pdfURL) || string.IsNullOrWhiteSpace(titleText);
}