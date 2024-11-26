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

    // Foot positions
    public Vector3 leftFootPosition;
    public Vector3 rightFootPosition;

    // Foot states
    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;

    [SerializeField, Tooltip("Maximum size for hand position LinkedLists")]
    private float maxHandPositionHistory = 120;

    public void Initialise()
    {
        leftHandPositions.Clear();
        rightHandPositions.Clear();

        isLeftFistClenched = isRightFistClenched = false;
        isLeftHandAboveShoulder = isRightHandAboveShoulder = false;

        leftFootPosition = Vector3.zero;
        rightFootPosition = Vector3.zero;
        
        isLeftFootGrounded = isRightFootGrounded = false;
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
