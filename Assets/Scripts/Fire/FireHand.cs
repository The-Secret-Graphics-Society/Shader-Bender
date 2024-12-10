using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FireHand : MonoBehaviour
{
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    [SerializeField] private ParticleSystem rightHandFire;
    [SerializeField] private ParticleSystem leftHandFire;
    [SerializeField] private ParticleSystem flameThrower;
    [SerializeField, Tooltip("If we should play the fire in both hands")]
    private bool fireInBothHands = true;
    [SerializeField] private bool isFireActive = false;
    private bool wasRightHandRaised = false;

    private void OnEnable()
    {
        ElementState.onFireActive += EnableFires;
        ElementState.onElementDeactivate += DisableFires;
    }

    private void OnDisable()
    {
        ElementState.onFireActive -= EnableFires;
        ElementState.onElementDeactivate -= DisableFires;
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
        if (flameThrower != null)
        {
            flameThrower.gameObject.transform.position = (_poseScriptableObject.GetCurrentLeftHandPosition() 
            + _poseScriptableObject.GetCurrentLeftHandPosition()) / 2;
            flameThrower.gameObject.transform.rotation = _poseScriptableObject.jointHandRotation;
        }

        bool isRightHandCurrentlyRaised = _poseScriptableObject.isLeftHandAboveShoulder && !_poseScriptableObject.isRightHandAboveShoulder;
        bool areBothHandsRaised = _poseScriptableObject.isLeftHandAboveShoulder && _poseScriptableObject.isRightHandAboveShoulder;
        bool areHandsClose = _poseScriptableObject.closeHands;
        bool areArmsExtended = _poseScriptableObject.leftArmExtended && _poseScriptableObject.rightArmExtended;

        //if (isRightHandCurrentlyRaised && !wasRightHandRaised)
        //{
        //    if (isFireActive)
        //    {
        //        DisableFire(rightHandFire);
        //        if (fireInBothHands) DisableFire(leftHandFire);
        //    }
        //    else
        //    {
        //        EnableFire(rightHandFire);
        //        if (fireInBothHands) EnableFire(leftHandFire);
        //    }
        //}

        wasRightHandRaised = isRightHandCurrentlyRaised;

        if (areHandsClose)
        {
            if (isFireActive)
            {
                Debug.Log("flamethrowering");
                EnableFire(flameThrower);

            }
        }
        else
        {
            DisableFire(flameThrower);
        }
    }

    void EnableFires()
    {
        if (rightHandFire != null)
        {
            rightHandFire.Play();
            Debug.Log("isfireactive");
            isFireActive = true;
        }
        if (leftHandFire != null)
        {
            leftHandFire.Play();
            isFireActive = true;
        }
        if (flameThrower != null)
        {
            flameThrower.Play();
        }
    }
    void DisableFires()
    {
        if (rightHandFire != null)
        {
            rightHandFire.Stop();
            isFireActive = false;
        }
        if (leftHandFire != null)
        {
            leftHandFire.Stop();
            isFireActive = false;
        }
        if (flameThrower != null)
        {
            flameThrower.Stop();
        }
    }

    void EnableFire(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    void DisableFire(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}
