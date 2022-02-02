using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    public DestinationPanelView DestinationPanelView;
    [SerializeField]
    public GameObject LoaderPanel;
    [SerializeField]
    public GameObject LogoPanel;

    [SerializeField]
    private PinManager PinManager;

    private void Awake()
    {
        AppStateChangedEvent += OnAppStateChanged;
        PinManager.PinClickedEvent += OnPinClicked;
        DestinationPanelView.SetEnableUI(false);
        LogoPanel.SetActive(false);
        LoaderPanel.SetActive(false);
        DestinationPanelView.SearchStartedEvent += EnableLoaderPanel;
        DestinationPanelView.SearchEndedEvent += DisableLoaderPanel;
    }

    private void OnDestroy()
    {
        AppStateChangedEvent -= OnAppStateChanged;
        PinManager.PinClickedEvent -= OnPinClicked;
        DestinationPanelView.SearchStartedEvent -= EnableLoaderPanel;
        DestinationPanelView.SearchEndedEvent -= DisableLoaderPanel;
    }

    private void DisableLoaderPanel() => SetEnableLoaderPanel(false);
    private void EnableLoaderPanel() => SetEnableLoaderPanel(true);

    public void SetEnableLoaderPanel(bool isEnabled, string addInfo = null)
    {
        Debug.Log("Loader isActive:" + isEnabled);

        if(addInfo != null)
        {
            //TODO
        }

        LoaderPanel.SetActive(isEnabled);
    } 
    
    public void SetEnableLogoPanel(bool isEnabled)
    {
        LogoPanel.SetActive(isEnabled);
    }

    private void OnPinClicked(AirportData airportData)
    {
        DestinationPanelView.OnPinClicked(airportData);
    }
    private void OnAppStateChanged(AppState appState)
    {
        EnableUI(appState == AppState.Running);
    }

    private void EnableUI(bool isEnabled)
    {
        DestinationPanelView.SetEnableUI(isEnabled);
    }
}
