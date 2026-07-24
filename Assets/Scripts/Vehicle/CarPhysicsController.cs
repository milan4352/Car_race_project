using UnityEngine;

namespace DrawAndRace.Vehicle
{
    /// <summary>
    /// 4-wheel vehicle physics controller supporting motor torque, front-wheel steering,
    /// dynamic weight transfer, brake/handbrake drift friction curves, and off-track modifiers.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarPhysicsController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider _frontLeftWheel;
        [SerializeField] private WheelCollider _frontRightWheel;
        [SerializeField] private WheelCollider _rearLeftWheel;
        [SerializeField] private WheelCollider _rearRightWheel;

        [Header("Wheel Visual Transforms")]
        [SerializeField] private Transform _frontLeftTransform;
        [SerializeField] private Transform _frontRightTransform;
        [SerializeField] private Transform _rearLeftTransform;
        [SerializeField] private Transform _rearRightTransform;

        [Header("Engine & Handling Stats")]
        [SerializeField] private float _motorTorque = 1500f;
        [SerializeField] private float _brakeTorque = 3000f;
        [SerializeField] private float _maxSteerAngle = 35f;
        [SerializeField] private float _topSpeedKmh = 180f;
        [SerializeField] private Vector3 _centerOfMassOffset = new Vector3(0, -0.5f, 0.2f);

        [Header("Drift & Friction Parameters")]
        [SerializeField] private float _normalSidewaysStiffness = 1.8f;
        [SerializeField] private float _driftSidewaysStiffness = 0.6f;

        private Rigidbody _rigidbody;
        private float _currentMotorInput;
        private float _currentSteerInput;
        private bool _isBraking;
        private bool _isHandbraking;

        private float _speedMultiplier = 1.0f; // Modified by OffTrackPenaltyHandler

        public float CurrentSpeedKmh => _rigidbody != null ? _rigidbody.linearVelocity.magnitude * 3.6f : 0f;
        public float SpeedMultiplier { get => _speedMultiplier; set => _speedMultiplier = Mathf.Clamp01(value); }
        public bool IsDrifting => _isHandbraking;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.centerOfMass += _centerOfMassOffset;
        }

        private void Update()
        {
            ReadInput();
            UpdateWheelVisuals();
        }

        private void FixedUpdate()
        {
            ApplyMotorAndBrakes();
            ApplySteering();
            ApplyDriftFriction();
        }

        private void ReadInput()
        {
            _currentMotorInput = Input.GetAxis("Vertical");
            _currentSteerInput = Input.GetAxis("Horizontal");
            _isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            _isHandbraking = Input.GetKey(KeyCode.Space);
        }

        private void ApplyMotorAndBrakes()
        {
            float speedKmh = CurrentSpeedKmh;
            float activeTorque = _motorTorque * _speedMultiplier;

            // Speed Governor
            if (speedKmh > _topSpeedKmh * _speedMultiplier)
            {
                activeTorque = 0f;
            }

            // Apply Motor Torque to Rear Wheels (RWD)
            float motorValue = _currentMotorInput * activeTorque;
            _rearLeftWheel.motorTorque = motorValue;
            _rearRightWheel.motorTorque = motorValue;

            // Apply Brakes
            float brakeValue = _isBraking ? _brakeTorque : (_isHandbraking ? _brakeTorque * 0.5f : 0f);
            _frontLeftWheel.brakeTorque = brakeValue;
            _frontRightWheel.brakeTorque = brakeValue;
            _rearLeftWheel.brakeTorque = brakeValue;
            _rearRightWheel.brakeTorque = brakeValue;
        }

        private void ApplySteering()
        {
            float targetAngle = _currentSteerInput * _maxSteerAngle;
            _frontLeftWheel.steerAngle = targetAngle;
            _frontRightWheel.steerAngle = targetAngle;
        }

        private void ApplyDriftFriction()
        {
            float stiffness = _isHandbraking ? _driftSidewaysStiffness : _normalSidewaysStiffness;
            SetWheelSidewaysStiffness(_rearLeftWheel, stiffness);
            SetWheelSidewaysStiffness(_rearRightWheel, stiffness);
        }

        private void SetWheelSidewaysStiffness(WheelCollider wheel, float stiffness)
        {
            if (wheel == null) return;
            WheelFrictionCurve friction = wheel.sidewaysFriction;
            friction.stiffness = stiffness;
            wheel.sidewaysFriction = friction;
        }

        private void UpdateWheelVisuals()
        {
            UpdateSingleWheel(_frontLeftWheel, _frontLeftTransform);
            UpdateSingleWheel(_frontRightWheel, _frontRightTransform);
            UpdateSingleWheel(_rearLeftWheel, _rearLeftTransform);
            UpdateSingleWheel(_rearRightWheel, _rearRightTransform);
        }

        private void UpdateSingleWheel(WheelCollider collider, Transform transform)
        {
            if (collider == null || transform == null) return;
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
