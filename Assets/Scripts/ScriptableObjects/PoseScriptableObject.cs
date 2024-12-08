using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoseScriptableObject", menuName = "ScriptableObjects/PoseScriptableObject", order = 1)]
public class PoseScriptableObject : ScriptableObject
{
    // Hand positions
    private LinkedList<Vector3> leftHandPositions = new LinkedList<Vector3>();
    private LinkedList<Vector3> rightHandPositions = new LinkedList<Vector3>();
    private LinkedList<Vector3> chestPositions = new LinkedList<Vector3>();
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


    // Hand states
    public bool closeHands;
    public bool leftArmExtended;
    public bool rightArmExtended;


    // Foot positions
    public Vector3 leftFootPosition;
    public Vector3 rightFootPosition;

    // Foot states
    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;

    // Calibration values
    public bool calibrated = false;
    public float hipsToShoulder = 0f;
    public float screenspaceHipsToShoulder = 0f;
    public float screenspaceToWorldspaceScale = 0f;
    public float floorHeight = 0f;
    public Vector3 screenspaceHipsPosition;
    public Vector3 screenspaceCurrentHipsPosition;

    // Calibration states
    public bool isCalibrated = false;
    public bool calibrating = false;

    [SerializeField, Tooltip("Maximum size for hand position LinkedLists")]
    private float maxPositionHistory = 120;

    public void Initialise()
    {
        leftHandPositions.Clear();
        rightHandPositions.Clear();
        chestPositions.Clear();

        isLeftFistClenched = isRightFistClenched = false;
        isLeftHandAboveShoulder = isRightHandAboveShoulder = false;

        leftFootPosition = Vector3.zero;
        rightFootPosition = Vector3.zero;
        
        isLeftFootGrounded = isRightFootGrounded = false;

        isLeftIndexExtended = isRightIndexExtended = false;
        isLeftThumbExtended = isRightThumbExtended = false;
        isLeftPinkyExtended = isRightPinkyExtended = false;
        leftArmExtended = rightArmExtended = false;
        closeHands = false;

        hipsToShoulder = screenspaceHipsToShoulder = screenspaceToWorldspaceScale = floorHeight = 0f;

        screenspaceHipsPosition = screenspaceCurrentHipsPosition = Vector3.zero;
        isCalibrated = calibrating = false;
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
        if (leftHandPositions.Count >= maxPositionHistory)
        {
            leftHandPositions.RemoveLast();
        }
        leftHandPositions.AddFirst(newPosition);
    }

    public void UpdateRightHandPosition(Vector3 newPosition)
    {
        if (rightHandPositions.Count >= maxPositionHistory)
        {
            rightHandPositions.RemoveLast();
        }
        rightHandPositions.AddFirst(newPosition);
    }

    public void UpdateChestPosition(Vector3 newPosition)
    {
        if (chestPositions.Count >= maxPositionHistory)
        {
            chestPositions.RemoveLast();
        }
        chestPositions.AddFirst(newPosition);
    }

    public void ClearHandPositions()
    {
        leftHandPositions.Clear();
        rightHandPositions.Clear();
    }

    public void ClearChestPosition()
    {
        chestPositions.Clear();
    }

    public Vector3 GetCurrentChestPosition()
    {
        return chestPositions.First?.Value ?? Vector3.zero;
    }
}
