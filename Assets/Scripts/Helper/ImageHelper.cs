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
        // Cache to store loaded images so we don't read from disk twice
        private static readonly Dictionary<string, Sprite> spriteCache = new();

        public static bool TryGetCachedSprite(string key, out Sprite sprite)
        {
            return spriteCache.TryGetValue(key, out sprite);
        }

        /// <summary>
        /// Loads an image from StreamingAssets (or Cache), applies it to the UI Image, and preserves aspect ratio.
        /// </summary>
        public static async Task LoadAndApplyImageAsync(string folderId, string imageURL, Image image)
        {
            if (image == null)
            {
                Debug.LogWarning("[ImageHelper] Target Image component is null. Aborting load.");
                return;
            }

            string cacheKey = $"{folderId}/{imageURL}";
            Debug.Log($"[ImageHelper] Requesting image: {cacheKey}");

            // 1. Check if we already have it in the cache
            if (!spriteCache.TryGetValue(cacheKey, out var sprite))
            {
                Debug.Log($"[ImageHelper] Image not in cache. Preloading: {cacheKey}");

                // 2. Preload from disk
                await PreloadImageAsync(folderId, imageURL);

                // 3. Try grabbing it from the cache again
                if (!spriteCache.TryGetValue(cacheKey, out sprite))
                {
                    Debug.LogError($"[ImageHelper] Final failure. Could not load or cache image: {cacheKey}");
                    return;
                }
            }
            else
            {
                Debug.Log($"[ImageHelper] Loaded successfully from cache: {cacheKey}");
            }

            // 4. Apply to image and ensure aspect ratio is preserved (No stretching!)
            image.sprite = sprite;
            image.preserveAspect = true;
            Debug.Log($"[ImageHelper] Successfully applied sprite to Image component.");
        }

        /// <summary>
        /// Loads an image from disk using UnityWebRequest and stores it in the dictionary cache.
        /// </summary>
        public static async Task PreloadImageAsync(string folderId, string imageURL)
        {
            string cacheKey = $"{folderId}/{imageURL}";

            if (spriteCache.ContainsKey(cacheKey))
            {
                Debug.Log($"[ImageHelper] Preload skipped, already cached: {cacheKey}");
                return;
            }

            string filePath = Path.Combine(Application.streamingAssetsPath, folderId, imageURL);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[ImageHelper] File does not exist at path: {filePath}");
                return;
            }

            string uri = "file://" + filePath;
            Debug.Log($"[ImageHelper] Starting download from URI: {uri}");

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
            {
                var operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ImageHelper] WebRequest failed for {uri}. Error: {uwr.error}");
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                if (texture == null)
                {
                    Debug.LogError($"[ImageHelper] Failed to extract texture from WebRequest: {uri}");
                    return;
                }

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                spriteCache[cacheKey] = sprite;
                Debug.Log($"[ImageHelper] Successfully cached sprite: {cacheKey}");
            }
        }

        /// <summary>
        /// Call this when exiting a gallery or switching projects to free up RAM.
        /// </summary>
        public static void ClearCache()
        {
            Debug.Log($"[ImageHelper] Clearing {spriteCache.Count} cached sprites to free memory.");

            foreach (var kvp in spriteCache)
            {
                if (kvp.Value != null)
                {
                    // Destroy both the sprite and its underlying texture to prevent RAM leaks
                    if (kvp.Value.texture != null) Object.Destroy(kvp.Value.texture);
                    Object.Destroy(kvp.Value);
                }
            }

            spriteCache.Clear();
        }
    }
}