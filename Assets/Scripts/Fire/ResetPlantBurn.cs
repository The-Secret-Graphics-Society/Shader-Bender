using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlantBurn : MonoBehaviour
{
    public PoseScriptableObject poseScriptableObject;
    public GameObject[] plants;
    public bool reset = false;

    void Awake()
    {
        StartCoroutine(ResetPlants());
    }

    private void Update()
    {
        if (poseScriptableObject.calibrating)
        {
            foreach (GameObject plant in plants)
            {
                if (plant.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    //Debug.Log("resetting burn");
                    if (renderer.materials[1].HasFloat("_burnAmount"))
                    {
                        //Debug.Log("reset");

                        renderer.materials[1].SetFloat("_burnAmount", 0.0f);
                    }
                }
            }
        }
    }

    IEnumerator ResetPlants()
    {
        while(true)
        {
            yield return new WaitForSeconds(15);
            foreach (GameObject plant in plants)
            {
                if (plant.TryGetComponent<Renderer>(out Renderer burnRenderer))
                {
                    if (burnRenderer.materials[1].GetFloat("_burnAmount") > 0.01f)
                    {
                        burnRenderer.materials[1].SetFloat("_burnAmount", 0.0f);
                    }
                }
            }
        }
    }
}
