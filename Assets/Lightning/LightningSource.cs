using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSource : MonoBehaviour
{
    public Transform[] lightningTargetPoints;
    public Transform lightningSpawnPoint;

    public float lightningSpawnRate = 1f;
    public float lightningRaycastRange = 5f;
    public float lightningRaycastAngleRange = 15f;

    private void Start()
    {
        SpawnLightning();
    }

    public void SpawnLightning()
    {
        StartCoroutine(SpawnLightningCoroutine());
    }

    IEnumerator SpawnLightningCoroutine()
    {
        // fire raycasts and spawn a lightning 
        // raycast source from spawnpoint forward with the angle range
        Vector3 raycastDirection = lightningSpawnPoint.forward;
        for (int i = 0; i < lightningTargetPoints.Length; i++)
        {
            yield return new WaitForSeconds(lightningSpawnRate);
            //fire raycast
            RaycastHit hit;
            if (Physics.Raycast(lightningSpawnPoint.position, raycastDirection, out hit, lightningRaycastRange))
            {
                lightningTargetPoints[i].position = hit.point;
            }
            else
            {
                lightningTargetPoints[i].position = lightningSpawnPoint.position + raycastDirection * lightningRaycastRange;
            }
        }
        yield return null;
    }
}
