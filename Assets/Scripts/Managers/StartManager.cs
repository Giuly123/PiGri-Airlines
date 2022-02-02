using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static SphereTranslate;
using static SphericalCam;
using static Utilities;

public class StartManager : MonoBehaviour
{
    [SerializeField]
    private EarthFocus EarthFocus;
    [SerializeField]
    private UIManager UIManager;
    [SerializeField]
    private PinManager PinManager;
    [SerializeField]
    private Regressor Regressor;

    [SerializeField]
    private Vector2 StartAngle;
    [SerializeField]
    private Vector2 EndAngle;
    [SerializeField]
    private float StartDistance;
    [SerializeField]
    private float EndDistance;
    [SerializeField]
    private float AnimationTranslateSpeed;
    [SerializeField]
    private float AnimationRotationSpeed;
    [SerializeField]
    private float InAppTranslateSpeed;
    [SerializeField]
    private float InAppRotationSpeed;
    [SerializeField]
    private Transform Earth;

    private void Start()
    {
        StartCoroutine(CoInit());
    }

    private IEnumerator CoInit()
    {
        this.UIManager.SetEnableLoaderPanel(true);

        yield return new WaitUntil(() => PinManager.Instance.DeserializeTask.IsCompleted);

        TaskCompletionSource<bool> regressorTask = new TaskCompletionSource<bool>();

        Regressor.Init(regressorTask);

        yield return new WaitUntil(() => regressorTask.Task.IsCompleted);

        yield return new WaitForEndOfFrame();

        this.UIManager.SetEnableLoaderPanel(false);

        Task<bool> cameraAnimationTask = StartCameraAnimation();
        PinManager.SpawnPins();

        yield return new WaitUntil(() => PinManager.SpawnTask.IsCompleted);

        yield return new WaitUntil(() => cameraAnimationTask.IsCompleted);

        yield return new WaitUntil(() => UIManager.DestinationPanelView.ReadyTask.Task.IsCompleted);

        //Animazione Logo

        this.UIManager.SetEnableLogoPanel(true);

        yield return new WaitForSeconds(3);

        this.UIManager.SetEnableLogoPanel(false);

        CurrentAppState = AppState.Running;
        TraslateCameraAnimation();
    }


    private Task<bool> StartCameraAnimation()
    {
        SphereTranslateData StartTranslateData = new SphereTranslateData()
        {
            Target = Earth,
            AngleH = StartAngle.x,
            AngleV = StartAngle.y,
            Distance = StartDistance,
        };
        SphereTranslateData EndTranslateData = new SphereTranslateData()
        {
            Target = Earth,
            AngleH = EndAngle.x,
            AngleV = EndAngle.y,
            Distance = EndDistance,
            TranslateSpeed = AnimationTranslateSpeed,
            RotationSpeed = AnimationRotationSpeed,
        };

        TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();
        EarthFocus.AnimateCamera(StartTranslateData, EndTranslateData, taskCompletion);
        return taskCompletion.Task;
    }
    private async void TraslateCameraAnimation()
    {
        SphereTranslateData TranslateData = new SphereTranslateData()
        {
            OffsetPos = Vector3.right * 55f,
            AngleH = 0,
            AngleV = 0,
            Distance = 200,
            TranslateSpeed = AnimationRotationSpeed,
            RotationSpeed = AnimationRotationSpeed,
        };

        TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();
        EarthFocus.MoveFocus(EndAngle.x, EndAngle.y, TranslateData, taskCompletion);
        await taskCompletion.Task;
        //TranslateData.TranslateSpeed = InAppTranslateSpeed;
        //TranslateData.RotationSpeed = InAppRotationSpeed;
        //EarthFocus.MoveFocus(EndAngle.x, EndAngle.y, TranslateData);
        EarthFocus.EnableMouse();
    }

}
