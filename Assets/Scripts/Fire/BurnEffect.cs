using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem part;
    [SerializeField] private List<ParticleCollisionEvent> collisionEvents;
    [SerializeField] private float burnIncrement = 0.05f;
    

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Burnable")
        {
            if (TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (renderer.material.HasFloat("_burnAmount")) 
                {

                    renderer.material.SetFloat("_burnAmount", renderer.material.GetFloat("_burnAmount") <= 1.0f ? renderer.material.GetFloat("_burnAmount") + burnIncrement : 1.0f);
                }
            }
        }
    }  
}
