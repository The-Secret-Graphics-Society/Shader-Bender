using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntensityFlicker : MonoBehaviour
{
    public float flickerSpeed = 1f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1f;

    // Update is called once per frame
    void Update()
    {
        UpdateLightIntensity();
    }

    private void UpdateLightIntensity()
    {
        Light light = GetComponent<Light>();
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * flickerSpeed, 1));
    }
}
