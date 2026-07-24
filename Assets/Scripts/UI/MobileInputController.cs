using UnityEngine;
using UnityEngine.UI;
using DrawAndRace.Vehicle;

namespace DrawAndRace.UI
{
    /// <summary>
    /// Touchscreen Mobile Input Controller supporting left/right touch steering D-Pad,
    /// Gas pedal, Brake pedal, and Handbrake buttons.
    /// Auto-detects mobile platform or touchscreen input.
    /// </summary>
    public class MobileInputController : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private CarPhysicsController _carController;

        [Header("Mobile UI Panel")]
        [SerializeField] private GameObject _mobileControlsPanel;

        [Header("Touch Input States")]
        public float SteeringInput { get; private set; }
        public float ThrottleInput { get; private set; }
        public bool IsHandbrakePressed { get; private set; }

        private void Start()
        {
            AutoDetectPlatform();
        }

        private void AutoDetectPlatform()
        {
            bool isMobilePlatform = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
            if (_mobileControlsPanel != null)
            {
                _mobileControlsPanel.SetActive(isMobilePlatform);
            }
        }

        public void SetSteerLeft(bool pressed)
        {
            SteeringInput = pressed ? -1.0f : 0.0f;
        }

        public void SetSteerRight(bool pressed)
        {
            SteeringInput = pressed ? 1.0f : 0.0f;
        }

        public void SetAccelerate(bool pressed)
        {
            ThrottleInput = pressed ? 1.0f : 0.0f;
        }

        public void SetBrake(bool pressed)
        {
            ThrottleInput = pressed ? -1.0f : 0.0f;
        }

        public void SetHandbrake(bool pressed)
        {
            IsHandbrakePressed = pressed;
        }
    }
}
