using System;
using System.Collections.Generic;

[Serializable]
public class VideosTabData 
{
    // Lower Rect
    public List<VideoData> videoDatas {  get; set; }
}

[Serializable]
public class VideoData
{
    public string videoURL { get; set; }
    public string titleText { get; set; }

    public bool IsInvalid() => string.IsNullOrWhiteSpace(videoURL) || string.IsNullOrWhiteSpace(titleText);
}