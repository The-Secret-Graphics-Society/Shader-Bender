using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlantBurn : MonoBehaviour
{
    public PoseScriptableObject poseScriptableObject;
    public GameObject[] plants;
    public bool reset = false;

    private void Update()
    {
        if (poseScriptableObject.calibrating)
        {
            foreach (GameObject plant in plants)
            {
                if (plant.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    Debug.Log("resetting burn");
                    if (renderer.materials[1].HasFloat("_burnAmount"))
                    {
                        Debug.Log("reset");

                        renderer.materials[1].SetFloat("_burnAmount", 0.0f);
                    }
                }
            }
        }
    }
}
