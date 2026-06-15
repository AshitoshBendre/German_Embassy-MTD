using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Helpers
{
    public static class ImageHelper
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new();
        public static bool TryGetCachedSprite(string key, out Sprite sprite)
        {
            return spriteCache.TryGetValue(key, out sprite);
        }
        /*public static async Task LoadAndApplyImageAsync(string folderId, string imageURL, Image image)
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
        }*/
        /*public static async Task LoadAndApplyImageAsync(
    string folderId,
    string imageURL,
    Image image)
        {
            string filePath = Path.Combine(
                Application.streamingAssetsPath,
                folderId,
                imageURL);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Image not found: {filePath}");
                return;
            }

            byte[] bytes;

            try
            {
                bytes = await Task.Run(() => File.ReadAllBytes(filePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to read image: {filePath}\n{e}");
                return;
            }

            if (image == null)
                return;

            Texture2D texture = new Texture2D(2, 2);

            if (!texture.LoadImage(bytes))
            {
                Debug.LogError($"Failed to decode image: {filePath}");
                Object.Destroy(texture);
                return;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            image.sprite = sprite;
        }
    }*/
        public static async Task LoadAndApplyImageAsync(
    string folderId,
    string imageURL,
    Image image)
        {
            string cacheKey = $"{folderId}/{imageURL}";

            if (!spriteCache.TryGetValue(cacheKey, out var sprite))
            {
                await PreloadImageAsync(folderId, imageURL);

                if (!spriteCache.TryGetValue(cacheKey, out sprite))
                    return;
            }

            image.sprite = sprite;
        }
        public static async Task PreloadImageAsync(string folderId, string imageURL)
        {
            string cacheKey = $"{folderId}/{imageURL}";

            if (spriteCache.ContainsKey(cacheKey))
                return;

            string filePath = Path.Combine(
                Application.streamingAssetsPath,
                folderId,
                imageURL);

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
                    Debug.LogError($"Failed to preload image: {cacheKey}");
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                spriteCache[cacheKey] = sprite;
            }
        }
    }
}
