using System;
using System.Collections.Generic;
using UnityEngine;
using DrawAndRace.TrackEditor;

namespace DrawAndRace.Vehicle
{
    /// <summary>
    /// Validates checkpoint trigger order sequentially, calculates lap split times,
    /// tracks current/best lap records, and detects wrong-way driving.
    /// </summary>
    public class LapTracker : MonoBehaviour
    {
        [Header("Lap Configuration")]
        [SerializeField] private int _totalLapsToComplete = 3;

        private int _nextRequiredCheckpointIndex = 0;
        private int _totalCheckpoints = 0;
        private int _currentLap = 1;
        private float _currentLapTime = 0f;
        private float _bestLapTime = float.MaxValue;
        private bool _isRaceActive = false;
        private bool _isWrongWay = false;

        public event Action<int, float> OnCheckpointPassed; // checkpointIndex, splitTime
        public event Action<int, float> OnLapCompleted;     // lapNumber, lapTime
        public event Action<float> OnRaceFinished;         // totalRaceTime
        public event Action<bool> OnWrongWayStatusChanged; // isWrongWay

        public int CurrentLap => _currentLap;
        public int TotalLaps => _totalLapsToComplete;
        public int TotalCheckpoints => _totalCheckpoints;
        public int CurrentCheckpointIndex => _nextRequiredCheckpointIndex;
        public float CurrentLapTime => _currentLapTime;
        public float BestLapTime => _bestLapTime;
        public bool IsRaceActive => _isRaceActive;
        public bool IsWrongWay => _isWrongWay;

        private void Update()
        {
            if (_isRaceActive)
            {
                _currentLapTime += Time.deltaTime;
            }
        }

        public void InitializeRace(int totalCheckpoints)
        {
            _totalCheckpoints = totalCheckpoints;
            _nextRequiredCheckpointIndex = 0;
            _currentLap = 1;
            _currentLapTime = 0f;
            _isRaceActive = true;
            _isWrongWay = false;
            Debug.Log($"[LapTracker] Race initialized with {_totalCheckpoints} checkpoints across {_totalLapsToComplete} laps.");
        }

        public void ResetTracker()
        {
            _nextRequiredCheckpointIndex = 0;
            _currentLap = 1;
            _currentLapTime = 0f;
            _isRaceActive = true;
            _isWrongWay = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isRaceActive || _totalCheckpoints == 0) return;

            string objName = other.gameObject.name;
            if (!objName.StartsWith("Checkpoint_")) return;

            // Parse Checkpoint Index from name "Checkpoint_Trigger_02" or "Checkpoint_02"
            string[] parts = objName.Split('_');
            if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int hitIndex))
            {
                HandleCheckpointHit(hitIndex);
            }
        }

        private void HandleCheckpointHit(int hitIndex)
        {
            if (hitIndex == _nextRequiredCheckpointIndex)
            {
                OnCheckpointPassed?.Invoke(hitIndex, _currentLapTime);
                _nextRequiredCheckpointIndex = (_nextRequiredCheckpointIndex + 1) % _totalCheckpoints;

                if (_isWrongWay)
                {
                    _isWrongWay = false;
                    OnWrongWayStatusChanged?.Invoke(false);
                }

                // Check for Lap Completion (hit Start/Finish Checkpoint 0 after completing all other checkpoints)
                if (hitIndex == 0 && _nextRequiredCheckpointIndex == 1)
                {
                    CompleteLap();
                }
            }
            else if (hitIndex != (_nextRequiredCheckpointIndex - 1 + _totalCheckpoints) % _totalCheckpoints)
            {
                // Wrong direction or skipped checkpoint
                _isWrongWay = true;
                OnWrongWayStatusChanged?.Invoke(true);
                Debug.LogWarning($"[LapTracker] Wrong checkpoint hit! Required={_nextRequiredCheckpointIndex}, Hit={hitIndex}");
            }
        }

        private void CompleteLap()
        {
            float lapTime = _currentLapTime;
            if (lapTime < _bestLapTime)
            {
                _bestLapTime = lapTime;
            }

            OnLapCompleted?.Invoke(_currentLap, lapTime);
            Debug.Log($"[LapTracker] Lap {_currentLap} completed in {lapTime:F2}s! Best: {_bestLapTime:F2}s");

            if (_currentLap >= _totalLapsToComplete)
            {
                _isRaceActive = false;
                OnRaceFinished?.Invoke(lapTime);
                Debug.Log("[LapTracker] Race Finished!");
            }
            else
            {
                _currentLap++;
                _currentLapTime = 0f;
            }
        }
    }
}
