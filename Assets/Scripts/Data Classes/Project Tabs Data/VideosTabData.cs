using System;
using System.Collections.Generic;

[Serializable]
public class VideosTabData 
{
    // Lower Rect
    public List<VideoData> videoDatas;
}

[Serializable]
public class VideoData
{
    public string videoURL;
    public string titleText;

    public bool IsInvalid() => string.IsNullOrWhiteSpace(videoURL) || string.IsNullOrWhiteSpace(titleText);
}