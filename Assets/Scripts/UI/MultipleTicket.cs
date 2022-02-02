using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultipleTicket : Ticket
{
    [SerializeField]
    private Button toggleDetailedPanel;

    [SerializeField]
    private Ticket prefabTiket;

    [SerializeField]
    private GameObject spacer;

    [SerializeField]
    private TMP_Text stopsText;

    [SerializeField]
    private GameObject subTickets;

    //[SerializeField]
    //private VerticalLayoutGroup verticalLayoutGroup;

    private bool isDetailed = false;

    private void Awake()
    {
        toggleDetailedPanel.onClick.AddListener(ToggleDetailedPanel);
    }

    private void Start()
    {
        ticketClickedEvent += Deselect;
    }

    public void Init(List<string> list)
    {
        pathText.text = $"{list[0]}:{list[list.Count-1]}";

        double totalPrice = 0;
        double totalDuration = 0;

        stopsText.text = $"Stops: {list.Count - 2}";

        for (int i = 0; i < list.Count-1; i++)
        {
            double[] prediction = Regressor.Instance.GetPredicitions(list[i], list[i + 1]);

            Ticket tempTicket = Instantiate(prefabTiket, subTickets.transform);
            tempTicket.Init(new List<string> { list[i] , list[i + 1] }, prediction);

            if(prediction != null)
            {
                totalDuration += prediction[0];
                totalPrice += prediction[1];
            }
        }

        TimeSpan spWorkMin = TimeSpan.FromMinutes(totalDuration);
        string stringDuration = string.Format("{0:00}h{1:00}m", (int)spWorkMin.TotalHours, spWorkMin.Minutes);
        string stringPrice = (Math.Truncate(100 * totalPrice) / 100).ToString();

        durationText.text = $"{stringDuration}";
        priceText.text = $"{stringPrice}$";

        button.onClick.AddListener(() => base.OnButtonClicked(list));
    }


    private void OnDestroy()
    {
        toggleDetailedPanel.onClick.RemoveListener(ToggleDetailedPanel);
        button.onClick.RemoveAllListeners();
        ticketClickedEvent -= Deselect;
    }


    private void ToggleDetailedPanel()
    {
        isDetailed = !isDetailed;

        spacer.SetActive(isDetailed);
        subTickets.SetActive(isDetailed);

        toggleDetailedPanel.transform.eulerAngles = isDetailed ? new Vector3(0,0,0) : new Vector3(0, 0, 180);
    }


}
