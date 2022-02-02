using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DropdownSearch;

public class DestinationPanelView : MonoBehaviour
{
    public delegate void OnPanelToggled(bool isOpen);
    public static event OnPanelToggled PanelToggledEvent;

    public delegate void OnSearchStarted();
    public event OnSearchStarted SearchStartedEvent;

    public delegate void OnSearchEnded();
    public event OnSearchEnded SearchEndedEvent;

    [SerializeField]
    private RectTransform ContainerRectTransform;
    [SerializeField]
    private RectTransform ContentRectTransform;

    [SerializeField]
    private DropdownSearch2D DestinationDropdownSearch, StartDropdownSearch;
    [SerializeField]
    private Button HideToggleButton, SearchButton;
    [SerializeField]
    private Image HideToggleIcon;

    [SerializeField]
    private float TranslateSpeed = 2f;

    [SerializeField]
    private GameObject ListPathsPanel;

    [SerializeField]
    private Transform ListPathsContentPanel;

    [SerializeField]
    private TMP_Text ListTitleText;

    [SerializeField]
    private TMP_Text NotPathsFound;

    [SerializeField]
    private Ticket TicketPrefab;

    [SerializeField]
    private MultipleTicket MultipleTicketPrefab;

    [SerializeField]
    private Transform LightTransform;

    [SerializeField]
    private Slider TimeSlider;

    [SerializeField]
    private bool isOpen = false;
    private Coroutine hideCoroutine;
    private Vector2 targetAnchoredPosition;

    private AirportData SelectedStartAirport => (AirportData)StartDropdownSearch.SelectedOption.ExtraData;
    private AirportData SelectedDestinationAirport => (AirportData)DestinationDropdownSearch.SelectedOption.ExtraData;

    public TaskCompletionSource<bool> ReadyTask = new TaskCompletionSource<bool>();

    private Vector2 closedPanelPosition;

    #region UnityMessages
    private void Awake()
    {
        SearchButton.interactable = false;
        StartDropdownSearch.OnItemSelected += OnAirportSelected;
        DestinationDropdownSearch.OnItemSelected += OnAirportSelected;
        StartDropdownSearch.OnItemDeselected += OnAirportDeselected;
        DestinationDropdownSearch.OnItemDeselected += OnAirportDeselected;
        SearchButton.onClick.AddListener(SearchSelectedFlight);
        closedPanelPosition = new Vector2(ContentRectTransform.sizeDelta.x, 0);
        ContainerRectTransform.anchoredPosition = closedPanelPosition;

        NotPathsFound.gameObject.SetActive(false);
        ListPathsPanel.SetActive(false);
        ListTitleText.gameObject.SetActive(false);

        TimeSlider.onValueChanged.AddListener(OnSliderValueChange);
    }

    private void Start()
    {
        if (PinManager.Instance.DeserializeTask.IsCompleted)
        {
            PopulateDropdowns(PinManager.Instance.DeserializeTask.Result);
        }
        else
        {
            Debug.LogError("Await");
            PopulateDropdowns(new List<AirportData>());
        }
        ReadyTask.TrySetResult(true);
    }


    private void OnDestroy()
    {
        StartDropdownSearch.OnItemSelected -= OnAirportSelected;
        DestinationDropdownSearch.OnItemSelected -= OnAirportSelected;
        StartDropdownSearch.OnItemDeselected -= OnAirportDeselected;
        DestinationDropdownSearch.OnItemDeselected -= OnAirportDeselected;
        SearchButton.onClick.RemoveListener(SearchSelectedFlight);
        TimeSlider.onValueChanged.RemoveListener(OnSliderValueChange);
    }

    private void OnEnable()
    {
        HideToggleButton.onClick.AddListener(ToggleHide);
    }

    private void OnDisable()
    {
        HideToggleButton.onClick.RemoveListener(ToggleHide);
    }

    #endregion

    #region AirportSelection

    private void OnAirportDeselected()
    {
        EnableSearchButton(false);
    }

    private void OnAirportSelected(int _)
    {
        int startIndex = StartDropdownSearch.SelectedIndex;
        int destinationIndex = DestinationDropdownSearch.SelectedIndex;
        if (startIndex > -1 && destinationIndex > -1 && startIndex != destinationIndex)
        {
            EnableSearchButton(true);
        }
    }

    internal void OnPinClicked(AirportData airportData)
    {
        ToggleHide(true);
        if (StartDropdownSearch.SelectedIndex <= -1)
        {
            StartDropdownSearch.SelectOption(airportData.Name);
        }
        else
        {
            DestinationDropdownSearch.SelectOption(airportData.Name);
        }
    }

    #endregion

    #region UIEnable&Toggles

    private void EnableSearchButton(bool isEnabled)
    {
        SearchButton.interactable = isEnabled;
    }
    private void EnableHideButton(bool isEnabled)
    {
        //HideToggleButton.gameObject.SetActive(isEnabled);
    }

    public void SetEnableUI(bool isEnabled)
    {
        ContainerRectTransform.gameObject.SetActive(isEnabled);
        EnableHideButton(isEnabled);
        if (isEnabled)
        {
            ToggleHide(true);
        }
    }
    private void ToggleHide() => ToggleHide(!isOpen);
    private void ToggleHide(bool isOpen)
    {
        if (this.isOpen == isOpen)
        { return; }
        this.isOpen = isOpen;
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        targetAnchoredPosition = isOpen ? Vector2.zero : closedPanelPosition;
        hideCoroutine = StartCoroutine(HideAnimation());

        PanelToggledEvent?.Invoke(isOpen);
        HideToggleIcon.transform.rotation = Quaternion.Euler(Vector3.forward * (isOpen ? -90 : 90));
    }

    #endregion

    public void PopulateDropdowns(List<AirportData> airportDatas)
    {
        List<OptionData> airportsOptions = new List<OptionData>();

        foreach (AirportData airportData in airportDatas)
        {
            OptionData airportOption = new OptionData()
            {
                text = airportData.Name,
                ExtraData = airportData
            };
            airportsOptions.Add(airportOption);
        }

        StartDropdownSearch.SetOptions(airportsOptions);
        DestinationDropdownSearch.SetOptions(airportsOptions);
    }

    private IEnumerator HideAnimation()
    {
        while (ContainerRectTransform.anchoredPosition != targetAnchoredPosition)
        {
            Vector3 currentPos = new Vector3(ContainerRectTransform.anchoredPosition.x, ContainerRectTransform.anchoredPosition.y);
            Vector3 newPos = Vector3.Lerp(currentPos, targetAnchoredPosition, TranslateSpeed * Time.deltaTime);
            ContainerRectTransform.anchoredPosition = new Vector2(newPos.x, newPos.y);
            yield return null;
        }
    }

    private void SearchSelectedFlight()
    {
        if (SelectedStartAirport != null && SelectedDestinationAirport != null && SelectedStartAirport.Iata != SelectedDestinationAirport.Iata)
        {
            SearchStartedEvent?.Invoke();
            StartCoroutine(CoSearchSelectedFlight());
        }
    }

    const int SEARCH_TIMEOUT = 10;


    private IEnumerator CoSearchSelectedFlight()
    {
        yield return null;

        TaskCompletionSource<List<List<string>>> task = new TaskCompletionSource<List<List<string>>>();
        CancellationTokenSource timeoutToken = new CancellationTokenSource();

        StartCoroutine(Pathfinder.FindFlight(SelectedStartAirport, SelectedDestinationAirport, task, timeoutToken.Token));

        timeoutToken.CancelAfter(SEARCH_TIMEOUT * 1000);

        yield return new WaitUntil(() => task.Task.IsCompleted);

        List<List<string>> flights = task.Task.Result;

        if (flights.Count > 0)
        {
            ListTitleText.text = $"Flights found: {flights.Count}";

            foreach (Transform oldTikect in ListPathsContentPanel)
            {
                Destroy(oldTikect.gameObject);
            }

            foreach (List<string> flight in flights)
            {
                if (flight.Count > 2)
                {
                    MultipleTicket ticket = Instantiate(MultipleTicketPrefab, ListPathsContentPanel);
                    ticket.Init(flight);
                }
                else
                {
                    Ticket ticket = Instantiate(TicketPrefab, ListPathsContentPanel);
                    ticket.Init(flight);
                }
            }

            NotPathsFound.gameObject.SetActive(false);
            ListPathsPanel.SetActive(true);
            ListTitleText.gameObject.SetActive(true);
        }
        else
        {
            ListPathsPanel.SetActive(false);
            ListTitleText.gameObject.SetActive(false);
            NotPathsFound.gameObject.SetActive(true);
        }


        SearchEndedEvent?.Invoke();
    }
    private bool isTimePaused = false;

    private void Update()
    {
        if (!isTimePaused)
        {
            TimeSlider.value = (TimeSlider.value + Time.deltaTime) % 360;
        }
    }
    public void PauseTime()
    {
        isTimePaused = true;
    }
    public void StartTime()
    {
        isTimePaused = false;
    }
    private void OnSliderValueChange(float value)
    {
        LightTransform.transform.localEulerAngles = new Vector3(0, value, 0);
    }

}
