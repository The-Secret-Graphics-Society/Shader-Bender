using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    public GameObject fireParticles;
    public GameObject burnParticles;

    public float burnDuration = 10;

    void Update()
    {
       if (fireParticles != null)
        {
        Vector3 pos = GameObject.FindGameObjectWithTag("Fire").transform.position;
        Ray ray = new Ray(pos, Vector3.down); // Cast downward
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1f)) // Adjust range as needed
        {
            if (hit.collider.CompareTag("Ground")) // Ensure it's the ground
            {
                TriggerBurnEffect(hit.point);
            }
        }
        } 
    }

    void TriggerBurnEffect(Vector3 position)
    {
        if (burnParticles != null)
        {
            // Instantiate the burn effect at the specified position
            GameObject burnEffect = Instantiate(burnParticles, position, Quaternion.identity);

            // Destroy the effect after a set duration
            Destroy(burnEffect, burnDuration);
        }
    }

    
}
