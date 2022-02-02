using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ticket : MonoBehaviour
{
    [SerializeField]
    protected TMP_Text priceText;    
    
    [SerializeField]
    protected TMP_Text durationText;

    [SerializeField]
    protected TMP_Text pathText;

    [SerializeField]
    protected Button button;

    [SerializeField]
    protected Image backgroundButton;

    public delegate void OnTicketClickedDelegate(Ticket ticket);
    public static event OnTicketClickedDelegate ticketClickedEvent;

    [SerializeField]
    protected Color selectedColor;

    [SerializeField]
    protected Color unselectedColor;

    static public List<AirportData> selectedAirports = new List<AirportData>();

    private void Start()
    {
        ticketClickedEvent += Deselect;
    }

    public void Init(List<string> list, double[] prediction = null)
    {
        pathText.text = pathText.text = $"{list[0]}:{list[ 1]}";

        if (prediction == null)
        {
            prediction = Regressor.Instance.GetPredicitions(list[0], list[1]);

            if (prediction == null)
            {
                Debug.LogError("Prediction failed");
                return;
            }
        }

        TimeSpan spWorkMin = TimeSpan.FromMinutes(prediction[0]);
        string stringDuration = string.Format("{0:00}h{1:00}m", (int)spWorkMin.TotalHours, spWorkMin.Minutes);

        string stringPrice = (Math.Truncate(100 * prediction[1]) / 100).ToString();

        durationText.text = $"{stringDuration}";
        priceText.text = $"{stringPrice}$";

        button.onClick.AddListener(() => OnButtonClicked(list));
    }


    protected void Deselect(Ticket ticket)
    {
        if(ticket != this && backgroundButton != null)
        {
            backgroundButton.color = unselectedColor;
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
        ticketClickedEvent -= Deselect;
    }

    private void ClearOldPins()
    {
        foreach(AirportData airpot in selectedAirports)
        {
            airpot.pinInstance.SetMaterial(Pin.StatePin.Unselected);
        }

        selectedAirports.Clear();
    }


    private void ChangeColorSlectedPins()
    {
        selectedAirports[0].pinInstance.SetMaterial(Pin.StatePin.Start);

        for (int i = 1; i < selectedAirports.Count -1; i++)
        {
            selectedAirports[i].pinInstance.SetMaterial(Pin.StatePin.Middle);
        }

        selectedAirports[selectedAirports.Count - 1].pinInstance.SetMaterial(Pin.StatePin.Destination);
    }


    protected virtual void OnButtonClicked(List<string> iatas)
    {

        backgroundButton.color = selectedColor;

        ClearOldPins();

        ArchesSpawner.Instance.DeleteOldsArches();

        AirportData result1;
        AirportData result2 = PinManager.Instance.GetAirportData(iatas[0]);

        selectedAirports.Add(result2);

        for (int i = 1; i < iatas.Count; i++)
        {
            result1 = result2;

            result2 = PinManager.Instance.GetAirportData(iatas[i]);
            selectedAirports.Add(result2);

            Vector3 pos1 = result1.pinInstance.AlternativePivot.transform.position;
            Vector3 pos2 = result2.pinInstance.AlternativePivot.transform.position;

            ArchesSpawner.Instance.CreateArchFrom2Points(pos1, pos2);
        }

        ChangeColorSlectedPins();
        ticketClickedEvent?.Invoke(this);
    }


}
