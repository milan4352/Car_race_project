using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace DrawAndRace.TrackEditor
{
    /// <summary>
    /// Procedurally extrudes a 3D road mesh with curbs, shoulders, seamless PBR UV tiling,
    /// and a 3D MeshCollider along a SplineContainer.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class RoadMeshExtruder : MonoBehaviour
    {
        [Header("Road Configuration")]
        [SerializeField] private float _roadWidth = 8.0f;
        [SerializeField] private float _curbWidth = 0.5f;
        [SerializeField] private float _curbHeight = 0.15f;
        [SerializeField] private float _shoulderWidth = 1.0f;
        [SerializeField] private float _uvTileLength = 4.0f; // UV repeats every 4 meters
        [SerializeField] private int _segmentsPerMeter = 2;

        [Header("Materials")]
        [SerializeField] private Material _roadMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Mesh _generatedMesh;

        public float RoadWidth
        {
            get => _roadWidth;
            set { _roadWidth = Mathf.Clamp(value, 4.0f, 20.0f); }
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        /// <summary>
        /// Extrudes 3D mesh geometry along the given SplineContainer.
        /// </summary>
        public Mesh ExtrudeRoad(SplineContainer splineContainer)
        {
            if (splineContainer == null || splineContainer.Spline == null || splineContainer.Spline.Count < 2)
            {
                Debug.LogWarning("[RoadMeshExtruder] Cannot extrude road: Invalid SplineContainer.");
                return null;
            }

            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshCollider == null) _meshCollider = GetComponent<MeshCollider>();

            Spline spline = splineContainer.Spline;
            float totalLength = splineContainer.CalculateLength();
            int sampleCount = Mathf.Max(20, Mathf.RoundToInt(totalLength * _segmentsPerMeter));

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // Define cross-section offsets relative to center (X offset, Y height, U coordinate)
            float halfRoad = _roadWidth * 0.5f;
            float halfWithCurb = halfRoad + _curbWidth;
            float halfTotal = halfWithCurb + _shoulderWidth;

            // 6 Cross-section vertices per segment:
            // 0: Left Shoulder Outer (-halfTotal, 0)
            // 1: Left Curb Top (-halfRoad, _curbHeight)
            // 2: Left Road Edge (-halfRoad, 0)
            // 3: Right Road Edge (+halfRoad, 0)
            // 4: Right Curb Top (+halfRoad, _curbHeight)
            // 5: Right Shoulder Outer (+halfTotal, 0)

            Vector3[] profileOffsets = new Vector3[]
            {
                new Vector3(-halfTotal, 0, 0),
                new Vector3(-halfRoad, _curbHeight, 0),
                new Vector3(-halfRoad, 0, 0),
                new Vector3(halfRoad, 0, 0),
                new Vector3(halfRoad, _curbHeight, 0),
                new Vector3(halfTotal, 0, 0)
            };

            float[] profileU = new float[] { 0.0f, 0.15f, 0.2f, 0.8f, 0.85f, 1.0f };

            float cumulativeDistance = 0f;
            Vector3 previousPosition = Vector3.zero;

            for (int i = 0; i <= sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                if (spline.Closed && i == sampleCount) t = 0f;

                splineContainer.Evaluate(t, out float3 position, out float3 forward, out float3 up);

                Vector3 pos = position;
                Vector3 fwd = math.normalize(forward);
                Vector3 upVec = math.normalize(up);
                Vector3 rightVec = Vector3.Cross(upVec, fwd).normalized;

                if (i > 0)
                {
                    cumulativeDistance += Vector3.Distance(previousPosition, pos);
                }
                previousPosition = pos;

                float vCoord = cumulativeDistance / _uvTileLength;

                // Add cross-section vertices
                for (int p = 0; p < profileOffsets.Length; p++)
                {
                    Vector3 offset = profileOffsets[p];
                    Vector3 worldVert = pos + (rightVec * offset.x) + (upVec * offset.y);

                    vertices.Add(transform.InverseTransformPoint(worldVert));
                    normals.Add(upVec);
                    uvs.Add(new Vector2(profileU[p], vCoord));
                }

                // Add triangles connecting to previous segment
                if (i > 0)
                {
                    int currBase = i * profileOffsets.Length;
                    int prevBase = (i - 1) * profileOffsets.Length;

                    for (int p = 0; p < profileOffsets.Length - 1; p++)
                    {
                        int p0 = prevBase + p;
                        int p1 = prevBase + p + 1;
                        int c0 = currBase + p;
                        int c1 = currBase + p + 1;

                        triangles.Add(p0);
                        triangles.Add(c0);
                        triangles.Add(p1);

                        triangles.Add(p1);
                        triangles.Add(c0);
                        triangles.Add(c1);
                    }
                }
            }

            if (_generatedMesh == null)
            {
                _generatedMesh = new Mesh { name = "ProceduralRoadMesh" };
            }
            else
            {
                _generatedMesh.Clear();
            }

            _generatedMesh.SetVertices(vertices);
            _generatedMesh.SetNormals(normals);
            _generatedMesh.SetUVs(0, uvs);
            _generatedMesh.SetTriangles(triangles, 0);

            _generatedMesh.RecalculateBounds();
            _generatedMesh.RecalculateTangents();

            _meshFilter.sharedMesh = _generatedMesh;
            _meshCollider.sharedMesh = _generatedMesh;

            if (_roadMaterial != null)
            {
                _meshRenderer.sharedMaterial = _roadMaterial;
            }

            Debug.Log($"[RoadMeshExtruder] Road mesh extruded: {vertices.Count} verts, {triangles.Count / 3} tris, Length={totalLength:F1}m");
            return _generatedMesh;
        }
    }
}
