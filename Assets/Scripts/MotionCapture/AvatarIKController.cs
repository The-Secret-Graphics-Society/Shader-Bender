using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class AvatarIKController : MonoBehaviour
{
    [Header("IK Targets")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform headTarget;

    // Scale factors to adjust the size of the avatar to match the landmarks
    [Header("Scale Factors")]
    [SerializeField] private float xScale = 1.0f;
    [SerializeField] private float yScale = 1.0f;
    [SerializeField] private float zScale = 1.0f;
    [SerializeField] private Vector3 positionOffset;
    private Animator avatarAnimator;

    void Start()
    {
        // Ensure the animator is assigned
        if (avatarAnimator == null)
        {
            avatarAnimator = GetComponent<Animator>();
        }

        positionOffset = transform.position;
    }

    public void UpdateAvatarPose(List<Landmark> landmarks)
    {
        if (landmarks == null || landmarks.Count == 0)
            return;

        UpdateIKTargets(landmarks);
    }

    private void UpdateIKTargets(List<Landmark> landmarks)
    {
        // Get landmark positions to calculate midpoints
        // We will get the spine as a vector which we can use for rotation.
        Vector3 leftHip = GetVectorFromLandmark(landmarks[23]);
        Vector3 rightHip = GetVectorFromLandmark(landmarks[24]);
        Vector3 leftShoulder = GetVectorFromLandmark(landmarks[11]);
        Vector3 rightShoulder = GetVectorFromLandmark(landmarks[12]);
        Vector3 shoulderMidpoint = (leftShoulder + rightShoulder) / 2.0f;
        Vector3 hipMidpoint = (leftHip + rightHip) / 2.0f;
        Vector3 bodyRight = (rightHip - leftHip).normalized;
        Vector3 bodyUp = (shoulderMidpoint - hipMidpoint).normalized;
        Vector3 bodyForward = Vector3.Cross(bodyRight, bodyUp).normalized;
        Quaternion bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);

        // Apply position and rotation to the root of the avatar
        var hipsTransform = avatarAnimator.GetBoneTransform(HumanBodyBones.Hips);
        hipsTransform.position = hipMidpoint + positionOffset;
        transform.rotation = bodyRotation;

        // Update left hand target
        Vector3 leftWrist = GetVectorFromLandmark(landmarks[15]) + positionOffset;
        leftHandTarget.position = leftWrist;

        // Update right hand target
        Vector3 rightWrist = GetVectorFromLandmark(landmarks[16]) + positionOffset;
        rightHandTarget.position = rightWrist;

        // Update left foot target
        Vector3 leftAnkle = GetVectorFromLandmark(landmarks[27]) + positionOffset;
        leftFootTarget.position = leftAnkle;

        // Update right foot target
        Vector3 rightAnkle = GetVectorFromLandmark(landmarks[28]) + positionOffset;
        rightFootTarget.position = rightAnkle;

        // Update head target, not implemented
        // Vector3 nose = GetVectorFromLandmark(landmarks[0]) + positionOffset;
        // headTarget.position = nose;
    }

    private Vector3 GetVectorFromLandmark(Landmark landmark)
    {
        float x = landmark.x * xScale;
        float y = -landmark.y * yScale; 
        float z = landmark.z * zScale;

        return new Vector3(x, y, z);
    }
}
