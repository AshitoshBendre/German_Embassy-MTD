using System;
using System.Collections.Generic;

[Serializable]
public class VideosTabData
{
    // Lower Rect
    public List<VideoData> videoDatas;
    public float idleTimeout = 2f;
}

[Serializable]
public class VideoData
{
    public string videoURL;
    public string titleText;
    public string thumbnailURL; // Added thumbnail support

    // Validate that all three required fields exist
    public bool IsInvalid() =>
        string.IsNullOrWhiteSpace(videoURL) ||
        string.IsNullOrWhiteSpace(titleText) ||
        string.IsNullOrWhiteSpace(thumbnailURL);
}