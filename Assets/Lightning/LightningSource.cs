using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ElementState;

public class LightningSource : MonoBehaviour
{
    public Transform[] lightningTargetPoints;
    public Transform[] lightningSpawnPoints;
    [SerializeField] private PoseScriptableObject pose;

    [SerializeField] private LSystemLightning[] lightningGroup;

    public float lightningSpawnRate = 1f;
    public float lightningRaycastRange = 5f;
    public float lightningRaycastAngleRange = 15f;
    private bool isLightningActive = false;

    private void Start()
    {
        for (int i = 0; i < lightningGroup.Length; i++)
        {
            lightningGroup[i].animating = false;

            // set all target and spawn points to inactive
            lightningTargetPoints[i].gameObject.SetActive(false);
            lightningSpawnPoints[i].gameObject.SetActive(false);
        }
        ElementState.onLightningActive += SpawnLightning;
        ElementState.onElementDeactivate += DeactivateLightning;
    }

    private void OnDestroy()
    {
        ElementState.onLightningActive -= SpawnLightning;
        ElementState.onElementDeactivate -= DeactivateLightning;
    }
    private void DeactivateLightning()
    {
        for (int i = 0; i < lightningGroup.Length; i++)
        {
            lightningGroup[i].animating = false;

            // set all target and spawn points to inactive
            lightningTargetPoints[i].gameObject.SetActive(false);
            lightningSpawnPoints[i].gameObject.SetActive(false);
        }
        isLightningActive = false;
    }

    private void SpawnLightning()
    {
        Debug.Log("lightning active");

        for (int i = 0; i < lightningGroup.Length; i++)
        {
            lightningGroup[i].animating = true;

            // set all target and spawn points to inactive
            lightningTargetPoints[i].gameObject.SetActive(true);
            lightningSpawnPoints[i].gameObject.SetActive(true);
        }
        isLightningActive = true;
        StartCoroutine(SpawnLightningCoroutine());
        StartCoroutine(TrackLightningDirection());
    }

    private IEnumerator TrackLightningDirection()
    {
        while (isLightningActive)
        {
            // track the direction of the lightning spawn point
            for (int i = 0; i < lightningSpawnPoints.Length; i++)
            {
                lightningSpawnPoints[i].position = (pose.GetCurrentLeftHandPosition() + pose.GetCurrentRightHandPosition()) / 2f;
                lightningSpawnPoints[i].rotation = pose.jointHandRotation;
            }
            // and update the target points
            yield return null;
        }
    }
    private IEnumerator SpawnLightningCoroutine()
    {
        while (isLightningActive) {
            // fire raycasts and spawn a lightning 
            // raycast source from spawnpoint forward with the angle range
            for (int i = 0; i < lightningTargetPoints.Length; i++)
            {
                yield return new WaitForSeconds(lightningSpawnRate);
                lightningTargetPoints[i].position = lightningSpawnPoints[i].position + lightningSpawnPoints[i].forward * lightningRaycastRange;

                // add a random offset vector to the target point that is orthagonal to the forward vector
                Vector3 offset = Vector3.Cross(lightningSpawnPoints[i].forward, Vector3.up) * Random.Range(-lightningRaycastAngleRange, lightningRaycastAngleRange);
                // randomize the angle of the offset vector
                offset = Quaternion.AngleAxis(Random.Range(-lightningRaycastAngleRange, lightningRaycastAngleRange), lightningSpawnPoints[i].forward) * offset;
                lightningTargetPoints[i].position += Vector3.Normalize(offset) * 2f;

                //raycast to the target point, and if an intersection is found, use that point as the target
                RaycastHit hit;
                if (Physics.Raycast(lightningSpawnPoints[i].position, lightningTargetPoints[i].position - lightningSpawnPoints[i].position, out hit, lightningRaycastRange))
                {
                    // check if hit the surface layer
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Surface"))
                    {
                        lightningTargetPoints[i].position = hit.point;
                    }
                }
            }
        }

        yield return null;
    }
}
