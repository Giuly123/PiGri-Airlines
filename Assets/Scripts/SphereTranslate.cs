using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class SphereTranslate
{
    [Serializable]
    public struct SphereTranslateData
    {
        public float Distance;
        public float TranslateSpeed;
        public float RotationSpeed;
        public float AngleH;
        public float AngleV;
        public Transform Target;
        public Vector3 OffsetPos;
        public bool isOffsetInTargetSpace;
        public Vector3 TargetPos
        {
            get
            {
                Vector3 _targetPos = (Target != null) ? Target.position : Vector3.zero;
                Vector3 _offsetPos = (Target != null && isOffsetInTargetSpace) ? Target.TransformDirection(OffsetPos) : OffsetPos;
                return _targetPos + _offsetPos;
            }
        }
    }
    public static void InstantTranslate(Transform movableObject, SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        Utilities.Pair<Vector3, Quaternion> finalPosRot = GetFinalPosRot(translateData);
        movableObject.position = finalPosRot.x;
        movableObject.LookAt(translateData.TargetPos, finalPosRot.y * Vector3.up);

        if (taskCompletion != null)
        {
            taskCompletion.TrySetResult(true);
        }
    }
    public static bool LerpTranslate(Transform movableObject, SphereTranslateData translateData, TaskCompletionSource<bool> taskCompletion = null)
    {
        Utilities.Pair<Vector3, Quaternion> finalPosRot = GetFinalPosRot(translateData);
        movableObject.position = Vector3.Lerp(movableObject.position, finalPosRot.x, translateData.TranslateSpeed * Time.deltaTime);

        Vector3 finalFw = (translateData.TargetPos - movableObject.position).normalized;
        Vector3 finalUp = finalPosRot.y * Vector3.up;

        Vector3 lerpFw = Vector3.Slerp(movableObject.forward, finalFw, translateData.RotationSpeed * Time.deltaTime);
        Vector3 lerpUp = Vector3.Slerp(movableObject.up, finalUp, translateData.RotationSpeed * Time.deltaTime);

        movableObject.LookAt(movableObject.position + lerpFw, lerpUp);

        bool animationCompleted = movableObject.position.Equals(finalPosRot.x, 75f) && movableObject.forward.Equals(finalFw, 0.000001f) && movableObject.up.Equals(finalUp, 0.000001f); 
        if (taskCompletion != null && animationCompleted)
        {
            taskCompletion.TrySetResult(true);
        }
        return animationCompleted;
    }

    private static Utilities.Pair<Vector3, Quaternion> GetFinalPosRot(SphereTranslateData translateData)
    {
        Quaternion rotation = Quaternion.Euler(translateData.AngleV, translateData.AngleH, 0);
        Vector3 worldPos = translateData.TargetPos + rotation * (Vector3.forward * translateData.Distance);
        return new Utilities.Pair<Vector3, Quaternion>() { x = worldPos, y = rotation };
    }
}
