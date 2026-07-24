using UnityEngine;

namespace DrawAndRace.Vehicle
{
    /// <summary>
    /// Smooth elevated 3D chase camera following vehicle position & heading
    /// with dynamic FOV velocity kick for high-speed feel.
    /// </summary>
    public class VehicleCameraFollow : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform _targetVehicle;
        [SerializeField] private Vector3 _offset = new Vector3(0, 8f, -12f);
        [SerializeField] private float _positionSmoothTime = 0.15f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        [Header("Dynamic FOV Kick")]
        [SerializeField] private float _baseFOV = 60f;
        [SerializeField] private float _maxSpeedFOV = 75f;
        [SerializeField] private float _maxSpeedForFOV = 160f; // km/h

        private Camera _camera;
        private CarPhysicsController _targetCarPhysics;
        private Vector3 _currentVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = Camera.main;
        }

        public void SetTarget(Transform target)
        {
            _targetVehicle = target;
            if (_targetVehicle != null)
            {
                _targetCarPhysics = _targetVehicle.GetComponent<CarPhysicsController>();
            }
        }

        private void LateUpdate()
        {
            if (_targetVehicle == null) return;

            // Follow Position
            Vector3 targetPosition = _targetVehicle.TransformPoint(_offset);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _positionSmoothTime);

            // Follow Rotation
            Quaternion targetRotation = Quaternion.LookRotation(_targetVehicle.position + Vector3.up * 1.5f - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / _rotationSmoothTime);

            // Dynamic FOV Speed Kick
            if (_camera != null && _targetCarPhysics != null)
            {
                float speedKmh = _targetCarPhysics.CurrentSpeedKmh;
                float targetFOV = Mathf.Lerp(_baseFOV, _maxSpeedFOV, speedKmh / _maxSpeedForFOV);
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, Time.deltaTime * 3.0f);
            }
        }
    }
}
