using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableWhenCalibrated : MonoBehaviour
{
    [SerializeField] private PoseScriptableObject poseScriptableObject;
    [SerializeField] private RawImage webcamImage;

    void Awake()
    {
        webcamImage = GetComponent<RawImage>();
    }

    // Should use c# events to subscribe to calibration events
    private void Update()
    {
        if (poseScriptableObject.calibrating)
        {
            // make webcam image transparent
            webcamImage.material.color = new Color(1, 1, 1, 1f);
        }
        else
        {
            // make webcam image opaque
            webcamImage.material.color = new Color(1, 1, 1, 0f);
        }

    }
}
