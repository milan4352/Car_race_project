using System.Collections.Generic;
using UnityEngine;

namespace DrawAndRace.TrackEditor
{
    /// <summary>
    /// Master coordinator for the Track Editor workflow: connects 2D canvas drawing inputs
    /// to track validation, 3D spline generation, procedural road extrusion, checkpoint placement,
    /// and environment prop scattering.
    /// </summary>
    [RequireComponent(typeof(TrackSplineGenerator), typeof(RoadMeshExtruder), typeof(CheckpointGenerator))]
    public class TrackEditorManager : MonoBehaviour
    {
        [Header("Editor Components")]
        [SerializeField] private TrackDrawingCanvas _drawingCanvas;
        [SerializeField] private Camera _editorCamera;

        [Header("Track Configuration")]
        [SerializeField] private float _trackWidth = 8.0f;
        [SerializeField] private float _maxLoopGapMeters = 5.0f;

        private TrackSplineGenerator _splineGenerator;
        private RoadMeshExtruder _roadExtruder;
        private CheckpointGenerator _checkpointGenerator;
        private ProceduralEnvironmentScatterer _environmentScatterer;

        private void Awake()
        {
            _splineGenerator = GetComponent<TrackSplineGenerator>();
            _roadExtruder = GetComponent<RoadMeshExtruder>();
            _checkpointGenerator = GetComponent<CheckpointGenerator>();
            _environmentScatterer = GetComponent<ProceduralEnvironmentScatterer>();

            if (_drawingCanvas == null) _drawingCanvas = FindFirstObjectByType<TrackDrawingCanvas>();
            if (_editorCamera == null) _editorCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (_drawingCanvas == null) _drawingCanvas = FindFirstObjectByType<TrackDrawingCanvas>();
            if (_drawingCanvas != null)
            {
                _drawingCanvas.OnDrawingCompleted += HandleDrawingCompleted;
            }
        }

        private void OnDisable()
        {
            if (_drawingCanvas != null)
            {
                _drawingCanvas.OnDrawingCompleted -= HandleDrawingCompleted;
            }
        }

        private void HandleDrawingCompleted(IReadOnlyList<Vector2> screenPoints)
        {
            if (screenPoints == null || screenPoints.Count < 5)
            {
                Debug.LogWarning("[TrackEditorManager] Drawing cancelled: Insufficient points.");
                return;
            }

            // 1. Convert Screen Points to 3D World Points (Y=0)
            List<Vector3> worldPoints = _splineGenerator.ConvertScreenPointsToWorld(screenPoints, _editorCamera);

            // 2. Validate Track (Closed Loop & Self-Intersection)
            TrackValidationResult validation = TrackValidator.ValidateTrack(worldPoints, _maxLoopGapMeters);
            if (!validation.IsValid)
            {
                Debug.LogError($"[TrackEditorManager] Track Validation Failed: {validation.ErrorMessage}");
                return;
            }

            // 3. Downsample & Simplify Point Sequence
            List<Vector3> simplifiedPoints = TrackValidator.DownsamplePoints(worldPoints);

            // 4. Generate 3D Spline
            _splineGenerator.GenerateSplineFromWorldPoints(simplifiedPoints, isClosedLoop: true);

            // 5. Extrude Procedural 3D Road Mesh
            _roadExtruder.RoadWidth = _trackWidth;
            _roadExtruder.ExtrudeRoad(_splineGenerator.SplineContainer);

            // 6. Generate Checkpoints (>30 degree heading change)
            _checkpointGenerator.GenerateCheckpoints(_splineGenerator.SplineContainer, _trackWidth);

            // 7. Scatter Procedural Environment Props
            if (_environmentScatterer != null)
            {
                _environmentScatterer.ScatterEnvironment(_splineGenerator.SplineContainer, _trackWidth);
            }

            Debug.Log("[TrackEditorManager] Track successfully generated, extruded, and dressed with environment props!");
        }

        public void ClearCurrentTrack()
        {
            if (_drawingCanvas != null) _drawingCanvas.ClearDrawing();
            if (_checkpointGenerator != null) _checkpointGenerator.ClearCheckpoints();
            if (_environmentScatterer != null) _environmentScatterer.ClearEnvironment();
        }
    }
}
