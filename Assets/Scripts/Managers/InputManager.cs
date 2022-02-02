//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class InputManager : MonoBehaviour
//{
//    [SerializeField]
//    private Transform ScenarioContainer;
//    public void Update()
//    {
//        if (Utilities.CurrentAppState != Utilities.AppState.Running)
//        {
//            return;
//        }

//        MouseControls();
//    }
//    private void MouseControls()
//    {
//        float xMove = Input.GetAxis("Mouse X");
//        float yMove = Input.GetAxis("Mouse Y");
//        Vector2 MouseMove = new Vector2(xMove, yMove);
//        MouseMove.Scale(MouseSensibility);

//        if (Input.GetButton("Fire1"))
//        {
//            if (xMove != 0 || yMove != 0)
//            {
//                CurrentCameraTranslateData.AngleH += MouseMove.x;
//                CurrentCameraTranslateData.AngleV = Mathf.Clamp(CurrentCameraTranslateData.AngleV + MouseMove.y, -90, 90);
//            }
//        }
//        CurrentCameraTranslateData.Distance += Input.GetAxis("Mouse ScrollWheel");
//    }
//    public void LateUpdate()
//    {
//        Utilities.Pair<Vector3, Quaternion> finalPosRot = GetFinalPosRot(CurrentCameraTranslateData);

//        transform.position = Vector3.Slerp(transform.position, finalPosRot.x, CurrentCameraTranslateData.TranslateSpeed * Time.deltaTime);
//        if (!CurrentCameraTranslateData.KeepRotation)
//        {
//            transform.LookAt(CurrentCameraTranslateData.TargetPos, finalPosRot.y * Vector3.up);
//        }
//        if (currentTask != null && transform.position.Equals(finalPosRot.x, 75f))
//        {
//            currentTask.TrySetResult(true);
//            currentTask = null;
//        }
//    }
//}
