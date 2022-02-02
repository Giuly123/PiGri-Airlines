using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchesSpawner : MonoBehaviour
{

    public static ArchesSpawner Instance
    {
        private set;
        get;
    }


    [SerializeField]
    private ParticleSystem ParticleSystemPrefab;

    [SerializeField]
    private Transform ArchesContainer;

    [SerializeField]
    private Transform EarthTransform;

    [SerializeField]
    const float DefaultHight = 4;

    [SerializeField]
    Vector3 test1;   
    
    [SerializeField]
    Vector3 test2;

    [SerializeField]
    float hightTest = 1;

    private float timeDelay = 0;

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

    public void DeleteOldsArches()
    {
        timeDelay = 0;

        foreach (Transform particleSystemTransform in ArchesContainer)
        {
            Destroy(particleSystemTransform.gameObject);
        }
    }

    public void CreateArchFrom2Points(Vector3 pos1, Vector3 pos2, float hight = DefaultHight)
    {
        float distance = Vector3.Distance(pos1, pos2);

        Vector3 direction = (pos2 - pos1).normalized;
        Vector3 midPoint = pos1 + (direction * (distance / 2));

        float midPointEarthDistance = Vector3.Distance(midPoint, EarthTransform.position);

        float newHight = EarthTransform.lossyScale.x - midPointEarthDistance + hight;

        float radius = (4 * newHight * newHight + distance * distance) / (8 * newHight);

        Vector3 directionFromEarth = (EarthTransform.position - midPoint).normalized;

        Vector3 center = EarthTransform.position - directionFromEarth * (EarthTransform.lossyScale.x + hight - radius);

        ParticleSystem arch = Instantiate(ParticleSystemPrefab, ArchesContainer, false);
        arch.transform.position = center;

        arch.startDelay = timeDelay++;
        var emission = arch.emission;
        emission.rateOverTime = radius * arch.emission.rateOverTime.constant;

        var shape = arch.shape;
        shape.radius = radius;

        arch.transform.LookAt(EarthTransform, Vector3.Cross(direction, directionFromEarth));
    }

}
