using System;
using System.Collections.Generic;

[Serializable]
public class GalleryTabData 
{
    // Lower Rect
    public List<GalleryData> galleryDatas;
}

[Serializable]
public class GalleryData
{
    public string imageURL;
    public string titleText;

    public bool IsInvalid() => string.IsNullOrWhiteSpace(imageURL) || string.IsNullOrWhiteSpace(titleText);
}