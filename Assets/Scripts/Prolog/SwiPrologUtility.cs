using SbsSW.SwiPlCs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SwiPrologUtility
{
    public static bool IsInitialized = false;

    public static List<AirportData> DeserializeAirportsData()
    {
        List<AirportData> airportsData = new List<AirportData>();

        bool result = Init();

        if (result)
        {
            using (PlQuery q = new PlQuery("prop(IATA, AERO, COUNTRY, COORD, CONTINENT)."))
            {
                foreach (PlQueryVariables v in q.SolutionVariables)
                {
                    string[] coords = v["COORD"].ToString().Split(' ');
                    AirportData airportData = new AirportData()
                    {
                        Iata = v["IATA"].ToString(),
                        Name = v["AERO"].ToString(),
                        Country = v["COUNTRY"].ToString(),
                        Continent = v["CONTINENT"].ToString(),
                        CoordinateV = coords[0],
                        CoordinateH = coords[1]
                    };
                    airportsData.Add(airportData);
                }
                //q.Variables["AERO"].Unify("Aeroporto di Bari");
                //foreach (PlQueryVariables v in q.SolutionVariables)
                //{
                //    text.text += v["ID"].ToString() + " : " + v["COORD"].ToString() + "\n";
                //    Debug.Log(v["ID"].ToString() + " : " + v["COORD"].ToString());
                //}
                q.Dispose();
            }
        }
        return airportsData;
    }

    public static AirportData GetAirportData(string IATA)
    {
        Init();
        using (PlQuery q = new PlQuery($"prop('{IATA}', AERO, COUNTRY, COORD, CONTINENT)."))
        {
            //q.Variables["IATA"].Unify(IATA);
            AirportData airportData = null;
            foreach (PlQueryVariables v in q.SolutionVariables)
            {
                string[] coords = v["COORD"].ToString().Split(' ');
                airportData = new AirportData()
                {
                    Iata = IATA,
                    Name = v["AERO"].ToString(),
                    Country = v["COUNTRY"].ToString(),
                    Continent = v["CONTINENT"].ToString(),
                    CoordinateV = coords[0],
                    CoordinateH = coords[1]
                };
            }
            q.Dispose();
            return airportData;
        }
    }

    public static List<string> GetFlights(string IATA)
    {
        Init();
        List<string> reachableAirports = new List<string>();
        using (PlQuery q = new PlQuery($"flight('{IATA}', DEST)."))
        {
            foreach (PlQueryVariables v in q.SolutionVariables)
            {
                reachableAirports.Add(v["DEST"].ToString());
            }
            q.Dispose();
        }
        return reachableAirports;
    }
    static bool Init()
    {
        if (PlEngine.IsInitialized)
        {
            //PlEngine.PlCleanup();
            //Debug.Log("SwiProlog already initialized");
            return IsInitialized;
        }
        Application.quitting -= Quit;
        Application.quitting += Quit;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif


        try
        {
            String[] param = { "-q" };  // suppressing informational and banner messages
                                        //PlEngine.Initialize(param);
            string project_dir = Application.streamingAssetsPath;
            string pl_source = Path.Combine(project_dir, "airports.pl");
            string[] argv = { "-q", pl_source };
            Debug.Log(pl_source);
            PlEngine.Initialize(argv);
            Debug.Log("SwiProlog initialized");
            IsInitialized = true;
            return IsInitialized;
        }
        catch
        {
            Debug.LogError("SwiProlog not initialized");
            IsInitialized = false;
            return IsInitialized;
        }
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Quit();
        }
    }
#endif

    private static void Quit()
    {
        if (PlEngine.IsInitialized)
        {
            IsInitialized = false;
            //PlEngine.PlCleanup();
        }
    }

}
