using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FireHand : MonoBehaviour
{
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [SerializeField] private ParticleSystem rightHandFire;
    [SerializeField] private ParticleSystem leftHandFire;
    [SerializeField, Tooltip("If we should play the fire in both hands")]
    private bool fireInBothHands = true;
    private bool isFireActive = false;
    private bool wasRightHandRaised = false;


    void Start()
    {
        // Do nothing :)
    }

    void Update()
    {
        if (rightHandFire != null)
        {
            rightHandFire.gameObject.transform.position = _poseScriptableObject.GetCurrentLeftHandPosition();
            rightHandFire.gameObject.transform.rotation = _poseScriptableObject.leftHandRotation;
        }
        if (leftHandFire != null)
        {
            leftHandFire.gameObject.transform.position = _poseScriptableObject.GetCurrentRightHandPosition();
            leftHandFire.gameObject.transform.rotation = _poseScriptableObject.rightHandRotation;
        }

        bool isRightHandCurrentlyRaised = _poseScriptableObject.isLeftHandAboveShoulder && !_poseScriptableObject.isRightHandAboveShoulder;
        bool areBothHandsRaised = _poseScriptableObject.isLeftHandAboveShoulder && _poseScriptableObject.isRightHandAboveShoulder;

        if (areBothHandsRaised)
        {
            if (isFireActive)
            {
                DisableFire(rightHandFire);
                if (fireInBothHands) DisableFire(leftHandFire);
            }
            return;
        }

        if (isRightHandCurrentlyRaised && !wasRightHandRaised)
        {
            if (isFireActive)
            {
                DisableFire(rightHandFire);
                if (fireInBothHands) DisableFire(leftHandFire);
            }
            else
            {
                EnableFire(rightHandFire);
                if (fireInBothHands) EnableFire(leftHandFire);
            }
        }

        wasRightHandRaised = isRightHandCurrentlyRaised;
    }

    void EnableFire(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
            isFireActive = true;
        }
    }

    void DisableFire(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            isFireActive = false;
        }
    }
}
