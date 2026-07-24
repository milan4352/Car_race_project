#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DrawAndRace.UI;
using DrawAndRace.Core;

namespace DrawAndRace.Editor
{
    public static class Phase4UISceneBuilder
    {
        [MenuItem("DrawAndRace/Setup Phase 4 Full Game Loop & UI")]
        public static void BuildPhase4UI()
        {
            // Find or Create UI Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("UI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            Transform canvasTransform = canvas.transform;

            // 1. Create Main HUD Panel
            GameObject hudPanel = CreatePanel(canvasTransform, "HUDPanel");
            HUDController hudController = hudPanel.AddComponent<HUDController>();

            // Speedometer Gauge Container (Bottom Right)
            GameObject speedGaugeObj = CreatePanel(hudPanel.transform, "SpeedometerGauge");
            RectTransform speedRect = speedGaugeObj.GetComponent<RectTransform>();
            speedRect.anchorMin = new Vector2(1, 0);
            speedRect.anchorMax = new Vector2(1, 0);
            speedRect.pivot = new Vector2(1, 0);
            speedRect.sizeDelta = new Vector2(240, 140);
            speedRect.anchoredPosition = new Vector2(-30, 30);

            // Speed Text
            GameObject speedTextObj = CreateText(speedGaugeObj.transform, "SpeedText", "0", 48, TextAlignmentOptions.Right);
            RectTransform speedTextRect = speedTextObj.GetComponent<RectTransform>();
            speedTextRect.anchoredPosition = new Vector2(-20, 25);
            TextMeshProUGUI speedTMP = speedTextObj.GetComponent<TextMeshProUGUI>();

            // KM/H Subtitle Text
            GameObject kmhTextObj = CreateText(speedGaugeObj.transform, "KMHLabel", "KM/H", 16, TextAlignmentOptions.Right);
            kmhTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -20);

            // Gear Text
            GameObject gearTextObj = CreateText(speedGaugeObj.transform, "GearText", "GEAR 1", 20, TextAlignmentOptions.Left);
            gearTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180, -20);
            TextMeshProUGUI gearTMP = gearTextObj.GetComponent<TextMeshProUGUI>();

            // Lap & Checkpoint UI Container (Top Left)
            GameObject lapPanelObj = CreatePanel(hudPanel.transform, "LapTimingPanel");
            RectTransform lapRect = lapPanelObj.GetComponent<RectTransform>();
            lapRect.anchorMin = new Vector2(0, 1);
            lapRect.anchorMax = new Vector2(0, 1);
            lapRect.pivot = new Vector2(0, 1);
            lapRect.sizeDelta = new Vector2(300, 120);
            lapRect.anchoredPosition = new Vector2(30, -30);

            GameObject lapTextObj = CreateText(lapPanelObj.transform, "LapText", "LAP 1 / 3", 26, TextAlignmentOptions.Left);
            lapTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -20);
            TextMeshProUGUI lapTMP = lapTextObj.GetComponent<TextMeshProUGUI>();

            GameObject lapTimeObj = CreateText(lapPanelObj.transform, "CurrentLapTimeText", "00:00.00", 22, TextAlignmentOptions.Left);
            lapTimeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -50);
            TextMeshProUGUI currentLapTimeTMP = lapTimeObj.GetComponent<TextMeshProUGUI>();

            GameObject bestLapTimeObj = CreateText(lapPanelObj.transform, "BestLapTimeText", "BEST: --:--.--", 18, TextAlignmentOptions.Left);
            bestLapTimeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -80);
            TextMeshProUGUI bestLapTimeTMP = bestLapTimeObj.GetComponent<TextMeshProUGUI>();

            // Checkpoint Text (Top Right)
            GameObject cpTextObj = CreateText(hudPanel.transform, "CheckpointText", "CHECKPOINT 0 / 8", 20, TextAlignmentOptions.Right);
            RectTransform cpRect = cpTextObj.GetComponent<RectTransform>();
            cpRect.anchorMin = new Vector2(1, 1);
            cpRect.anchorMax = new Vector2(1, 1);
            cpRect.pivot = new Vector2(1, 1);
            cpRect.anchoredPosition = new Vector2(-30, -30);
            TextMeshProUGUI cpTMP = cpTextObj.GetComponent<TextMeshProUGUI>();

            // Warning Banners (Center Overlay)
            GameObject offTrackBanner = CreateWarningBanner(hudPanel.transform, "OffTrackWarningBanner", "OFF-TRACK SPEED PENALTY -55%", new Color(0.93f, 0.27f, 0.27f));
            GameObject wrongWayBanner = CreateWarningBanner(hudPanel.transform, "WrongWayWarningBanner", "WRONG WAY! TURN AROUND", new Color(0.95f, 0.6f, 0.1f));

            // Assign HUDController Serialized Properties
            SerializedObject hudSerialized = new SerializedObject(hudController);
            hudSerialized.FindProperty("_speedText").objectReferenceValue = speedTMP;
            hudSerialized.FindProperty("_gearText").objectReferenceValue = gearTMP;
            hudSerialized.FindProperty("_lapText").objectReferenceValue = lapTMP;
            hudSerialized.FindProperty("_currentLapTimeText").objectReferenceValue = currentLapTimeTMP;
            hudSerialized.FindProperty("_bestLapTimeText").objectReferenceValue = bestLapTimeTMP;
            hudSerialized.FindProperty("_checkpointText").objectReferenceValue = cpTMP;
            hudSerialized.FindProperty("_offTrackWarningBanner").objectReferenceValue = offTrackBanner;
            hudSerialized.FindProperty("_wrongWayWarningBanner").objectReferenceValue = wrongWayBanner;
            hudSerialized.ApplyModifiedProperties();

            // 2. Create Victory Panel
            GameObject victoryPanel = CreatePanel(canvasTransform, "VictoryPanel");
            RaceFinishController victoryController = victoryPanel.AddComponent<RaceFinishController>();

            GameObject victoryTitle = CreateText(victoryPanel.transform, "VictoryTitle", "RACE FINISHED!", 42, TextAlignmentOptions.Center);
            victoryTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            GameObject totalTimeText = CreateText(victoryPanel.transform, "TotalRaceTimeText", "TOTAL TIME: 01:25.40", 24, TextAlignmentOptions.Center);
            totalTimeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

            GameObject bestLapText = CreateText(victoryPanel.transform, "BestLapText", "BEST LAP: 00:39.85", 22, TextAlignmentOptions.Center);
            bestLapText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

            // Replay Buttons
            Button restartBtn = CreateButton(victoryPanel.transform, "RestartButton", "RESTART RACE", new Vector2(0, -70));
            Button mainMnuBtn = CreateButton(victoryPanel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -130));

            SerializedObject victorySerialized = new SerializedObject(victoryController);
            victorySerialized.FindProperty("_victoryPanel").objectReferenceValue = victoryPanel;
            victorySerialized.FindProperty("_totalRaceTimeText").objectReferenceValue = totalTimeText.GetComponent<TextMeshProUGUI>();
            victorySerialized.FindProperty("_bestLapTimeText").objectReferenceValue = bestLapText.GetComponent<TextMeshProUGUI>();
            victorySerialized.FindProperty("_restartButton").objectReferenceValue = restartBtn;
            victorySerialized.FindProperty("_mainMenuButton").objectReferenceValue = mainMnuBtn;
            victorySerialized.ApplyModifiedProperties();

            victoryPanel.SetActive(false); // Hide victory modal on start

            // 3. Create MainMenuController
            GameObject menuObj = new GameObject("MainMenuController");
            MainMenuController mainMenu = menuObj.AddComponent<MainMenuController>();

            Debug.Log("[Phase4UISceneBuilder] Phase 4 High-End HUD Speedometer & Core Game Loop UI successfully built!");
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = panel.AddComponent<Image>();
            img.material = Canvas.GetDefaultCanvasMaterial();
            img.color = new Color(0.06f, 0.09f, 0.16f, 0.65f); // Glassmorphism Dark Theme
            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(350, 60);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = alignment;
            return textObj;
        }

        private static GameObject CreateWarningBanner(Transform parent, string name, string message, Color bannerColor)
        {
            GameObject banner = new GameObject(name);
            banner.transform.SetParent(parent, false);
            RectTransform rect = banner.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.75f);
            rect.anchorMax = new Vector2(0.5f, 0.75f);
            rect.sizeDelta = new Vector2(500, 50);

            Image img = banner.AddComponent<Image>();
            img.material = Canvas.GetDefaultCanvasMaterial();
            img.color = bannerColor;

            GameObject textObj = CreateText(banner.transform, "MessageText", message, 20, TextAlignmentOptions.Center);
            textObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return banner;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220, 50);
            rect.anchoredPosition = position;

            Image img = btnObj.AddComponent<Image>();
            img.material = Canvas.GetDefaultCanvasMaterial();
            img.color = new Color(0.02f, 0.71f, 0.83f); // Neon Cyan

            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = CreateText(btnObj.transform, "LabelText", label, 18, TextAlignmentOptions.Center);
            textObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            return btn;
        }
    }
}
#endif
