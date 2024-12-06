using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Tasks.Components.Containers;
using System.Collections;
using UnityEditor;
using System.Net.NetworkInformation;
using System;
using UnityEngine.ProBuilder.Shapes;

[CustomEditor(typeof(PoseManager))] 
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
    private int[] landmarkIndices = { 0, 7, 8, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 31, 32 };

    // Arrays to hold landmark cubes, Kalman filters, and updated positions
    private GameObject[] landmarkCubes = new GameObject[33];
    private KalmanFilter[] landmarkFilters = new KalmanFilter[33];
    private Vector3[] landmarkPositions = new Vector3[33];

    float fistThreshold;
    float pointThreshold;
    float pinkyThreshold;
    float thumbThreshold;


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



            // Disable rendering cubes representing hand or ear landmarks or if we don't want to render them
            if (index == 7 || index == 8 || (index >= 17 && index <= 22) || !renderCubes)
            {
                if (cube.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                {
                    meshRenderer.enabled = false;
                }
            }
        }

        // Subscribe to the landmarks updated event
        annotationController.OnPoseLandmarksUpdated += OnPoseLandmarksUpdated;

        // Start the calibration process
        StartCoroutine(StartupCalibrateToPlayer(5));
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

            // Offset based on calibration

            if (poseScriptableObject.isCalibrated)
            {
                // Get the distance to floor based on the foot closest to the floor (cannot jump)
                float leftFootDist = poseScriptableObject.leftFootPosition.y - poseScriptableObject.floorHeight;
                float rightFoot = poseScriptableObject.rightFootPosition.y - poseScriptableObject.floorHeight;

                if (Mathf.Abs(leftFootDist) < Mathf.Abs(rightFoot))
                {
                    worldPosition.y -= leftFootDist;
                }
                else
                {
                    worldPosition.y -= rightFoot;
                }

                float distanceToFloor = Mathf.Min(Mathf.Abs(poseScriptableObject.leftFootPosition.y - poseScriptableObject.floorHeight), 
                                                      Mathf.Abs(poseScriptableObject.rightFootPosition.y - poseScriptableObject.floorHeight));

                worldPosition.y -= distanceToFloor;
            }

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
            poseScriptableObject.isLeftFistClenched = DetectFist(leftHand, landmarkPositions[15], landmarkPositions[17], landmarkPositions[19], landmarkPositions[21], true);
        }
        GameObject rightHand = landmarkCubes[16];
        if (rightHand != null)
        {
            poseScriptableObject.isRightFistClenched = DetectFist(rightHand, landmarkPositions[16], landmarkPositions[18], landmarkPositions[20], landmarkPositions[22], false);
        }

        // Hand above shoulder detection, not fully implemented?
        poseScriptableObject.isLeftHandAboveShoulder = landmarkPositions[11].y < landmarkPositions[15].y;
        poseScriptableObject.isRightHandAboveShoulder = landmarkPositions[12].y < landmarkPositions[16].y;

        // check for close hands
        poseScriptableObject.closeHands = DetectCloseHands(landmarkPositions[15], landmarkPositions[16]);

        // check for extended arms
        poseScriptableObject.leftArmExtended = DetectExtendedArm(landmarkPositions[11], landmarkPositions[15]);
        poseScriptableObject.rightArmExtended = DetectExtendedArm(landmarkPositions[12], landmarkPositions[16]);

        // Foot grounded detection
        if (poseScriptableObject.calibrated)
        {
            poseScriptableObject.isLeftFootGrounded = isFootGrounded(landmarkPositions[31], poseScriptableObject.floorHeight);
            poseScriptableObject.isRightFootGrounded = isFootGrounded(landmarkPositions[32], poseScriptableObject.floorHeight);
        }

        // Update hand and foot positions
        poseScriptableObject.UpdateLeftHandPosition(landmarkPositions[15]);
        poseScriptableObject.UpdateRightHandPosition(landmarkPositions[16]);
        poseScriptableObject.leftFootPosition = landmarkPositions[31];
        poseScriptableObject.rightFootPosition = landmarkPositions[32];

        poseScriptableObject.UpdateChestPosition((landmarkPositions[11] + landmarkPositions[12]) / 2f );

        // Update the rotations of the hands, not fully implemented, just using a vector from the elbow to the wrist
        poseScriptableObject.leftHandRotation = Quaternion.LookRotation((landmarkPositions[15] - landmarkPositions[13]).normalized);
        poseScriptableObject.rightHandRotation = Quaternion.LookRotation((landmarkPositions[16] - landmarkPositions[14]).normalized);

        // Update the avatar pose
        if (avatarIKController != null)
        {
            if (avatarIKController.enabled) avatarIKController.UpdateAvatarPose(landmarkPositions);
        }
    }

    private bool DetectFist(GameObject wristCube, Vector3 wrist, Vector3 pinky, Vector3 index, Vector3 thumb, bool left)
    {
        // Calculate distances between wrist and finger landmarks
        float pinkyDistance = Vector3.Distance(wrist, pinky);
        float indexDistance = Vector3.Distance(wrist, index);
        float thumbDistance = Vector3.Distance(wrist, thumb);

        // Calculate a dynamic threshold based on hand size (distance between wrist and middle of fingers)
        // float fistThreshold = (pinkyDistance + indexDistance + thumbDistance) / 3.0f * handSizeFactor;
        // This should be done at calibration, with the size of the open fist used

        // if not calibrated
        if (!poseScriptableObject.isCalibrated)
        {
            fistThreshold = handSizeFactor;
        }
        if (left)
        {
            poseScriptableObject.isLeftPinkyExtended = false;
            poseScriptableObject.isLeftIndexExtended = false;
            poseScriptableObject.isLeftThumbExtended = false;
        }
        else
        {
            poseScriptableObject.isRightPinkyExtended = false;
            poseScriptableObject.isRightIndexExtended = false;
            poseScriptableObject.isRightThumbExtended = false;
        }

        // Check if each finger is curled in (distance below threshold) and
        // decide if it's a fist based on the number of curled fingers
        int curledFingers = 0;
        if (pinkyDistance < pinkyThreshold)
        {
            if (left)
            {
                poseScriptableObject.isLeftPinkyExtended = true;
            }
            else
            {
                poseScriptableObject.isRightPinkyExtended = true;
            }
            curledFingers++;
        }
        if (indexDistance < pointThreshold)
        {
            if (left)
            {
                poseScriptableObject.isLeftIndexExtended = true;
            }
            else
            {
                poseScriptableObject.isLeftIndexExtended = true;
            }
            curledFingers++;
        }
        if (thumbDistance < thumbThreshold)
        {
            if (left)
            {
                poseScriptableObject.isLeftThumbExtended = true;
            }
            else
            {
                poseScriptableObject.isRightThumbExtended = true;
            }
            poseScriptableObject.isLeftThumbExtended = true;
            curledFingers++;
        }
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


    private bool DetectCloseHands(Vector3 leftHand, Vector3 rightHand)
    {
        return Vector3.Distance(leftHand, rightHand) < 0.25f;
    }

    private bool DetectExtendedArm(Vector3 shoulder, Vector3 wrist)
    {
        return Vector3.Distance(shoulder, wrist) > 0.3f;
    }

    // Check if feet are in contact with the ground
    private bool isFootGrounded(Vector3 foot, float floorHeight = -0.46f) {
        if (poseScriptableObject.isCalibrated)
        {
            return foot.y < poseScriptableObject.floorHeight + 0.1f;
        }
        else {
            return foot.y < floorHeight + 0.1f;
        }
    }

    private IEnumerator StartupCalibrateToPlayer(int seconds = 5)
    {
        poseScriptableObject.calibrating = true;
        Debug.Log("Calibrating to player...");

        for (int i = 0; i < seconds; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{seconds-i}...");
        }

        CalibrateToPlayer();
    }

    private void CalibrateToPlayer()
    {
        // Set the floor height to the lowest foot position
        poseScriptableObject.floorHeight = Mathf.Min(poseScriptableObject.leftFootPosition.y, poseScriptableObject.rightFootPosition.y);

        //poseScriptableObject.defaultHipPosition = (landmarkPositions[23] + landmarkPositions[24])/2f;

        // Set the hips to shoulder distance to the distance between the hips and shoulders
        //poseScriptableObject.hipsToShoulder = Vector3.Distance((landmarkPositions[23] + landmarkPositions[24])/2f , (landmarkPositions[12]+ landmarkPositions[11])/2f);

        // Set the screen space hips position to the hips position projected onto the screen
        //poseScriptableObject.screenspaceHipsPosition = Camera.main.WorldToScreenPoint(landmarkPositions[24]);

        // set distance between palm and thumb (HandSizeFactor)
        //poseScriptableObject.palmToThumb = Vector3.Distance(landmarkPositions[15], landmarkPositions[20]);
        // Calculate distances between wrist and finger landmarks
        float pinkyDistanceL = Vector3.Distance(landmarkPositions[15], landmarkPositions[17]);
        float indexDistanceL = Vector3.Distance(landmarkPositions[15], landmarkPositions[19]);
        float thumbDistanceL = Vector3.Distance(landmarkPositions[15], landmarkPositions[21]);

        float pinkyDistanceR = Vector3.Distance(landmarkPositions[16], landmarkPositions[18]);
        float indexDistanceR = Vector3.Distance(landmarkPositions[16], landmarkPositions[20]);
        float thumbDistanceR = Vector3.Distance(landmarkPositions[16], landmarkPositions[22]);

        // Calculate a dynamic threshold based on hand size (distance between wrist and middle of fingers) from both hands
        //fistThreshold = (pinkyDistanceL + pinkyDistanceR + indexDistanceL + indexDistanceR + thumbDistanceL + thumbDistanceR)/ 6.0f * handSizeFactor;

        pointThreshold = (indexDistanceL + indexDistanceR) / 2.0f * 0.6f;
        pinkyThreshold = (pinkyDistanceL + pinkyDistanceR) / 2.0f * 0.6f;
        thumbThreshold = (thumbDistanceL + thumbDistanceR) / 2.0f * 0.6f;

        //print point, pinky and thumb thresholds
        Debug.Log("Point threshold: " + pointThreshold);
        Debug.Log("Pinky threshold: " + pinkyThreshold);
        Debug.Log("Thumb threshold: " + thumbThreshold);

        // Set the calibrated flag to true
        poseScriptableObject.calibrating = false;
        poseScriptableObject.isCalibrated = true;
        Debug.Log("Calibrated to player!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(StartupCalibrateToPlayer());
        }
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
