using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHand : MonoBehaviour
{
    public ParticleSystem fireParticles;
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    private bool rightHandRaised;
    private Vector3 handPosition;
    private Transform handPos;

    void Start()
    {
        rightHandRaised = false;
    }

    void Update()
    {
        if (_poseScriptableObject.isLeftHandAboveShoulder && !_poseScriptableObject.isRightHandAboveShoulder)
        {
            rightHandRaised = true;
            //Debug.Log("right hand raised");
        }
        else
        {
            rightHandRaised = false;
            //Debug.Log("right hand lowered");
        }
        // When the interaction criteria is met, we spawn in the fireEruption
        if (rightHandRaised)
        {
            //Fire();
        }
    }
    /*
    void Fire()
    {
        handPosition = _poseScriptableObject.GetCurrentLeftHandPosition();
        Debug.Log("Current Hand Position: " + handPosition);

        Instantiate(fireParticles, handPosition, Quaternion.identity);
    }
    */
}
