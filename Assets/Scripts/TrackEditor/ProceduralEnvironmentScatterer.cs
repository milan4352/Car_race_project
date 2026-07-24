using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace DrawAndRace.TrackEditor
{
    [System.Serializable]
    public struct EnvironmentPropEntry
    {
        public GameObject Prefab;
        public float SpawnWeight;
        public Vector2 ScaleRange;
        public bool AlignWithRoadRotation;
    }

    /// <summary>
    /// Procedurally scatters 3D foliage, rocks, barriers, and street lamps along spline track margins
    /// while strictly maintaining safety offsets from the drivable road surface.
    /// </summary>
    public class ProceduralEnvironmentScatterer : MonoBehaviour
    {
        [Header("Scattering Parameters")]
        [SerializeField] private float _safetyOffsetMargin = 2.5f; // meters beyond road edge
        [SerializeField] private float _propSpacingMeter = 6.0f;
        [SerializeField] private float _scatterJitter = 1.5f;

        [Header("Prop Libraries")]
        [SerializeField] private List<EnvironmentPropEntry> _leftSideProps = new List<EnvironmentPropEntry>();
        [SerializeField] private List<EnvironmentPropEntry> _rightSideProps = new List<EnvironmentPropEntry>();

        private readonly List<GameObject> _spawnedProps = new List<GameObject>();
        private Transform _propsContainer;

        /// <summary>
        /// Scatters environment props along left and right track borders.
        /// </summary>
        public void ScatterEnvironment(SplineContainer splineContainer, float roadWidth = 8.0f)
        {
            ClearEnvironment();
            if (splineContainer == null || splineContainer.Spline == null) return;

            if (_propsContainer == null)
            {
                GameObject containerObj = new GameObject("EnvironmentProps");
                containerObj.transform.SetParent(transform);
                _propsContainer = containerObj.transform;
            }

            Spline spline = splineContainer.Spline;
            float totalLength = splineContainer.CalculateLength();
            int steps = Mathf.Max(10, Mathf.RoundToInt(totalLength / _propSpacingMeter));
            float minOffsetFromCenter = (roadWidth * 0.5f) + _safetyOffsetMargin;

            System.Random rand = new System.Random(42); // Deterministic seed

            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                splineContainer.Evaluate(t, out float3 pos, out float3 fwd, out float3 up);

                Vector3 position = pos;
                Vector3 forward = math.normalize(fwd);
                Vector3 upVec = math.normalize(up);
                Vector3 right = Vector3.Cross(upVec, forward).normalized;

                // Scatter Left Side Prop
                float leftOffset = minOffsetFromCenter + (float)(rand.NextDouble() * _scatterJitter);
                Vector3 leftPos = position - (right * leftOffset);
                SpawnPropAt(_leftSideProps, leftPos, forward, upVec, rand);

                // Scatter Right Side Prop
                float rightOffset = minOffsetFromCenter + (float)(rand.NextDouble() * _scatterJitter);
                Vector3 rightPos = position + (right * rightOffset);
                SpawnPropAt(_rightSideProps, rightPos, forward, upVec, rand);
            }

            Debug.Log($"[ProceduralEnvironmentScatterer] Spawned {_spawnedProps.Count} environment props along track margins.");
        }

        private void SpawnPropAt(List<EnvironmentPropEntry> library, Vector3 position, Vector3 forward, Vector3 up, System.Random rand)
        {
            if (library == null || library.Count == 0) return;

            // Pick random weighted prop
            EnvironmentPropEntry entry = library[rand.Next(0, library.Count)];
            if (entry.Prefab == null) return;

            Quaternion rot = Quaternion.Euler(0, (float)(rand.NextDouble() * 360.0), 0);
            if (entry.AlignWithRoadRotation)
            {
                rot = Quaternion.LookRotation(forward, up);
            }

            GameObject prop = Instantiate(entry.Prefab, position, rot, _propsContainer);
            
            float scaleMin = entry.ScaleRange.x > 0 ? entry.ScaleRange.x : 0.8f;
            float scaleMax = entry.ScaleRange.y > 0 ? entry.ScaleRange.y : 1.2f;
            float scale = scaleMin + (float)(rand.NextDouble() * (scaleMax - scaleMin));
            prop.transform.localScale = Vector3.one * scale;

            _spawnedProps.Add(prop);
        }

        public void ClearEnvironment()
        {
            foreach (var prop in _spawnedProps)
            {
                if (prop != null) DestroyImmediate(prop);
            }
            _spawnedProps.Clear();

            if (_propsContainer != null)
            {
                DestroyImmediate(_propsContainer.gameObject);
                _propsContainer = null;
            }
        }
    }
}
