using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PanelButtonUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _panelCoverImage;
    [SerializeField] private HorizontalListBuilder _horizontalListBuilder;
    [SerializeField] private UIManager _manager;
    private string _myFolderId;

    public void Initialize(PanelContext context)
    {
        _myFolderId = context.FolderId;

        _titleText.text = context.Data.titleText;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _horizontalListBuilder?.BuildProjectList(_myFolderId,_manager));

        ImageHelper.LoadAndApplyImageAsync(context.FolderId, context.Data.imageURL, _panelCoverImage);
    }
}
