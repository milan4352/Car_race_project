using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DrawAndRace.UI
{
    /// <summary>
    /// Manages core game loop navigation, mode selection (Draw Custom Track vs Prebuilt Circuit),
    /// 3D car selection turntable preview, performance stats cards, and scene switching.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject _modeSelectionPanel;
        [SerializeField] private GameObject _carSelectionPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _victoryPanel;

        [Header("Car Selection Turntable")]
        [SerializeField] private Transform _turntablePivot;
        [SerializeField] private float _turntableRotationSpeed = 25f;
        [SerializeField] private GameObject[] _carPrefabs; // 0: Red Supercar, 1: Blue GT Racer, 2: Gold Hypercar

        [Header("Car Stats UI")]
        [SerializeField] private TextMeshProUGUI _carNameText;
        [SerializeField] private Image _topSpeedBar;
        [SerializeField] private Image _accelerationBar;
        [SerializeField] private Image _handlingBar;

        private int _selectedCarIndex = 0;
        private GameObject _activeTurntableCar;

        private void Start()
        {
            ShowModeSelection();
        }

        private void Update()
        {
            if (_turntablePivot != null && _carSelectionPanel.activeSelf)
            {
                _turntablePivot.Rotate(Vector3.up, _turntableRotationSpeed * Time.deltaTime, Space.World);
            }
        }

        public void ShowModeSelection()
        {
            SetPanelState(_modeSelectionPanel);
        }

        public void ShowCarSelection()
        {
            SetPanelState(_carSelectionPanel);
            UpdateTurntableCar();
        }

        public void SelectNextCar()
        {
            if (_carPrefabs == null || _carPrefabs.Length == 0) return;
            _selectedCarIndex = (_selectedCarIndex + 1) % _carPrefabs.Length;
            UpdateTurntableCar();
        }

        public void SelectPreviousCar()
        {
            if (_carPrefabs == null || _carPrefabs.Length == 0) return;
            _selectedCarIndex = (_selectedCarIndex - 1 + _carPrefabs.Length) % _carPrefabs.Length;
            UpdateTurntableCar();
        }

        public void StartDrawTrackMode()
        {
            SetPanelState(_hudPanel);
            PlayerPrefs.SetInt("SelectedCarIndex", _selectedCarIndex);
            Debug.Log("[MainMenuController] Starting Draw Track Mode...");
        }

        public void StartPrebuiltCircuitMode()
        {
            SetPanelState(_hudPanel);
            PlayerPrefs.SetInt("SelectedCarIndex", _selectedCarIndex);
            Debug.Log("[MainMenuController] Starting Prebuilt Circuit Mode...");
        }

        private void UpdateTurntableCar()
        {
            if (_turntablePivot == null || _carPrefabs == null || _carPrefabs.Length == 0) return;

            if (_activeTurntableCar != null)
            {
                Destroy(_activeTurntableCar);
            }

            GameObject prefab = _carPrefabs[_selectedCarIndex];
            if (prefab != null)
            {
                _activeTurntableCar = Instantiate(prefab, _turntablePivot);
                _activeTurntableCar.transform.localPosition = Vector3.zero;
                _activeTurntableCar.transform.localRotation = Quaternion.identity;

                // Disable physics during turntable preview
                Rigidbody rb = _activeTurntableCar.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }

            UpdateStatsUI();
        }

        private void UpdateStatsUI()
        {
            string[] carNames = { "RED SUPERCAR GT", "COBALT BLUE RACER", "LIQUID GOLD HYPERCAR" };
            float[] topSpeeds = { 0.85f, 0.88f, 0.96f };
            float[] accelerations = { 0.90f, 0.85f, 0.95f };
            float[] handlings = { 0.88f, 0.92f, 0.90f };

            if (_carNameText != null) _carNameText.text = carNames[_selectedCarIndex];
            if (_topSpeedBar != null) _topSpeedBar.fillAmount = topSpeeds[_selectedCarIndex];
            if (_accelerationBar != null) _accelerationBar.fillAmount = accelerations[_selectedCarIndex];
            if (_handlingBar != null) _handlingBar.fillAmount = handlings[_selectedCarIndex];
        }

        private void SetPanelState(GameObject activePanel)
        {
            if (_modeSelectionPanel != null) _modeSelectionPanel.SetActive(_modeSelectionPanel == activePanel);
            if (_carSelectionPanel != null) _carSelectionPanel.SetActive(_carSelectionPanel == activePanel);
            if (_hudPanel != null) _hudPanel.SetActive(_hudPanel == activePanel);
            if (_victoryPanel != null) _victoryPanel.SetActive(_victoryPanel == activePanel);
        }
    }
}
