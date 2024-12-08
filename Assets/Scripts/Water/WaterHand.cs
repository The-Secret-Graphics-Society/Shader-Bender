using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WaterHand : MonoBehaviour
{
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [SerializeField] private ParticleSystem rightHandWater;
    [SerializeField] private ParticleSystem leftHandWater;
    [SerializeField, Tooltip("If we should play the water in both hands")]
    private bool waterInBothHands = true;
    private bool isWaterActive = false;


    void Start()
    {
        // Do nothing :)

    }

    private void OnEnable()
    {
        ElementState.onWaterActive += EnableWater;
        ElementState.onElementDeactivate += DisableWater;
    }

    private void OnDisable()
    {
        ElementState.onWaterActive -= EnableWater;
        ElementState.onElementDeactivate -= DisableWater;
    }

    void Update()
    {
        if (rightHandWater != null)
        {
            rightHandWater.gameObject.transform.position = _poseScriptableObject.GetCurrentLeftHandPosition();
            //rightHandWater.gameObject.transform.rotation = _poseScriptableObject.leftHandRotation;
        }
        if (leftHandWater != null)
        {
            leftHandWater.gameObject.transform.position = _poseScriptableObject.GetCurrentRightHandPosition();
            //leftHandWater.gameObject.transform.rotation = _poseScriptableObject.rightHandRotation;
        }
    }

    void EnableWater()
    {
        if (rightHandWater != null)
        {
            rightHandWater.Play();
            isWaterActive = true;
        }
        if (leftHandWater != null && waterInBothHands)
        {
            leftHandWater.Play();
            isWaterActive = true;
        }
    }
    void DisableWater()
    {
        if (rightHandWater != null)
        {
            rightHandWater.Stop();
            isWaterActive = false;
        }
        if (leftHandWater != null)
        {
            leftHandWater.Stop();
            isWaterActive = false;
        }
    }
}