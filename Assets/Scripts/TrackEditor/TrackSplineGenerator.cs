using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace DrawAndRace.TrackEditor
{
    /// <summary>
    /// Converts 2D viewport drawing points into a 3D Unity SplineContainer
    /// on the ground plane (Y=0) with closed Catmull-Rom smooth tangents.
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public class TrackSplineGenerator : MonoBehaviour
    {
        [Header("Spline Options")]
        [SerializeField] private float _groundY = 0f;
        [SerializeField] private TangentMode _tangentMode = TangentMode.AutoSmooth;

        private SplineContainer _splineContainer;

        public SplineContainer SplineContainer => _splineContainer;
        public Spline Spline => _splineContainer != null ? _splineContainer.Spline : null;

        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        /// <summary>
        /// Converts 2D screen points to 3D world space points via camera plane raycasting.
        /// </summary>
        public List<Vector3> ConvertScreenPointsToWorld(IReadOnlyList<Vector2> screenPoints, Camera targetCamera)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            List<Vector3> worldPoints = new List<Vector3>(screenPoints.Count);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, _groundY, 0));

            foreach (Vector2 screenPos in screenPoints)
            {
                Ray ray = targetCamera.ScreenPointToRay(screenPos);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    worldPoints.Add(ray.GetPoint(enter));
                }
            }

            return worldPoints;
        }

        /// <summary>
        /// Populates the SplineContainer knots from a sequence of 3D world points.
        /// </summary>
        public Spline GenerateSplineFromWorldPoints(List<Vector3> worldPoints, bool isClosedLoop = true)
        {
            if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
            if (_splineContainer == null) _splineContainer = gameObject.AddComponent<SplineContainer>();

            Spline spline = _splineContainer.Spline;
            spline.Clear();

            foreach (Vector3 point in worldPoints)
            {
                BezierKnot knot = new BezierKnot(new float3(point.x, point.y, point.z));
                spline.Add(knot, _tangentMode);
            }

            spline.Closed = isClosedLoop;

            Debug.Log($"[DrawAndRace] Spline generated with {spline.Count} knots. ClosedLoop={isClosedLoop}");
            return spline;
        }
    }
}
