using System;
using System.Collections.Generic;

[Serializable]
public class GalleryTabData 
{
    // Lower Rect
    public List<GalleryData> galleryDatas {  get; set; }
}

[Serializable]
public class GalleryData
{
    public string imageURL { get; set; }
    public string titleText { get; set; }

    public bool IsInvalid() => string.IsNullOrWhiteSpace(imageURL) || string.IsNullOrWhiteSpace(titleText);
}