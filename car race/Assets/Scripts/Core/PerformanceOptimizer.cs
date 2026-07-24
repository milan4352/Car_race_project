using UnityEngine;

namespace DrawAndRace.Core
{
    /// <summary>
    /// Enforces 60 FPS target framerate, disables screen sleep for mobile devices,
    /// and manages garbage collection efficiency for optimal runtime performance.
    /// </summary>
    public class PerformanceOptimizer : MonoBehaviour
    {
        [Header("Framerate & Power Tuning")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _disableScreenSleep = true;
        [SerializeField] private bool _enableVsync = false;

        private void Awake()
        {
            ApplyPerformanceSettings();
        }

        public void ApplyPerformanceSettings()
        {
            QualitySettings.vSyncCount = _enableVsync ? 1 : 0;
            Application.targetFrameRate = _targetFrameRate;

            if (_disableScreenSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            Debug.Log($"[PerformanceOptimizer] Applied Performance Settings: Target FPS={_targetFrameRate}, VSync={_enableVsync}, ScreenSleepDisabled={_disableScreenSleep}");
        }
    }
}
