using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningAudio : MonoBehaviour
{
    private void Awake()
    {
        ElementState.onLightningActive += PlayLightningAudio;
        ElementState.onElementDeactivate += StopLightningAudio;
    }

    private void OnDestroy()
    {
        ElementState.onLightningActive -= PlayLightningAudio;
        ElementState.onElementDeactivate -= StopLightningAudio;
    }

    private void PlayLightningAudio()
    {
        Debug.Log("lightning active");
        // Play audio here
        GetComponent<AudioSource>().Play();
    }

    private void StopLightningAudio()
    {
        // Stop audio here
        GetComponent<AudioSource>().Stop();
    }

}
