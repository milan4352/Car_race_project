using UnityEngine;

namespace DrawAndRace.Vehicle
{
    /// <summary>
    /// 4-wheel vehicle physics controller supporting motor torque, front-wheel steering,
    /// dynamic weight transfer, brake/handbrake drift friction curves, and off-track modifiers.
    /// Dual input system support (Legacy UnityEngine.Input & New UnityEngine.InputSystem).
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
        public bool IsHandbraking => _isHandbraking;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.centerOfMass += _centerOfMassOffset;
            }
            AutoBindWheelReferences();
        }

        private void AutoBindWheelReferences()
        {
            if (_frontLeftWheel == null) _frontLeftWheel = transform.Find("Wheels/Wheel_FL")?.GetComponent<WheelCollider>();
            if (_frontRightWheel == null) _frontRightWheel = transform.Find("Wheels/Wheel_FR")?.GetComponent<WheelCollider>();
            if (_rearLeftWheel == null) _rearLeftWheel = transform.Find("Wheels/Wheel_RL")?.GetComponent<WheelCollider>();
            if (_rearRightWheel == null) _rearRightWheel = transform.Find("Wheels/Wheel_RR")?.GetComponent<WheelCollider>();

            if (_frontLeftTransform == null) _frontLeftTransform = transform.Find("Wheels/Wheel_FL/Wheel_FL_Visual");
            if (_frontRightTransform == null) _frontRightTransform = transform.Find("Wheels/Wheel_FR/Wheel_FR_Visual");
            if (_rearLeftTransform == null) _rearLeftTransform = transform.Find("Wheels/Wheel_RL/Wheel_RL_Visual");
            if (_rearRightTransform == null) _rearRightTransform = transform.Find("Wheels/Wheel_RR/Wheel_RR_Visual");
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
            float vertical = 0f;
            float horizontal = 0f;
            bool isBrakingKey = false;
            bool isHandbrakingKey = false;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1.0f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    vertical -= 1.0f;
                    isBrakingKey = true;
                }
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1.0f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1.0f;
                if (keyboard.spaceKey.isPressed) isHandbrakingKey = true;
            }

            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stick.y) > 0.1f) vertical = stick.y;
                if (Mathf.Abs(stick.x) > 0.1f) horizontal = stick.x;
                if (gamepad.buttonSouth.isPressed || gamepad.rightTrigger.isPressed) vertical = 1.0f;
                if (gamepad.buttonWest.isPressed || gamepad.leftTrigger.isPressed) isBrakingKey = true;
                if (gamepad.buttonEast.isPressed) isHandbrakingKey = true;
            }
#else
            try
            {
                vertical = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");
                isBrakingKey = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                isHandbrakingKey = Input.GetKey(KeyCode.Space);
            }
            catch (System.Exception)
            {
                // Fallback in case of input system mismatch
            }
#endif

            _currentMotorInput = vertical;
            _currentSteerInput = horizontal;
            _isBraking = isBrakingKey;
            _isHandbraking = isHandbrakingKey;
        }

        private void ApplyMotorAndBrakes()
        {
            float speedKmh = CurrentSpeedKmh;
            float activeTorque = _motorTorque * _speedMultiplier;

            if (speedKmh > _topSpeedKmh * _speedMultiplier)
            {
                activeTorque = 0f;
            }

            float motorValue = _currentMotorInput * activeTorque;
            if (_rearLeftWheel != null) _rearLeftWheel.motorTorque = motorValue;
            if (_rearRightWheel != null) _rearRightWheel.motorTorque = motorValue;

            float brakeValue = _isBraking ? _brakeTorque : (_isHandbraking ? _brakeTorque * 0.5f : 0f);
            if (_frontLeftWheel != null) _frontLeftWheel.brakeTorque = brakeValue;
            if (_frontRightWheel != null) _frontRightWheel.brakeTorque = brakeValue;
            if (_rearLeftWheel != null) _rearLeftWheel.brakeTorque = brakeValue;
            if (_rearRightWheel != null) _rearRightWheel.brakeTorque = brakeValue;
        }

        private void ApplySteering()
        {
            float targetAngle = _currentSteerInput * _maxSteerAngle;
            if (_frontLeftWheel != null) _frontLeftWheel.steerAngle = targetAngle;
            if (_frontRightWheel != null) _frontRightWheel.steerAngle = targetAngle;
        }

        private void ApplyDriftFriction()
        {
            float stiffness = _isHandbraking ? _driftSidewaysStiffness : _normalSidewaysStiffness;
            if (_rearLeftWheel != null) SetWheelSidewaysStiffness(_rearLeftWheel, stiffness);
            if (_rearRightWheel != null) SetWheelSidewaysStiffness(_rearRightWheel, stiffness);
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
            if (_frontLeftWheel != null && _frontLeftTransform != null) UpdateSingleWheel(_frontLeftWheel, _frontLeftTransform);
            if (_frontRightWheel != null && _frontRightTransform != null) UpdateSingleWheel(_frontRightWheel, _frontRightTransform);
            if (_rearLeftWheel != null && _rearLeftTransform != null) UpdateSingleWheel(_rearLeftWheel, _rearLeftTransform);
            if (_rearRightWheel != null && _rearRightTransform != null) UpdateSingleWheel(_rearRightWheel, _rearRightTransform);
        }

        private void UpdateSingleWheel(WheelCollider collider, Transform transform)
        {
            if (collider == null || transform == null) return;
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
