using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;

namespace Mediapipe.Unity
{
    public class PoseLandmarkerResultAnnotationController : AnnotationController<MultiPoseLandmarkListWithMaskAnnotation>
    {
        public delegate void PoseLandmarksUpdated(List<Mediapipe.Tasks.Components.Containers.Landmark> landmarks);
        public event PoseLandmarksUpdated OnPoseLandmarksUpdated;
        [SerializeField] private bool _visualizeZ = false;

        private readonly object _currentTargetLock = new object();
        private PoseLandmarkerResult _currentTarget;

        public void InitScreen(int maskWidth, int maskHeight) => annotation.InitMask(maskWidth, maskHeight);

        public void DrawNow(PoseLandmarkerResult target)
        {
            target.CloneTo(ref _currentTarget);
            SyncNow();
        }

        public void DrawLater(PoseLandmarkerResult target) => UpdateCurrentTarget(target);

        private void ReadMask(IReadOnlyList<Image> segmentationMasks) => annotation.ReadMask(segmentationMasks, isMirrored);

        protected void UpdateCurrentTarget(PoseLandmarkerResult newTarget)
        {
            lock (_currentTargetLock)
            {
                newTarget.CloneTo(ref _currentTarget);
                isStale = true;
            }
        }

        protected override void SyncNow()
        {
            lock (_currentTargetLock)
            {
                isStale = false;
                if (_currentTarget.segmentationMasks != null)
                {
                    ReadMask(_currentTarget.segmentationMasks);
                    // TODO: stop disposing masks here
                    foreach (var mask in _currentTarget.segmentationMasks)
                    {
                        mask.Dispose();
                    }
                }

                if (_currentTarget.poseLandmarks != null && _currentTarget.poseLandmarks.Count > 0)
                {
                    //annotation.Draw(_currentTarget.poseLandmarks, _visualizeZ);
                    List<Landmarks> landmarks = _currentTarget.poseWorldLandmarks;
                    OnPoseLandmarksUpdated?.Invoke(landmarks[0].landmarks);
                }
            }
        }
    }
}
