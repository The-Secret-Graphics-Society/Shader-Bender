using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoseScriptableObject", menuName = "ScriptableObjects/PoseScriptableObject", order = 1)]
public class PoseScriptableObject : ScriptableObject
{
    // Hand positions
    private LinkedList<Vector3> leftHandPositions = new LinkedList<Vector3>();
    private LinkedList<Vector3> rightHandPositions = new LinkedList<Vector3>();
    public Quaternion leftHandRotation;
    public Quaternion rightHandRotation;

    // Hand states
    public bool isLeftFistClenched;
    public bool isRightFistClenched;
    public bool isLeftHandAboveShoulder;
    public bool isRightHandAboveShoulder;

    // Finger states
    public bool isLeftIndexExtended;
    public bool isRightIndexExtended;
    public bool isLeftThumbExtended;
    public bool isRightThumbExtended;
    public bool isLeftPinkyExtended;
    public bool isRightPinkyExtended;


    // Foot positions
    public Vector3 leftFootPosition;
    public Vector3 rightFootPosition;

    // Foot states
    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;

    // Calibration values
    public bool calibrated = false;
    public float floorHeight = 0;
    public float hipsToShoulder = 0;
    public Vector3 screenspaceHipsPosition = Vector3.zero;


    [SerializeField, Tooltip("Maximum size for hand position LinkedLists")]
    private float maxHandPositionHistory = 120;

    // Calibration Stats
    public float floorHeight = 0.0f;
    public bool isCalibrated = false;
    public bool calibrating = false;

    public void Initialise()
    {
        leftHandPositions.Clear();
        rightHandPositions.Clear();

        isLeftFistClenched = isRightFistClenched = false;
        isLeftHandAboveShoulder = isRightHandAboveShoulder = false;

        leftFootPosition = Vector3.zero;
        rightFootPosition = Vector3.zero;
        
        isLeftFootGrounded = isRightFootGrounded = false;

        isLeftIndexExtended = isRightIndexExtended = false;
        isLeftThumbExtended = isRightThumbExtended = false;
        isLeftPinkyExtended = isRightPinkyExtended = false;

        isCalibrated = false;
        calibrating = false;
    }

    public Vector3 GetCurrentLeftHandPosition()
    {
        return leftHandPositions.First?.Value ?? Vector3.zero;
    }

    public Vector3 GetCurrentRightHandPosition()
    {
        return rightHandPositions.First?.Value ?? Vector3.zero;
    }

    public void UpdateLeftHandPosition(Vector3 newPosition)
    {
        if (leftHandPositions.Count >= maxHandPositionHistory)
        {
            leftHandPositions.RemoveLast();
        }
        leftHandPositions.AddFirst(newPosition);
    }

    public void UpdateRightHandPosition(Vector3 newPosition)
    {
        if (rightHandPositions.Count >= maxHandPositionHistory)
        {
            rightHandPositions.RemoveLast();
        }
        rightHandPositions.AddFirst(newPosition);
    }

    public void ClearHandPositions()
    {
        leftHandPositions.Clear();
        rightHandPositions.Clear();
    }
}
