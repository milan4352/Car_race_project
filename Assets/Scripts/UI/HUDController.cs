using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DrawAndRace.Vehicle;

namespace DrawAndRace.UI
{
    /// <summary>
    /// Controls the high-end in-game HUD overlay including speedometer gauge,
    /// gear indicator, lap timer, checkpoint progress, off-track penalty, and wrong-way warnings.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Vehicle References")]
        [SerializeField] private CarPhysicsController _carController;
        [SerializeField] private LapTracker _lapTracker;
        [SerializeField] private OffTrackPenaltyHandler _penaltyHandler;

        [Header("Speedometer & Physics UI")]
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _gearText;
        [SerializeField] private Image _speedGaugeFill;
        [SerializeField] private Image _rpmBarFill;
        [SerializeField] private float _maxDisplaySpeed = 220f; // Km/h

        [Header("Lap & Timing UI")]
        [SerializeField] private TextMeshProUGUI _lapText;
        [SerializeField] private TextMeshProUGUI _currentLapTimeText;
        [SerializeField] private TextMeshProUGUI _bestLapTimeText;
        [SerializeField] private TextMeshProUGUI _checkpointText;

        [Header("Warnings & Overlay Banners")]
        [SerializeField] private GameObject _offTrackWarningBanner;
        [SerializeField] private GameObject _wrongWayWarningBanner;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private void Start()
        {
            AutoBindComponents();
            HideAllWarnings();
        }

        private void Update()
        {
            if (_carController == null) AutoBindComponents();
            if (_carController == null) return;

            UpdateSpeedometer();
            UpdateLapAndTiming();
            UpdateWarningOverlays();
        }

        public void BindVehicle(CarPhysicsController car)
        {
            if (car == null) return;
            _carController = car;
            _lapTracker = car.GetComponent<LapTracker>();
            _penaltyHandler = car.GetComponent<OffTrackPenaltyHandler>();
        }

        private void AutoBindComponents()
        {
            if (_carController == null)
            {
                _carController = FindObjectOfType<CarPhysicsController>();
                if (_carController != null)
                {
                    _lapTracker = _carController.GetComponent<LapTracker>();
                    _penaltyHandler = _carController.GetComponent<OffTrackPenaltyHandler>();
                }
            }
        }

        private void UpdateSpeedometer()
        {
            float speedKmh = _carController.CurrentSpeedKmh;
            if (_speedText != null)
            {
                _speedText.text = $"{Mathf.RoundToInt(speedKmh)}";
            }

            if (_gearText != null)
            {
                int gear = Mathf.Clamp(Mathf.FloorToInt(speedKmh / 35f) + 1, 1, 6);
                _gearText.text = $"GEAR {gear}";
            }

            float normalizedSpeed = Mathf.Clamp01(speedKmh / _maxDisplaySpeed);

            if (_speedGaugeFill != null)
            {
                _speedGaugeFill.fillAmount = normalizedSpeed;
                _speedGaugeFill.color = Color.Lerp(new Color(0.02f, 0.71f, 0.83f), new Color(0.93f, 0.27f, 0.27f), normalizedSpeed);
            }

            if (_rpmBarFill != null)
            {
                float rpmNormalized = (speedKmh % 35f) / 35f;
                _rpmBarFill.fillAmount = rpmNormalized;
            }
        }

        private void UpdateLapAndTiming()
        {
            if (_lapTracker == null) return;

            if (_lapText != null)
            {
                _lapText.text = $"LAP {_lapTracker.CurrentLap} / {_lapTracker.TotalLaps}";
            }

            if (_currentLapTimeText != null)
            {
                _currentLapTimeText.text = FormatTime(_lapTracker.CurrentLapTime);
            }

            if (_bestLapTimeText != null)
            {
                _bestLapTimeText.text = _lapTracker.BestLapTime < float.MaxValue ? $"BEST: {FormatTime(_lapTracker.BestLapTime)}" : "BEST: --:--.--";
            }

            if (_checkpointText != null)
            {
                _checkpointText.text = $"CHECKPOINT {_lapTracker.CurrentCheckpointIndex} / {_lapTracker.TotalCheckpoints}";
            }
        }

        private void UpdateWarningOverlays()
        {
            if (_penaltyHandler != null && _offTrackWarningBanner != null)
            {
                _offTrackWarningBanner.SetActive(_penaltyHandler.IsOffTrack);
            }

            if (_lapTracker != null && _wrongWayWarningBanner != null)
            {
                _wrongWayWarningBanner.SetActive(_lapTracker.IsWrongWay);
            }
        }

        public void DisplayCountdown(int seconds)
        {
            if (_countdownText == null) return;
            _countdownText.gameObject.SetActive(true);
            _countdownText.text = seconds > 0 ? $"{seconds}" : "GO!";
            if (seconds <= 0)
            {
                Invoke(nameof(HideCountdown), 1.0f);
            }
        }

        private void HideCountdown()
        {
            if (_countdownText != null) _countdownText.gameObject.SetActive(false);
        }

        private void HideAllWarnings()
        {
            if (_offTrackWarningBanner != null) _offTrackWarningBanner.SetActive(false);
            if (_wrongWayWarningBanner != null) _wrongWayWarningBanner.SetActive(false);
            if (_countdownText != null) _countdownText.gameObject.SetActive(false);
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)(timeInSeconds / 60);
            int seconds = (int)(timeInSeconds % 60);
            int fraction = (int)((timeInSeconds * 100) % 100);
            return $"{minutes:00}:{seconds:00}.{fraction:00}";
        }
    }
}
