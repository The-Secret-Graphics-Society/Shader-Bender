using UnityEngine;

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

    public void UpdateAvatarPose(Vector3[] landmarks)
    {
        if (landmarks == null || landmarks.Length == 0)
            return;

        UpdateIKTargets(landmarks);
    }

    private void UpdateIKTargets(Vector3[] landmarks)
    {
        // Get landmark positions to calculate midpoints
        // We will get the spine as a vector which we can use for rotation.
        Vector3 leftHip = ScaleLandmarkVector(landmarks[23]);
        Vector3 rightHip = ScaleLandmarkVector(landmarks[24]);
        Vector3 leftShoulder = ScaleLandmarkVector(landmarks[11]);
        Vector3 rightShoulder = ScaleLandmarkVector(landmarks[12]);
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
        Vector3 leftWrist = ScaleLandmarkVector(landmarks[15]) + positionOffset;
        leftHandTarget.position = leftWrist;

        // Update right hand target
        Vector3 rightWrist = ScaleLandmarkVector(landmarks[16]) + positionOffset;
        rightHandTarget.position = rightWrist;

        // Update left foot target
        Vector3 leftAnkle = ScaleLandmarkVector(landmarks[27]) + positionOffset;
        leftFootTarget.position = leftAnkle;

        // Update right foot target
        Vector3 rightAnkle = ScaleLandmarkVector(landmarks[28]) + positionOffset;
        rightFootTarget.position = rightAnkle;

        // Update head target, not implemented
        // Vector3 nose = ScaleLandmarkVector(landmarks[0]) + positionOffset;
        // headTarget.position = nose;
    }

    private Vector3 ScaleLandmarkVector(Vector3 landmark)
    {
        float x = landmark.x * xScale;
        float y = landmark.y * yScale; 
        float z = landmark.z * zScale;

        return new Vector3(x, y, z);
    }
}
