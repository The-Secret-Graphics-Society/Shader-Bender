using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;
    [SerializeField] private PoseScriptableObject _poseScriptableObject;
    public Transform startPosition;
    public Transform endPosition;
    private bool interaction;
    private bool rightHandRaised;
    public bool canSpawnMeteor = true;
    //public int meteorCount = 1;
    public MeteorSound meteorSound;
    private float time;
    private bool _meteorTimerStarted = false;


    void Start()
    {
        interaction = false;

    }

    void Update()
    {
        if (_poseScriptableObject.isLeftHandAboveShoulder && !_poseScriptableObject.isRightHandAboveShoulder)
        {
            rightHandRaised = true;
            if(!_meteorTimerStarted) {
                time = Time.time;
                _meteorTimerStarted = true; 
            }
            //Debug.Log("right hand raised");
        }
        if(!_poseScriptableObject.isLeftHandAboveShoulder && rightHandRaised)
            {
                _meteorTimerStarted = false;
                interaction = true;
                rightHandRaised = false;
                //Debug.Log("right hand lowered");
            }
        // When the interaction criteria is met, we spawn in the fireEruption
        if (interaction)
        {
            if(canSpawnMeteor && (Time.time - time) > 2.0f)
            {
                MeteorShower();
            }
        interaction = false;
        }
    }

    void MeteorShower()
    {
        var startPos = startPosition.position;
        GameObject objVFX = Instantiate(meteorPrefab, startPos, Quaternion.identity) as GameObject;
        
        if (objVFX != null)
        {
            meteorSound.StartFalling();
        }

        if (objVFX.TryGetComponent<MeteorMovement>(out MeteorMovement meteorMovement))
            {
                meteorMovement.meteorSpawner = this;
                canSpawnMeteor = false;
            }
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
