using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utilities
{
    public struct Pair<T1,T2>
    {
        public T1 x;
        public T2 y;
    }

    public enum AppState
    {
        Starting,
        Running
    }

    public delegate void AppStateChangedDelegate(AppState appState);
    public static event AppStateChangedDelegate AppStateChangedEvent;

    private static AppState _CurrentAppState = AppState.Starting;
    public static AppState CurrentAppState {
        set
        {
            _CurrentAppState = value;
            AppStateChangedEvent?.Invoke(value);
        }
        get
        {
            return _CurrentAppState;
        }
    }

    public static bool Equals(this Vector3 lhs, Vector3 rhs, float tollerance)
    {
        return Vector3.SqrMagnitude(lhs - rhs) < tollerance;
    }
    public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
    public static double ConvertDegreeAngleToDouble(string point)
    {
        var multiplier = (point.Contains("N") || point.Contains("E")) ? -1 : 1; //handle south and west

        point = Regex.Replace(point, "°", "_"); //remove the characters
        point = Regex.Replace(point, "′", "_"); //remove the characters
        point = Regex.Replace(point, "″", "_"); //remove the characters

        var pointArray = point.Split('_'); //split the string.

        //Decimal degrees = 
        //   whole number of degrees, 
        //   plus minutes divided by 60, 
        //   plus seconds divided by 3600

        var degrees = Double.Parse(pointArray[0], CultureInfo.InvariantCulture);
        var minutes = Double.Parse(pointArray[1], CultureInfo.InvariantCulture) / 60;
        var seconds = Double.Parse(pointArray[2], CultureInfo.InvariantCulture) / 3600;

        return (degrees + minutes + seconds) * multiplier;
    }
}
