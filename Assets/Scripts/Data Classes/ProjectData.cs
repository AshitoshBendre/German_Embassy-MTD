using System;

[Serializable]
public class ProjectData
{
    public string projectTitle { get; set;  }

    public AboutTabData aboutTabData { get; set; }
    public VideosTabData videosTabData { get; set; }
    public GalleryTabData galleryTabData { get; set; }
    public DashboardTabData dashboardTabData {  get; set; }
    public ReportsTabData reportsTabData {  get; set; }
}
