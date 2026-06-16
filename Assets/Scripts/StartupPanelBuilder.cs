using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
public class StartupPanelBuilder : MonoBehaviour
{

    [SerializeField] private PanelButtonUI[] _panelButtons;

    [SerializeField] private StreamingAssetsLoader _loader;

    private async void Start()
    {
        List<PanelContext> panels = await _loader.LoadStartupPanelsAsync();

        for (int i = 0; i < _panelButtons.Count(); i++)
        {
            if (panels[i].Data.IsInvalid()) continue;

            _panelButtons[i].gameObject.SetActive(true);
            _panelButtons[i].Initialize(panels[i]);
        }
    }
}