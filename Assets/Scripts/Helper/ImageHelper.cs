using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Helpers
{
    public static class ImageHelper
    {
        public static async Task LoadAndApplyImageAsync(string folderId, string imageURL, Image image)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, folderId, imageURL);
            string uri = "file://" + filePath;

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
            {
                var operation = uwr.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to load image"+ folderId+" "+ imageURL);
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite newSprite = Sprite.Create(
                    texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                image.sprite = newSprite;
            }
        }
    }
}
