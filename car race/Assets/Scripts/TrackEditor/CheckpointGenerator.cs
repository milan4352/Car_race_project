using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace DrawAndRace.TrackEditor
{
    public struct CheckpointNode
    {
        public int Index;
        public Vector3 Position;
        public Quaternion Rotation;
        public float DistanceAlongSpline;
    }

    /// <summary>
    /// Auto-calculates and instantiates checkpoint triggers along a spline
    /// whenever cumulative heading change exceeds 30 degrees.
    /// </summary>
    public class CheckpointGenerator : MonoBehaviour
    {
        [Header("Checkpoint Placement Parameters")]
        [SerializeField] private float _headingAngleThresholdDegrees = 30.0f;
        [SerializeField] private float _minDistanceBetweenCheckpoints = 15.0f;
        [SerializeField] private GameObject _checkpointVisualPrefab;

        private readonly List<CheckpointNode> _checkpoints = new List<CheckpointNode>();
        private readonly List<GameObject> _instantiatedVisuals = new List<GameObject>();

        public IReadOnlyList<CheckpointNode> Checkpoints => _checkpoints;

        /// <summary>
        /// Generates checkpoints along the spline based on turn heading deltas.
        /// </summary>
        public List<CheckpointNode> GenerateCheckpoints(SplineContainer splineContainer, float roadWidth = 8.0f)
        {
            ClearCheckpoints();
            if (splineContainer == null || splineContainer.Spline == null) return _checkpoints;

            Spline spline = splineContainer.Spline;
            float totalLength = splineContainer.CalculateLength();
            int sampleCount = Mathf.Max(30, Mathf.RoundToInt(totalLength));

            Vector3 lastCheckpointPos = Vector3.zero;
            Vector3 lastHeading = Vector3.forward;
            float accumulatedHeadingDelta = 0f;
            int checkpointIndex = 0;

            // Always add Start/Finish Line checkpoint at t=0
            splineContainer.Evaluate(0f, out float3 startPos, out float3 startFwd, out float3 startUp);
            Vector3 sPos = startPos;
            Vector3 sFwd = math.normalize(startFwd);
            Quaternion sRot = Quaternion.LookRotation(sFwd, startUp);

            AddCheckpointNode(checkpointIndex++, sPos, sRot, 0f, roadWidth);
            lastCheckpointPos = sPos;
            lastHeading = sFwd;

            float stepDist = totalLength / sampleCount;

            for (int i = 1; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                splineContainer.Evaluate(t, out float3 currentPos, out float3 currentFwd, out float3 currentUp);

                Vector3 pos = currentPos;
                Vector3 fwd = math.normalize(currentFwd);
                float currentDist = t * totalLength;

                float angleDelta = Vector3.Angle(lastHeading, fwd);
                accumulatedHeadingDelta += angleDelta;
                lastHeading = fwd;

                float distFromLast = Vector3.Distance(lastCheckpointPos, pos);

                if (accumulatedHeadingDelta >= _headingAngleThresholdDegrees && distFromLast >= _minDistanceBetweenCheckpoints)
                {
                    Quaternion rot = Quaternion.LookRotation(fwd, currentUp);
                    AddCheckpointNode(checkpointIndex++, pos, rot, currentDist, roadWidth);

                    lastCheckpointPos = pos;
                    accumulatedHeadingDelta = 0f;
                }
            }

            Debug.Log($"[CheckpointGenerator] Generated {_checkpoints.Count} checkpoints along track length ({totalLength:F1}m).");
            return _checkpoints;
        }

        private void AddCheckpointNode(int index, Vector3 pos, Quaternion rot, float dist, float roadWidth)
        {
            CheckpointNode node = new CheckpointNode
            {
                Index = index,
                Position = pos,
                Rotation = rot,
                DistanceAlongSpline = dist
            };
            _checkpoints.Add(node);

            // Instantiate visual gantry or trigger volume
            if (_checkpointVisualPrefab != null)
            {
                GameObject visual = Instantiate(_checkpointVisualPrefab, pos, rot, transform);
                visual.name = $"Checkpoint_{index:D2}";
                _instantiatedVisuals.Add(visual);
            }
            else
            {
                // Instantiate default trigger collider volume
                GameObject triggerObj = new GameObject($"Checkpoint_Trigger_{index:D2}");
                triggerObj.transform.SetParent(transform);
                triggerObj.transform.SetPositionAndRotation(pos, rot);

                BoxCollider box = triggerObj.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(roadWidth + 4.0f, 6.0f, 2.0f);
                box.center = new Vector3(0, 3.0f, 0);

                _instantiatedVisuals.Add(triggerObj);
            }
        }

        public void ClearCheckpoints()
        {
            foreach (var vis in _instantiatedVisuals)
            {
                if (vis != null) DestroyImmediate(vis);
            }
            _instantiatedVisuals.Clear();
            _checkpoints.Clear();
        }
    }
}
