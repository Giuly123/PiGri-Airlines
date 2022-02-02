using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static SphereTranslate;

public class SphericalCam : MonoBehaviour
{
    [SerializeField]
    public SphereTranslateData CurrentTranslateData;
    public bool isMouseControlEnabled = false;
    private bool isMouseClickOnUI = false;
    [SerializeField]
    private Vector2 MouseSensibility = new Vector2(1, 1);
    public bool isRunningAnimation = false;

    private Transform Camera => transform;
    private TaskCompletionSource<bool> currentTask = null;

    public void Update()
    {
        if (Utilities.CurrentAppState != Utilities.AppState.Running)
        {
            return;
        }

        if (isRunningAnimation)
        {
            return;
        }


        //if (isMouseControlEnabled)
        //{
        //    MouseControls();
        //}
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
        if(!isMouseControlEnabled)
        {
            isRunningAnimation = LerpTranslate(Camera, CurrentTranslateData, currentTask);
        }
    }
    private void MouseControls()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            isMouseClickOnUI = CheckClickInUI();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            isMouseClickOnUI = false;
        }

        float xMove = Input.GetAxis("Mouse X");
        float yMove = Input.GetAxis("Mouse Y");
        Vector2 MouseMove = new Vector2(xMove, yMove);
        MouseMove.Scale(MouseSensibility);

        if (Input.GetButton("Fire1"))
        {
            if (xMove != 0 || yMove != 0)
            {
                CurrentTranslateData.AngleH += MouseMove.x;
                CurrentTranslateData.AngleV = Mathf.Clamp(CurrentTranslateData.AngleV + MouseMove.y, -90, 90);
            }
        }
        CurrentTranslateData.Distance += Input.GetAxis("Mouse ScrollWheel");
    }

    public void AnimateCamera(SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        isRunningAnimation = true;

        if (currentTask != null)
        {
            currentTask.TrySetResult(false);
        }

        CurrentTranslateData = translateData;
        currentTask = taskCompletion;
    }
    public void AnimateCamera(SphereTranslateData startTranslateData, SphereTranslateData targetTranslateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        CurrentTranslateData = startTranslateData;
        InstantTranslate(Camera, CurrentTranslateData);
        AnimateCamera(targetTranslateData, taskCompletion);
    }
    public void MoveCamera(SphereTranslateData translateData)
    {
        isRunningAnimation = true;

        if (currentTask != null)
        {
            currentTask.TrySetResult(false);
        }
        CurrentTranslateData = translateData;
        InstantTranslate(Camera, CurrentTranslateData);
    }
}