using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEWindPlacer : MonoBehaviour
{
    public Transform AoEWindDome;
    void Start()
    {
        // Do nothing lmfao
    }

    void FixedUpdate() {
        if (AoEWindDome != null) AoEWindDome.position = transform.position;
    }
}
