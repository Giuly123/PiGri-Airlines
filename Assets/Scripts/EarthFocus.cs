using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static SphereTranslate;

public class EarthFocus : MonoBehaviour
{
    [SerializeField]
    private Transform Earth;
    [SerializeField]
    private SphericalCam Camera;
    [SerializeField]
    private SphereTranslateData CurrentTranslateData;
    public bool isMouseControlEnabled = true;
    private bool isMouseClickOutsideUI = false;
    [SerializeField]
    private Vector2 MouseSensibility = new Vector2(1, 1);
    private TaskCompletionSource<bool> currentTask = null;
    private Transform FocusPivot => transform;
    private SphereTranslateData CameraFollowTranslateData => new SphereTranslateData()
    {
        Distance = 0,
        //AnimationSpeed = float.MaxValue,
        Target = FocusPivot
    };

    #region UnityMessages
    private void Awake()
    {
        //Camera.isMouseControlEnabled = !this.isMouseControlEnabled;
        Ticket.ticketClickedEvent += ShowTicketPath;
    }
    public void Update()
    {
        if (Utilities.CurrentAppState != Utilities.AppState.Running)
        {
            return;
        }

        if (isMouseControlEnabled)
        {
            MouseControls();
        }
    }
    private void OnDestroy()
    {
        Ticket.ticketClickedEvent -= ShowTicketPath;
    }

    internal bool CheckClickInUI()
    {
        var m_EventSystem = EventSystem.current;
        var m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_EventSystem.RaycastAll(m_PointerEventData, results);

        return (results.Count > 0);
    }

    public void LateUpdate()
    {
        if (isMouseControlEnabled)
        {
            SphereTranslate.InstantTranslate(transform, CurrentTranslateData);
            SphereTranslate.InstantTranslate(Camera.transform, Camera.CurrentTranslateData);
        }

        //LerpTranslate(FocusPivot, CurrentTranslateData, currentTask);
    }
    #endregion

    #region MouseControls

    [SerializeField]
    private Vector2
    currentAngle = Vector2.zero,
    angleSensibility = Vector3.one,
    angleSmoothTime = new Vector2(0.3f, 0.3f),
    verticalAngleBounds = new Vector2(-90, 90),
    zoomBounds = new Vector2(5, 15);

    [SerializeField]
    private float
        currentZoom = 0,
        zoomSensibility = 1,
        zoomSmoothTime = 0.3f;

    private Vector2 targetAngle, angleVelocity;
    private float targetZoom, zoomVelocity;

    public void EnableMouse()
    {
        isMouseControlEnabled = true;
        Camera.isMouseControlEnabled = true;
        targetAngle.x = CurrentTranslateData.AngleH;
        targetAngle.y = CurrentTranslateData.AngleV;
        currentAngle = targetAngle;
        targetZoom = Camera.CurrentTranslateData.Distance;
        currentZoom = targetZoom;
    }
    private void MouseControls()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            isMouseClickOutsideUI = !CheckClickInUI();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            isMouseClickOutsideUI = false;
        }

        if (isMouseClickOutsideUI)
        {
            targetAngle.x += Input.GetAxis("Mouse X") * angleSensibility.x;
            targetAngle.y += Input.GetAxis("Mouse Y") * angleSensibility.y;
        }
        targetAngle.x %= 360;
        targetAngle.y = Mathf.Clamp(targetAngle.y, verticalAngleBounds.x, verticalAngleBounds.y);
        if (!CheckClickInUI())
        {
            targetZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSensibility;
            targetZoom = Mathf.Clamp(targetZoom, zoomBounds.x, zoomBounds.y);
        }

        currentAngle.x = Mathf.SmoothDampAngle(currentAngle.x, targetAngle.x, ref angleVelocity.x, angleSmoothTime.x);
        currentAngle.y = Mathf.SmoothDampAngle(currentAngle.y, targetAngle.y, ref angleVelocity.y, angleSmoothTime.y);
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

        CurrentTranslateData.AngleH = currentAngle.x;
        Camera.CurrentTranslateData.AngleH = CurrentTranslateData.AngleH;
        CurrentTranslateData.AngleV = currentAngle.y;
        Camera.CurrentTranslateData.AngleV = CurrentTranslateData.AngleV;
        Camera.CurrentTranslateData.TranslateSpeed = new Vector3(angleVelocity.x, angleVelocity.y, zoomVelocity).magnitude;
        Camera.CurrentTranslateData.RotationSpeed = angleVelocity.magnitude;
        Camera.CurrentTranslateData.Distance = currentZoom;
        Camera.CurrentTranslateData.OffsetPos.x = Utilities.Remap(currentZoom, zoomBounds.x, zoomBounds.y, 0, 55f);

    }
    public void ShowTicketPath(Ticket ticket)
    {
        if (Ticket.selectedAirports.Count == 0)
        {
            return;
        }

        Vector2 centroid = Vector2.zero;
        Vector2 maxAngle = new Vector2((float)Ticket.selectedAirports[0].DecimalCoordinateH, (float)Ticket.selectedAirports[0].DecimalCoordinateV);
        Vector2 minAngle = maxAngle;


        foreach (AirportData airpot in Ticket.selectedAirports)
        {
            Vector2 airportCoord = new Vector2((float)airpot.DecimalCoordinateH, (float)airpot.DecimalCoordinateV);
            centroid += airportCoord;
            maxAngle.x = (airportCoord.x > maxAngle.x) ? airportCoord.x : maxAngle.x;
            maxAngle.y = (airportCoord.y > maxAngle.y) ? airportCoord.y : maxAngle.y;
            minAngle.x = (airportCoord.x < minAngle.x) ? airportCoord.x : minAngle.x;
            minAngle.y = (airportCoord.y < minAngle.y) ? airportCoord.y : minAngle.y;
        }
        centroid /= Ticket.selectedAirports.Count;

        Vector2 angleDiff = maxAngle - minAngle;
        float maxDiff = Mathf.Max(angleDiff.x, angleDiff.y);

        targetZoom = Utilities.Remap(maxDiff, 0,180, zoomBounds.x, zoomBounds.y);

        targetAngle = centroid;
    }



    #endregion

    #region AnimateCamera
    public void AnimateCamera(SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        if (currentTask != null)
        {
            currentTask.TrySetResult(false);
        }

        CurrentTranslateData = translateData;
        Camera.AnimateCamera(translateData, taskCompletion);
    }
    public void AnimateCamera(SphereTranslateData startTranslateData, SphereTranslateData targetTranslateData, TaskCompletionSource<bool> taskCompletion = null)
    {

        if (currentTask != null)
        {
            currentTask.TrySetResult(false);
        }

        CurrentTranslateData = startTranslateData;
        InstantTranslate(FocusPivot, CurrentTranslateData);
        CurrentTranslateData = targetTranslateData;
        targetAngle.x = CurrentTranslateData.AngleH;
        targetAngle.y = CurrentTranslateData.AngleV;
        targetZoom = CurrentTranslateData.Distance;
        Camera.AnimateCamera(startTranslateData, targetTranslateData, taskCompletion);
    }


    #endregion

    #region MoveFocus
    public async void MoveFocus(float longitude, float latitude, SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        if (currentTask != null)
        {
            currentTask.TrySetResult(false);
        }

        SphereTranslateData focusTranslateData = new SphereTranslateData()
        {
            AngleH = longitude,
            AngleV = latitude,
            TranslateSpeed = translateData.TranslateSpeed,
            RotationSpeed = translateData.RotationSpeed,
            Distance = 100f,//EarthRadius
            Target = Earth.transform,
        };
        CurrentTranslateData = focusTranslateData;
        targetAngle.x = CurrentTranslateData.AngleH;
        targetAngle.y = CurrentTranslateData.AngleV;
        targetZoom = CurrentTranslateData.Distance;
        InstantTranslate(FocusPivot, focusTranslateData);

        SphereTranslateData FollowTranslateData = CameraFollowTranslateData;
        FollowTranslateData.TranslateSpeed = translateData.TranslateSpeed;
        FollowTranslateData.RotationSpeed = translateData.RotationSpeed;
        FollowTranslateData.AngleH = longitude + translateData.AngleH;
        FollowTranslateData.AngleV = latitude + translateData.AngleV;
        FollowTranslateData.Distance = translateData.Distance;
        FollowTranslateData.OffsetPos = translateData.OffsetPos;
        FollowTranslateData.isOffsetInTargetSpace = true;
        Camera.AnimateCamera(FollowTranslateData, taskCompletion);
        targetZoom = FollowTranslateData.Distance;

        if (taskCompletion != null)
        {
            await taskCompletion.Task;
        }
    }
    public void MoveFocus(Pin pin, SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        AirportData airportData = pin.airportData;
        MoveFocus((float)airportData.DecimalCoordinateH, (float)airportData.DecimalCoordinateV, translateData, taskCompletion);

    }
    public void MoveFocus(Vector2 earthPointCoords, SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        MoveFocus(earthPointCoords.x, earthPointCoords.y, translateData, taskCompletion);
    }
    #endregion
}
