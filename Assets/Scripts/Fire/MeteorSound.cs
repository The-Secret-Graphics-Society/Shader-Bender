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
        if (!hasFallen)
        {
            source.PlayOneShot(meteorFall);
            hasFallen = true;

            // Start a coroutine to play the impact sound after the fall sound finishes
            StartCoroutine(PlayImpactAfterFall());
        }
    }

    // Called when the meteor impacts (if the impact is event-driven)
    public void Impact()
    {
        if (!hasFallen) return; // Ensure falling sound has played first
        source.PlayOneShot(meteorImpact);
    }

    private IEnumerator PlayImpactAfterFall()
    {
        // Wait for the meteorFall sound to complete
        yield return new WaitForSeconds(meteorFall.length);

        // Trigger the impact logic (you can replace this with an actual impact event if applicable)
        Impact();
    }
}
