using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject vfx;
    public Transform startPosition;
    public Transform endPosition;
    private bool interaction;
    public int meteorCount = 1;

    void Start()
    {
        interaction = false;
    }

    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            interaction = true;
        }
        // When the interaction criteria is met, we spawn in the fireEruption
        if (interaction)
        {
            MeteorShower();
            interaction = false;
        }
    }

    void MeteorShower()
    {
        var startPos = startPosition.position;
        GameObject objVFX = Instantiate(vfx, startPos, Quaternion.identity) as GameObject;
        var endPos = endPosition.position;
        Rotation(objVFX, endPos);
    }

    void Rotation(GameObject obj, Vector3 destination)
    {
        var direction = destination - obj.transform.position;
        var rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp (obj.transform.rotation, rotation, 1);
    }
}
