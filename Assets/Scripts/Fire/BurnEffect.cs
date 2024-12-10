using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private List<ParticleCollisionEvent> collisionEvents;
    [SerializeField] private float burnIncrement = 0.05f;
    

    void Start()
    {
        fireParticles = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Burnable")
        {
            if (other.TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (renderer.materials[1].HasFloat("_burnAmount")) 
                {
                    Debug.Log("hit");
                    if (renderer.materials[1].GetFloat("_burnAmount") <= 1.0f)
                    {
                        renderer.materials[1].SetFloat("_burnAmount", renderer.materials[1].GetFloat("_burnAmount") + renderer.material.GetFloat("_burnAmount") + burnIncrement);
                    }
                    else
                    {
                        renderer.materials[1].SetFloat("_burnAmount", 1.0f);
                    }
                }
            }
        }
    }  
}
