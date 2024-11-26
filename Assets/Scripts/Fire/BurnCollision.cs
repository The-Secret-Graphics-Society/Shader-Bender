using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnCollision : MonoBehaviour
{
    public float burnAmount = 0.002f;

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag != "Player")
        {
            var sr = other.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                    sr.color = new Color(sr.color.r - burnAmount,
                                        sr.color.g - burnAmount,
                                        sr.color.b - burnAmount,
                                        sr.color.a);
            }
        }
    }
}
