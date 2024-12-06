using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ElementState;

public class LightningSource : MonoBehaviour
{
    public Transform[] lightningTargetPoints;
    public Transform lightningSpawnPoint;
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
        }
        isLightningActive = false;
    }

    private void SpawnLightning()
    {
        Debug.Log("lightning active");

        for (int i = 0; i < lightningGroup.Length; i++)
        {
            lightningGroup[i].animating = true;
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
            lightningSpawnPoint.position = (pose.GetCurrentLeftHandPosition() + pose.GetCurrentRightHandPosition()) / 2f;
            lightningSpawnPoint.forward = -(lightningSpawnPoint.position -pose.GetCurrentChestPosition()).normalized;
            // and update the target points
            yield return null;
        }
    }
    private IEnumerator SpawnLightningCoroutine()
    {
        while (isLightningActive) {
            // fire raycasts and spawn a lightning 
            // raycast source from spawnpoint forward with the angle range
            Vector3 raycastDirection = lightningSpawnPoint.forward;
            for (int i = 0; i < lightningTargetPoints.Length; i++)
            {
                yield return new WaitForSeconds(lightningSpawnRate);
                //fire raycast
                //RaycastHit hit;
                lightningTargetPoints[i].position = lightningSpawnPoint.position + raycastDirection * lightningRaycastRange;
                //if (Physics.Raycast(lightningSpawnPoint.position, raycastDirection, out hit, lightningRaycastRange))
                //{
                //    lightningTargetPoints[i].position = hit.point;
                //}
                //else
                //{
                //    lightningTargetPoints[i].position = lightningSpawnPoint.position + raycastDirection * lightningRaycastRange;
                //}
            }
        }

        yield return null;
    }
}
