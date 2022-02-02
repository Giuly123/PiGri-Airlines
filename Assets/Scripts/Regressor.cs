using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Models.Regression.Linear;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class Regressor : MonoBehaviour
{
    public static Regressor Instance
    {
        private set;
        get;
    }


    [SerializeField]
    private PinManager PinManager;

    private MultivariateLinearRegression regression;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(TaskCompletionSource<bool> taskCompletionSource)
    {
        string project_dir = Application.streamingAssetsPath;
        string csv_source = Path.Combine(project_dir, "travels.CSV");
        List<TravelData> travelsData = ReadTravelCSV(csv_source);
        double[][] regressionInputs = new double[travelsData.Count][];
        double[][] regressionOutputs = new double[travelsData.Count][];
        for (int i = 0; i < travelsData.Count; i++)
        {
            TravelData travelData = travelsData[i];

            double[] travelInputs = { /*travelData.month,*/ travelData.startX, travelData.startY, travelData.destinationX, travelData.destinationY, travelData.distance, /*travelData.stops,*/ travelData.intercontinental };//[month;start[x,y];destination[x,y];stops]
            double[] travelOutputs = { travelData.price, travelData.duration };//[Price;duration]

            regressionInputs[i] = travelInputs;
            regressionOutputs[i] = travelOutputs;
        }

        double[][] predictions;
        regression = null;
        double[] errors = { -1, -1 };
        OrdinaryLeastSquares ols = new OrdinaryLeastSquares()
        {
            IsRobust = true,
            UseIntercept = false
        };
        regression = ols.Learn(regressionInputs, regressionOutputs);
        predictions = regression.Transform(regressionInputs);
        errors = new RSquaredLoss(regressionInputs.Length, regressionOutputs).Loss(predictions); // 0
        Debug.Log(errors[0] + "\t" + errors[1]);

        taskCompletionSource.SetResult(true);
    }

    public struct TravelData
    {
        public int month;
        public double startX;
        public double startY;
        public double destinationX;
        public double destinationY;
        public double intercontinental;
        public double distance;
        public int duration;
        public int stops;
        public double price;
    }


    public double[] GetPredicitions(string startIata, string destinationIata)
    {
       float distance = GetTravelDistance(startIata, new string[] { destinationIata });

        AirportData a1 = GetAirportCoords2(startIata);
        AirportData a2 = GetAirportCoords2(destinationIata);

        double[] travelTestInputs =
        {
            //8,
            a1.DecimalCoordinateH,
            a1.DecimalCoordinateV,
            a2.DecimalCoordinateH,
            a2.DecimalCoordinateV,
            distance,
            //0,
            (a1.Continent != a2.Continent) ? 2 : 1,
        };

        double[] predictions = regression.Transform(travelTestInputs);
        return predictions;
    }


    public List<TravelData> ReadTravelCSV(string path)
    {
        List<TravelData> list = new List<TravelData>();

        string[] rows = File.ReadAllLines(path);

        string[] record;

        for (int i = 0; i < rows.Length; i++)
        {
            record = rows[i].Split(';');//[month;start[x,y];destination[x,y];duration;stops;Price]
            AirportData startAirport = PinManager.GetAirportData(record[1]);
            string[] destinationAirports = record[2].Contains("_") ? record[2].Split('_') : new string[] { record[2] };
            AirportData destinationAirport = PinManager.GetAirportData(destinationAirports[destinationAirports.Length - 1]);
            try
            {
                TravelData data = new TravelData()
                {
                    month = int.Parse(record[0], CultureInfo.InvariantCulture),
                    startX = Utilities.ConvertDegreeAngleToDouble(startAirport.CoordinateH),
                    startY = Utilities.ConvertDegreeAngleToDouble(startAirport.CoordinateV),
                    destinationX = Utilities.ConvertDegreeAngleToDouble(destinationAirport.CoordinateH),
                    destinationY = Utilities.ConvertDegreeAngleToDouble(destinationAirport.CoordinateV),
                    intercontinental = (startAirport.Continent != destinationAirport.Continent) ? 2 : 1,
                    distance = GetTravelDistance(record[1], destinationAirports),
                    duration = int.Parse(record[4], CultureInfo.InvariantCulture),
                    stops = 0/*int.Parse(record[4], CultureInfo.InvariantCulture)*/,
                    price = double.Parse(record[3], CultureInfo.InvariantCulture),
                };
                list.Add(data);
            }
            catch
            {
                Debug.Log("");
            }
        }

        return list;
    }


    private AirportData GetAirportCoords2(string IATA)
    {
        AirportData airportData = PinManager.GetAirportData(IATA);
        if (airportData == null)
        {
            Debug.LogError(IATA);
            return null;
        }
        airportData.DecimalCoordinateH = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateH);
        airportData.DecimalCoordinateV = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateV);

        return airportData;
    }


    private float GetTravelDistance(string StartIata, string[] StopsIATA)
    {
        float distance = 0;

        Vector2 coords1, coords2 = PinManager.GetAirportCoords(StartIata);
        foreach (string IATA in StopsIATA)
        {
            coords1 = coords2;
            coords2 = PinManager.GetAirportCoords(IATA);
            distance += Mathf.Abs(Vector2.Angle(coords1, coords2));

        }

        return distance;
    }
}
