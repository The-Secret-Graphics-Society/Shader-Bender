using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSound : MonoBehaviour
{
    public AudioSource source;          
    public AudioClip meteorFall;        
    public AudioClip meteorImpact;      

    private bool hasFallen = false;

    // Called when the meteor starts falling
    public void StartFalling()
    {
        source.PlayOneShot(meteorFall);
    }

    // Called when the meteor impacts (if the impact is event-driven)
    public void Impact()
    {
        source.PlayOneShot(meteorImpact);
    }
}
