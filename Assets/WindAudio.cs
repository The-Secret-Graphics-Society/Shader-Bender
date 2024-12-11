using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindAudio : MonoBehaviour
{
    public AudioSource _windSound;
    // Start is called before the first frame update
    void Start()
    {
        _windSound = GetComponent<AudioSource>();
        _windSound.loop = false;
        
        ElementState.onAirActive += AoEActivated;
        ElementState.onElementDeactivate += AoEDeactivated;
    }

    void OnDisable()
    {
        ElementState.onAirActive -= AoEActivated;
        ElementState.onElementDeactivate -= AoEDeactivated;
    }
    private void AoEActivated()
    {
        Debug.Log("Wind sound start");
        _windSound.Play();
    }
    
    private void AoEDeactivated()
    {
        Debug.Log("Wind sound stop");
        _windSound.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
