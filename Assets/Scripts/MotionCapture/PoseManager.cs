using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Tasks.Components.Containers;

public class PoseManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PoseLandmarkerResultAnnotationController annotationController;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private AvatarIKController avatarIKController;
    [SerializeField] private PoseScriptableObject poseScriptableObject;

    [Header("Adjustable attributes")]
    [SerializeField] private float handSizeFactor = 0.4f;
    public bool renderCubes = true;
    [SerializeField, Tooltip("Kalman process noise, lower values make the filtering more resistant to sudden change.")]
    private float processNoise = 0.0001f;
    [SerializeField, Tooltip("Kalman measurement noise, higher values make the filtering trust measurements less.")]
    private float measurementNoise = 0.01f;

    // Indices of the pose landmarks to track
    private int[] landmarkIndices = { 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 31, 32 };

    // Arrays to hold landmark cubes, Kalman filters, and updated positions
    private GameObject[] landmarkCubes = new GameObject[33];
    private KalmanFilter[] landmarkFilters = new KalmanFilter[33];
    private Vector3[] landmarkPositions = new Vector3[33];


    void Awake()
    {
        if (annotationController == null) Debug.LogError("Annotation Controller not assigned to the PoseManager!");
        if (cubePrefab == null) Debug.LogError("Cube prefab not assigned to the PoseManager!");
        if (avatarIKController == null) Debug.LogError("AvatarIKController not assigned to the PoseManager!");
        if (poseScriptableObject == null) Debug.LogError("Pose scriptable object not assigned to the PoseManager!");
    }

    void Start()
    {
        poseScriptableObject.Initialise();

        // Initialise cubes and Kalman filtering for each landmark
        foreach (int index in landmarkIndices)
        {
            GameObject cube = Instantiate(cubePrefab);
            cube.name = $"Landmark_{index}";
            landmarkCubes[index] = cube;

            KalmanFilter filter = new KalmanFilter(Vector3.zero, processNoise, measurementNoise);
            landmarkFilters[index] = filter;

            // Disable rendering cubes representing hand landmarks or if we don't want to render them
            if ((index >= 17 && index <= 22) || !renderCubes)
            {
                if (cube.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                {
                    meshRenderer.enabled = false;
                }
            }
        }

        // Subscribe to the landmarks updated event
        annotationController.OnPoseLandmarksUpdated += OnPoseLandmarksUpdated;
    }

    void OnPoseLandmarksUpdated(List<Landmark> landmarks)
    {
        if (landmarks == null || landmarks.Count == 0)
            return;

        foreach (int index in landmarkIndices)
        {
            if (index >= landmarks.Count)
                continue;

            var landmark = landmarks[index];
            Vector3 worldPosition = new Vector3(landmark.x, -landmark.y, landmark.z);

            // Apply Kalman filtering
            KalmanFilter filter = landmarkFilters[index];
            if (filter != null)
            {
                Vector3 smoothedPosition = landmarkPositions[index] = filter.Update(worldPosition);

                GameObject cube = landmarkCubes[index];
                if (cube != null)
                {
                    cube.transform.position = smoothedPosition;
                }
            }
        }

        // Fist detection, not fully implemented
        GameObject leftHand = landmarkCubes[15];
        if (leftHand != null)
        {
            poseScriptableObject.isLeftFistClenched = DetectFist(leftHand, landmarkPositions[15], landmarkPositions[17], landmarkPositions[19], landmarkPositions[21]);
        }
        GameObject rightHand = landmarkCubes[16];
        if (rightHand != null)
        {
            poseScriptableObject.isRightFistClenched = DetectFist(rightHand, landmarkPositions[16], landmarkPositions[18], landmarkPositions[20], landmarkPositions[22]);
        }

        // Hand above shoulder detection, not fully implemented?
        poseScriptableObject.isLeftHandAboveShoulder = landmarkPositions[11].y < landmarkPositions[15].y;
        poseScriptableObject.isRightHandAboveShoulder = landmarkPositions[12].y < landmarkPositions[16].y;

        // Foot grounded detection, not fully implemented
        poseScriptableObject.isLeftFootGrounded = isFootGrounded(landmarkPositions[31]);
        poseScriptableObject.isRightFootGrounded = isFootGrounded(landmarkPositions[32]);

        // Update hand and foot positions
        poseScriptableObject.UpdateLeftHandPosition(landmarkPositions[15]);
        poseScriptableObject.UpdateRightHandPosition(landmarkPositions[16]);
        poseScriptableObject.leftFootPosition = landmarkPositions[31];
        poseScriptableObject.leftFootPosition = landmarkPositions[32];

        // Update the avatar pose
        if (avatarIKController != null)
        {
            avatarIKController.UpdateAvatarPose(landmarkPositions);
        }
    }

    private bool DetectFist(GameObject wristCube, Vector3 wrist, Vector3 pinky, Vector3 index, Vector3 thumb)
    {
        // Calculate distances between wrist and finger landmarks
        float pinkyDistance = Vector3.Distance(wrist, pinky);
        float indexDistance = Vector3.Distance(wrist, index);
        float thumbDistance = Vector3.Distance(wrist, thumb);

        // Calculate a dynamic threshold based on hand size (distance between wrist and middle of fingers)
        // float fistThreshold = (pinkyDistance + indexDistance + thumbDistance) / 3.0f * handSizeFactor;
        // This should be done at calibration, with the size of the open fist used
        float fistThreshold = handSizeFactor;

        // Check if each finger is curled in (distance below threshold) and
        // decide if it's a fist based on the number of curled fingers
        int curledFingers = 0;
        if (pinkyDistance < fistThreshold) curledFingers++;
        if (indexDistance < fistThreshold) curledFingers++;
        if (thumbDistance < fistThreshold) curledFingers++;
        bool isFist = curledFingers >= 2;

        if (wristCube.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            if (isFist)
            {
                meshRenderer.material.color = Color.green;
            }
            else
            {
                meshRenderer.material.color = Color.red;
            }
        }

        return isFist;
    }

    // NOT IMPLEMENTED
    private bool isFootGrounded(Vector3 foot, float floorHeight = -0.46f) {
        return foot.y < floorHeight + 0.1f;
    }

    void OnDestroy()
    {
        // Unsubscribe from the event
        if (annotationController != null)
        {
            annotationController.OnPoseLandmarksUpdated -= OnPoseLandmarksUpdated;
        }
    }
}
