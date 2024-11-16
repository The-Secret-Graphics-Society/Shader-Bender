using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Tasks.Components.Containers;

public class PoseLandmarkVisualiser : MonoBehaviour
{
    public PoseLandmarkerResultAnnotationController annotationController;
    public GameObject cubePrefab;

    // Indices of the pose landmarks to track
    private int[] landmarkIndices = { 0, 11, 12, 13, 14, 15, 16, 23, 24, 25, 26, 27, 28, 31, 32 };

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
