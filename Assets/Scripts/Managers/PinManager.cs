using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using static SwiPrologUtility;

[Serializable]
public class AirportData
{
    public string Iata;
    public string Name;
    public string CoordinateV;
    public string CoordinateH;
    [HideInInspector] public double DecimalCoordinateV;
    [HideInInspector] public double DecimalCoordinateH;
    public string Country;
    public string Continent;
    public Pin pinInstance;
}

public class PinManager : MonoBehaviour
{
    public delegate void PinClickedDelegate(AirportData airportData);
    public static event PinClickedDelegate PinClickedEvent;

    [SerializeField]
    private Pin PinPrefab;
    [SerializeField]
    private Transform PinsContainer;
    [SerializeField]
    public List<AirportData> airportsData;
    private int spawnedPinsCount = 0;
    private TaskCompletionSource<List<AirportData>> deserializeTaskCompletion = new TaskCompletionSource<List<AirportData>>();
    public Task<List<AirportData>> DeserializeTask => deserializeTaskCompletion.Task;
    private TaskCompletionSource<bool> spawnTaskCompletion = new TaskCompletionSource<bool>();
    public Task<bool> SpawnTask => spawnTaskCompletion.Task;

    public static PinManager Instance
    {
        private set;
        get;
    }



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            airportsData = DeserializeAirportsData();
            deserializeTaskCompletion.TrySetResult(airportsData);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetAirportCoords(string IATA)
    {
        AirportData airportData = GetAirportData(IATA);

        if (airportData == null)
        {
            Debug.LogError(IATA);
            return Vector2.zero;
        }
        airportData.DecimalCoordinateH = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateH);
        airportData.DecimalCoordinateV = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateV);

        Vector2 airportCoords = new Vector2((float)airportData.DecimalCoordinateH, (float)airportData.DecimalCoordinateV);

        return airportCoords;
    }

    [ContextMenu("SpawnPins")]
    public async void SpawnPins()
    {
        await deserializeTaskCompletion.Task;
        for (int i = 0; i < airportsData.Count; i += 10)
        {
            for (int j = i; j < Mathf.Min(airportsData.Count, i + 10); j++)
            {
                AirportData pin = airportsData[j];
                SpawnPin(pin);
            }
            await Task.Delay(100);
        }
    }
    private void IncreasePinCount()
    {
        spawnedPinsCount++;
        if (spawnedPinsCount == airportsData.Count)
        {
            spawnTaskCompletion.TrySetResult(true);
        }
    }

    public void SpawnPin(AirportData airportData)
    {
        Pin pin = airportData.pinInstance;

        if (airportData.pinInstance == null)
        {
            pin = Instantiate(PinPrefab, PinsContainer, false);
            airportData.pinInstance = pin;
        }

        pin.SetData(airportData, IncreasePinCount, PinClickedEvent.Invoke);

    }

    public AirportData GetAirportData(string iata)
    {
        if (airportsData == null)
        {
            Debug.LogError("AirportsData is null");
            return null;
        }

        return airportsData.Find(x => x.Iata == iata);
    }
}
