using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoButtonUI : MonoBehaviour
{
    private Button btn;
    private VideoSectionView sectionView;
    private string videoURL;
    private string titleText;

    [SerializeField] private TMP_Text title;
    [SerializeField] private RawImage thumbnailImage; // Assign this in the Inspector

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    public void Initialize(VideoSectionView sectionView, string videoURL, string titleText, string absoluteThumbnailPath)
    {
        this.sectionView = sectionView;
        this.videoURL = videoURL;
        this.titleText = titleText;
        title.text = titleText;

        // Load and apply the thumbnail image
        if (thumbnailImage != null && File.Exists(absoluteThumbnailPath))
        {
            byte[] fileData = File.ReadAllBytes(absoluteThumbnailPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); // Auto-resizes the texture dimensions
            thumbnailImage.texture = tex;
        }
        else
        {
            Debug.LogWarning($"[VideoButtonUI] Could not load thumbnail at {absoluteThumbnailPath} or RawImage is not assigned.");
        }
    }

    public void OnClick()
    {
        if (string.IsNullOrWhiteSpace(videoURL) || sectionView == null) return;

        sectionView.ShowVideoOnMainRectRawImage(videoURL, titleText);
    }
}