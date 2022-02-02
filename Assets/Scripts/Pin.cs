using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;

public class Pin : MonoBehaviour
{
    public float TranslateSpeed = 2;
    public Vector3 TargetScale = Vector3.one;
    public AirportData airportData { get; private set; }
    private Action<AirportData> onClickCallback = null;
    public Renderer PinRenderer;
    public Transform AlternativePivot;
    public Material PlasticMaterial, PlasticMaterial_Start, PlasticMaterial_Destination, PlasticMaterial_Middle;
    public bool IsSpawned;

    public enum StatePin { Unselected, Start, Destination, Middle}

    void OnMouseDown()
    {
        if(CurrentAppState == AppState.Running)
        {
            onClickCallback?.Invoke(airportData);
        }
    }

    public void SetMaterial(StatePin statePin)
    {
        // set material
        var mats = PinRenderer.materials;

        if (statePin == StatePin.Unselected)
        {
            mats[1] =  PlasticMaterial;
        }
        else if (statePin == StatePin.Start)
        {
            mats[1] = PlasticMaterial_Start;
        }        
        else if (statePin == StatePin.Destination)
        {
            mats[1] = PlasticMaterial_Destination;
        }      
        else if (statePin == StatePin.Middle)
        {
            mats[1] = PlasticMaterial_Middle;
        }

        PinRenderer.materials = mats;
    }


    public void SetData(AirportData airportData, Action spawnDoneCallback, Action<AirportData> onClickCallback)
    {
        this.onClickCallback = onClickCallback;

        if (this.airportData != null)
        {
            Debug.LogError("Pin Already Setted!");
            return;
        }
        this.airportData = airportData;
        name = airportData.Name;
        airportData.DecimalCoordinateV = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateV);
        airportData.DecimalCoordinateH = Utilities.ConvertDegreeAngleToDouble(airportData.CoordinateH);
        transform.rotation = Quaternion.Euler((float)airportData.DecimalCoordinateV, (float)airportData.DecimalCoordinateH, 0);
        //Debug.Log("InizioPin");
        StartCoroutine(SpawnAnimation(spawnDoneCallback));
    }

    private IEnumerator SpawnAnimation(Action spawnDoneCallback)
    {
        PinRenderer.transform.localPosition = new Vector3(0, 0, 300);
        Vector3 targetPos = new Vector3(0, 0, 100f);
        float animationSpeed = 1f;
        while (!PinRenderer.transform.localPosition.Equals(targetPos, 0.1f))
        {
            PinRenderer.transform.localPosition = Vector3.Slerp(PinRenderer.transform.localPosition, targetPos, animationSpeed * Time.deltaTime);
            yield return null;
        }
        IsSpawned = true;
        //Debug.Log("FinitoPin");
        spawnDoneCallback?.Invoke();
    }


    //void OnMouseEnter()
    //{
    //    TargetScale = Vector3.one * 3;
    //    var mats = PinRenderer.materials;
    //    mats[1] = PlasticMaterial_Hover;
    //    PinRenderer.materials = mats;
    //    Debug.Log($"Mouse is over {name}.");
    //}
    //void OnMouseExit()
    //{
    //    TargetScale = Vector3.one;
    //    var mats = PinRenderer.materials;
    //    mats[1] = PlasticMaterial;
    //    PinRenderer.materials = mats;
    //    Debug.Log($"Mouse is exit {name}.");
    //}
    //void OnMouseDown()
    //{
    //    var mats = PinRenderer.materials;
    //    mats[1] = PlasticMaterial_Selected;
    //    PinRenderer.materials = mats;
    //    Debug.Log($"Mouse is down {name}.");
    //}
    //private void Update()
    //{
    //    PinRenderer.transform.localScale = Vector3.Slerp(PinRenderer.transform.localScale, TargetScale, TranslateSpeed * Time.deltaTime);
    //}
}
