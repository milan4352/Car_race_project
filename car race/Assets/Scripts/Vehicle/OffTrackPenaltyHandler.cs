using UnityEngine;

namespace DrawAndRace.Vehicle
{
    /// <summary>
    /// Raycasts downward to detect if vehicle wheels are on the 3D procedural road surface.
    /// Applies a soft speed & grip reduction penalty when driving off-track onto grass/dirt.
    /// </summary>
    [RequireComponent(typeof(CarPhysicsController))]
    public class OffTrackPenaltyHandler : MonoBehaviour
    {
        [Header("Penalty Settings")]
        [SerializeField] private float _offTrackSpeedMultiplier = 0.45f; // 55% speed reduction off-track
        [SerializeField] private float _recoverySpeed = 3.0f; // Smooth transition speed
        [SerializeField] private float _raycastDistance = 2.0f;
        [SerializeField] private LayerMask _roadLayerMask = ~0; // Default to all layers

        private CarPhysicsController _carController;
        private bool _isOffTrack;

        public bool IsOffTrack => _isOffTrack;

        private void Awake()
        {
            _carController = GetComponent<CarPhysicsController>();
        }

        private void Update()
        {
            CheckOffTrackStatus();
        }

        private void CheckOffTrackStatus()
        {
            // Raycast down from car center
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
            bool hitRoad = false;

            if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _roadLayerMask))
            {
                // Check if hit object is procedural road or has road collider name
                if (hit.collider.name.Contains("ProceduralRoad") || hit.collider.GetComponent<TrackEditor.RoadMeshExtruder>() != null)
                {
                    hitRoad = true;
                }
            }

            _isOffTrack = !hitRoad;

            float targetMultiplier = _isOffTrack ? _offTrackSpeedMultiplier : 1.0f;
            _carController.SpeedMultiplier = Mathf.MoveTowards(_carController.SpeedMultiplier, targetMultiplier, Time.deltaTime * _recoverySpeed);
        }
    }
}
