using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Tasks.Components.Containers;

public class PoseLandmarkVisualiser : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PoseLandmarkerResultAnnotationController annotationController;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private AvatarIKController avatarIKController;

    [Header("Adjustable attributes")]
    [SerializeField] private float handSizeFactor = 0.4f;

    // Indices of the pose landmarks to track
    private int[] landmarkIndices = { 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 31, 32 };

    // Dictionary to hold landmark cubes
    private Dictionary<int, GameObject> landmarkCubes = new Dictionary<int, GameObject>();

    void Start()
    {
        // Initialise cubes for each landmark
        foreach (int index in landmarkIndices)
        {
            GameObject cube = Instantiate(cubePrefab);
            cube.name = $"Landmark_{index}";
            landmarkCubes.Add(index, cube);

            // Disable rendering cubes representing hand landmarks
            if (index >= 17 && index <= 22)
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

            if (landmarkCubes.TryGetValue(index, out GameObject cube))
            {
                cube.transform.position = worldPosition;
            }
        }

        // Fist detection
        if (landmarkCubes.TryGetValue(15, out GameObject leftHand))
        {
            DetectFist(leftHand, landmarks[15], landmarks[17], landmarks[19], landmarks[21]);
        }
        if (landmarkCubes.TryGetValue(16, out GameObject rightHand))
        {
            DetectFist(rightHand, landmarks[16], landmarks[18], landmarks[20], landmarks[22]);
        }

        // Update the avatar pose
        if (avatarIKController != null)
        {
            avatarIKController.UpdateAvatarPose(landmarks);
        }
    }

    void DetectFist(GameObject wristCube, Landmark wrist, Landmark pinky, Landmark index, Landmark thumb)
    {
        // Calculate distances between wrist and finger landmarks
        Vector3 wristPos = new Vector3(wrist.x, -wrist.y, wrist.z);
        Vector3 pinkyPos = new Vector3(pinky.x, -pinky.y, pinky.z);
        Vector3 indexPos = new Vector3(index.x, -index.y, index.z);
        Vector3 thumbPos = new Vector3(thumb.x, -thumb.y, thumb.z);
        float pinkyDistance = Vector3.Distance(wristPos, pinkyPos);
        float indexDistance = Vector3.Distance(wristPos, indexPos);
        float thumbDistance = Vector3.Distance(wristPos, thumbPos);

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
