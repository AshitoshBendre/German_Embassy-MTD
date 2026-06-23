using System.Collections;
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
        private static ImageDispatcher _dispatcher;

        // Lazy-load a hidden GameObject to run our Main-Thread Coroutine Queue
        private static ImageDispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                {
                    var go = new GameObject("[ImageHelper_FrameDispatcher]");
                    Object.DontDestroyOnLoad(go);
                    _dispatcher = go.AddComponent<ImageDispatcher>();
                }
                return _dispatcher;
            }
        }

        public static bool TryGetCachedSprite(string key, out Sprite sprite)
            => spriteCache.TryGetValue(key, out sprite);

        public static async Task LoadAndApplyImageAsync(string folderId, string imageURL, Image image)
        {
            if (image == null) return;

            string cacheKey = $"{folderId}/{imageURL}";

            if (!spriteCache.TryGetValue(cacheKey, out Sprite sprite))
            {
                sprite = await PreloadImageAsync(folderId, imageURL);
            }

            // Safety check: The UI Object might have been destroyed while we were reading from the SSD!
            if (sprite != null && image != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
        }

        public static Task<Sprite> PreloadImageAsync(string folderId, string imageURL)
        {
            string cacheKey = $"{folderId}/{imageURL}";

            if (spriteCache.TryGetValue(cacheKey, out Sprite existing))
                return Task.FromResult(existing);

            string filePath = Path.Combine(Application.streamingAssetsPath, folderId, imageURL);
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[ImageHelper] File missing: {filePath}");
                return Task.FromResult<Sprite>(null);
            }

            // file:/// formats absolute Windows drive paths correctly (e.g. file:///C:/Project/...)
            string uri = "file:///" + filePath.Replace("\\", "/");
            return Dispatcher.Enqueue(uri, cacheKey, spriteCache);
        }

        // Added specifically to fix your DashboardViewer's absolute paths
        public static Task<Sprite> LoadSpriteFromAbsolutePathAsync(string absoluteFilePath)
        {
            string cacheKey = absoluteFilePath;
            if (spriteCache.TryGetValue(cacheKey, out Sprite existing))
                return Task.FromResult(existing);

            if (!File.Exists(absoluteFilePath)) return Task.FromResult<Sprite>(null);

            string uri = "file:///" + absoluteFilePath.Replace("\\", "/");
            return Dispatcher.Enqueue(uri, cacheKey, spriteCache);
        }

        public static void ClearCache()
        {
            if (_dispatcher != null) _dispatcher.ClearQueue();

            foreach (var kvp in spriteCache)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.texture != null) Object.Destroy(kvp.Value.texture);
                    Object.Destroy(kvp.Value);
                }
            }
            spriteCache.Clear();
        }
    }

    // --- THE HIDDEN WORKER ---
    internal class ImageDispatcher : MonoBehaviour
    {
        private struct Job
        {
            public string uri;
            public string cacheKey;
            public TaskCompletionSource<Sprite> tcs;
            public Dictionary<string, Sprite> targetCache;
        }

        private readonly Queue<Job> _queue = new();
        private bool _isWorking = false;

        public Task<Sprite> Enqueue(string uri, string cacheKey, Dictionary<string, Sprite> cache)
        {
            var tcs = new TaskCompletionSource<Sprite>();
            _queue.Enqueue(new Job { uri = uri, cacheKey = cacheKey, tcs = tcs, targetCache = cache });

            if (!_isWorking) StartCoroutine(WorkLoop());
            return tcs.Task;
        }

        public void ClearQueue()
        {
            foreach (var job in _queue) job.tcs.TrySetResult(null);
            _queue.Clear();
        }

        private IEnumerator WorkLoop()
        {
            _isWorking = true;

            while (_queue.Count > 0)
            {
                var job = _queue.Dequeue();

                // If a duplicate UI request arrived while this sat in line, hand it the resolved sprite
                if (job.targetCache.TryGetValue(job.cacheKey, out Sprite cached))
                {
                    job.tcs.TrySetResult(cached);
                    continue;
                }

                using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(job.uri);
                yield return uwr.SendWebRequest(); // Native C++ background thread IO!

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    job.tcs.TrySetResult(null);
                    continue;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (tex != null)
                {
                    tex.Apply(false, true); // Instantly clears the CPU memory copy
                    Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                    job.targetCache[job.cacheKey] = s;
                    job.tcs.TrySetResult(s);
                }
                else
                {
                    job.tcs.TrySetResult(null);
                }

                // STRICT PACING: Force Unity to render the visual frame before doing the next image.
                yield return null;
            }

            _isWorking = false;
        }
    }
}