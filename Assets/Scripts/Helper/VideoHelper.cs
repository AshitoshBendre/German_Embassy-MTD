
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers
{
    [Obsolete]
    public static class VideoHelper
    {
        public static async Task LoadAndPlayImage(string folderId, string videoURL, RawImage image)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, folderId, videoURL);
        }
    }
}