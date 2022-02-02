using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Pathfinder
{
    const int MAX_AVAILABLEPATHS = 10;
    const int MAX_COST = 3;

    private class PathStepInfo
    {
        public AirportData AirportData;
        public PathStepInfo Parent = null;
        public int Cost = 0;
        public float Distance { get; set; }
        public float CostDistance => Cost + Distance;
    }

    public static IEnumerator FindFlight(AirportData startAriport, AirportData targetAirport, TaskCompletionSource<List<List<string>>> taskCompletionSource, CancellationToken cancellationToken)
    {
        List<AirportData> AirportsData = PinManager.Instance.airportsData;
        PathStepInfo start = new PathStepInfo()
        {
            AirportData = startAriport,
            Distance = GetAirportsNormalizedDistance(startAriport, targetAirport)
        };
        PathStepInfo target = new PathStepInfo()
        {
            AirportData = targetAirport,
        };

        List<PathStepInfo> activeAirports = new List<PathStepInfo>();
        activeAirports.Add(start);
        //List<PathStepInfo> visitedAirports = new List<PathStepInfo>();
        List<PathStepInfo> availablePaths = new List<PathStepInfo>();
        float startTime = Time.realtimeSinceStartup;

        while (activeAirports.Any())
        {
            yield return null;

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            PathStepInfo checkAirport = activeAirports.OrderBy(x => x.CostDistance).First();

            if (checkAirport.AirportData == target.AirportData)
            {
                availablePaths.Add(checkAirport);
                //Debug.Log($"Found path with {checkAirport.Cost} steps! Time: {startTime - Time.unscaledTime}");
                //PrintPath(checkAirport);

                //We can actually loop through the parents of each tile to find our exact path which we will show shortly. 
                //return;
                if (availablePaths.Count >= MAX_AVAILABLEPATHS)
                {
                    break;
                }
            }

            //visitedAirports.Add(checkAirport);
            activeAirports.Remove(checkAirport);

            if (checkAirport.Cost >= MAX_COST)
                continue;

            List<PathStepInfo> reachableAirports = GetReachableAirports(AirportsData, checkAirport, target);

            foreach (PathStepInfo reachableAirport in reachableAirports)
            {
                //We have already visited this tile so we don't need to do so again!
                if (CheckAlreadyVisited(checkAirport, reachableAirport.AirportData))
                    continue;

                //It's already in the active list, but that's OK, maybe this new tile has a better value (e.g. We might zigzag earlier but this is now straighter). 
                //if (activeAirports.Any(x => x.AirportData == reachableAirport.AirportData))
                //{
                //    var existingTile = activeAirports.First(x => x.AirportData == reachableAirport.AirportData);
                //    if (existingTile.CostDistance > checkAirport.CostDistance)
                //    {
                //        activeAirports.Remove(existingTile);
                //        activeAirports.Add(reachableAirport);
                //    }
                //}
                //else
                //{
                //    //We've never seen this tile before so add it to the list. 
                //    activeAirports.Add(reachableAirport);
                //}
                activeAirports.Add(reachableAirport);
            }
        }

        if (availablePaths.Count > 0)
        {
            Debug.Log($"Found {availablePaths.Count} paths! Time: {Time.realtimeSinceStartup - startTime}");
        }
        else
        {
            Debug.Log("No Path Found!");
        }

        List<List<string>> result = PathsToList(availablePaths);
        taskCompletionSource.TrySetResult(result);
    }

    static private List<List<string>> PathsToList(List<PathStepInfo> paths)
    {
        List<List<string>> result = new List<List<string>>();

        foreach (PathStepInfo path in paths)
        {
            List<string> tempList = new List<string>();

            PathStepInfo stepInfo = path;

            tempList.Add(stepInfo.AirportData.Iata);

            while (stepInfo.Parent != null)
            {
                stepInfo = stepInfo.Parent;
                tempList.Add(stepInfo.AirportData.Iata);
            }

            tempList.Reverse();
            result.Add(tempList);
        }

        return result;
    }


    private static bool CheckAlreadyVisited(PathStepInfo currentAirport, AirportData airportData)
    {
        PathStepInfo stepInfo = currentAirport;
        if (stepInfo.AirportData == airportData)
        {
            return true;
        }


        while (stepInfo.Parent != null)
        {
            stepInfo = stepInfo.Parent;
            if (stepInfo.AirportData == airportData)
            {
                return true;
            }
        }


        return false;
    }

    private static void PrintPath(PathStepInfo currentAirport)
    {
        string path = currentAirport.AirportData.Name + "\n";

        while (currentAirport.Parent != null)
        {
            currentAirport = currentAirport.Parent;
            path += currentAirport.AirportData.Name + "\n";
        }

        Debug.Log(path);
    }

    private static List<PathStepInfo> GetReachableAirports(List<AirportData> AirportsData, PathStepInfo currentAirport, PathStepInfo targetAirport)
    {
        List<PathStepInfo> reachableAirports = new List<PathStepInfo>();
        List<string> reachableAirportsIata = SwiPrologUtility.GetFlights(currentAirport.AirportData.Iata);
        foreach (string iata in reachableAirportsIata)
        {
            AirportData reachableAirportData = AirportsData.Find(x => x.Iata == iata);
            if (reachableAirportData != null)
            {
                PathStepInfo PathStepInfo = new PathStepInfo
                {
                    AirportData = reachableAirportData,
                    Parent = currentAirport,
                    Cost = currentAirport.Cost + 1,
                    Distance = GetAirportsNormalizedDistance(reachableAirportData, targetAirport.AirportData)
                };
                reachableAirports.Add(PathStepInfo);
            }
        }

        return reachableAirports;
    }

    private static float GetAirportsNormalizedDistance(AirportData airport1, AirportData airport2)
    {
        Vector2 airport1Coords = new Vector2((float)airport1.DecimalCoordinateH, (float)airport1.DecimalCoordinateV);
        Vector2 airport2Coords = new Vector2((float)airport2.DecimalCoordinateH, (float)airport2.DecimalCoordinateV);

        float distance = Mathf.Abs(Vector2.Angle(airport1Coords, airport2Coords));
        
        return Utilities.Remap(distance, 0, 180, 0, 1);
    }
}
