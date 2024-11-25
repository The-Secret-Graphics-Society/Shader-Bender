using UnityEngine;

public class AvatarIKController : MonoBehaviour
{
    [Header("IK Targets")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform head_L;

    // Scale factors to adjust the size of the avatar to match the landmarks
    [Header("Scale Factors")]
    [SerializeField] private float xScale = 1.0f;
    [SerializeField] private float yScale = 1.0f;
    [SerializeField] private float zScale = 1.0f;
    [SerializeField] private Vector3 positionOffset;

    [Header("Other Attributes")]
    [SerializeField, Tooltip("Skinned mesh renderer of the character, so we can disable it if we don't get the rigging working fully.")]
    private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private bool disableAvatarRendering = false;

    private Animator avatarAnimator;

    void Start()
    {
        // Ensure the animator is assigned
        if (avatarAnimator == null)
        {
            avatarAnimator = GetComponent<Animator>();
        }

        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.enabled = !disableAvatarRendering;
        }

        positionOffset = transform.position;
    }

    public void UpdateAvatarPose(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length == 0) return;

        if (disableAvatarRendering) return;

        UpdateIKTargets(landmarks);
    }

    private void UpdateIKTargets(Vector3[] landmarks)
    {
        // Get landmark positions to calculate midpoints
        // We will get the spine as a vector which we can use for rotation.
        // Point between the ears is the head midpoint
        Vector3 leftHip = ScaleLandmarkVector(landmarks[23]);
        Vector3 rightHip = ScaleLandmarkVector(landmarks[24]);
        Vector3 leftShoulder = ScaleLandmarkVector(landmarks[11]);
        Vector3 rightShoulder = ScaleLandmarkVector(landmarks[12]);
        Vector3 hipMidpoint = (leftHip + rightHip) / 2.0f;
        Vector3 shoulderMidpoint = (leftShoulder + rightShoulder) / 2.0f;
        Vector3 bodyRight = (rightHip - leftHip).normalized;
        Vector3 bodyUp = (shoulderMidpoint - hipMidpoint).normalized;
        Vector3 bodyForward = Vector3.Cross(bodyRight, bodyUp).normalized;
        Quaternion bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);

        // Apply position and rotation to the root of the avatar
        transform.position = hipMidpoint + positionOffset;
        transform.rotation = bodyRotation;

        // Update hand targets
        leftHandTarget.position = ScaleLandmarkVector(landmarks[19]) + positionOffset;
        rightHandTarget.position = ScaleLandmarkVector(landmarks[20]) + positionOffset;

        // Update foot targets
        leftFootTarget.position = ScaleLandmarkVector(landmarks[31]) + positionOffset;
        rightFootTarget.position = ScaleLandmarkVector(landmarks[32]) + positionOffset;

        // Update head target
        // Rotation is in the neck, thus we need to rotate from there
        Vector3 headMidpoint = (ScaleLandmarkVector(landmarks[7]) + ScaleLandmarkVector(landmarks[8])) / 2.0f;
        Vector3 lookOrientation = (ScaleLandmarkVector(landmarks[0]) - headMidpoint).normalized;
        headTarget.position = head_L.position + lookOrientation + positionOffset;
    }

    private Vector3 ScaleLandmarkVector(Vector3 landmark)
    {
        float x = landmark.x * xScale;
        float y = landmark.y * yScale;
        float z = landmark.z * zScale;

        return new Vector3(x, y, z);
    }
}
