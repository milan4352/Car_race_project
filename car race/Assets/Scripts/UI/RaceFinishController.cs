using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DrawAndRace.Vehicle;

namespace DrawAndRace.UI
{
    /// <summary>
    /// Displays the race finish victory screen, lap breakdown, best lap record badge,
    /// and replay/restart/main menu navigation buttons.
    /// </summary>
    public class RaceFinishController : MonoBehaviour
    {
        [Header("Victory Screen References")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private TextMeshProUGUI _totalRaceTimeText;
        [SerializeField] private TextMeshProUGUI _bestLapTimeText;
        [SerializeField] private GameObject _newRecordBadge;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _reDrawTrackButton;
        [SerializeField] private Button _mainMenuButton;

        private void Start()
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_newRecordBadge != null) _newRecordBadge.SetActive(false);

            if (_restartButton != null) _restartButton.onClick.AddListener(RestartRace);
            if (_reDrawTrackButton != null) _reDrawTrackButton.onClick.AddListener(ReDrawTrack);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void ShowRaceFinish(float totalTime, float bestLapTime, bool isNewRecord)
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(true);

            if (_totalRaceTimeText != null) _totalRaceTimeText.text = $"TOTAL TIME: {FormatTime(totalTime)}";
            if (_bestLapTimeText != null) _bestLapTimeText.text = $"BEST LAP: {FormatTime(bestLapTime)}";

            if (_newRecordBadge != null) _newRecordBadge.SetActive(isNewRecord);

            Time.timeScale = 0f; // Pause physics during victory screen
        }

        public void RestartRace()
        {
            Time.timeScale = 1f;
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            LapTracker lapTracker = FindObjectOfType<LapTracker>();
            if (lapTracker != null)
            {
                lapTracker.ResetTracker();
            }
        }

        public void ReDrawTrack()
        {
            Time.timeScale = 1f;
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null)
            {
                mainMenu.ShowModeSelection();
            }
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null)
            {
                mainMenu.ShowModeSelection();
            }
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
