#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DrawAndRace.Core;

namespace DrawAndRace.Editor
{
    public static class Phase6WrapUpSceneBuilder
    {
        [MenuItem("DrawAndRace/5. One-Click Full Project Builder (Play Instant Game)", false, 5)]
        public static void BuildFullProject()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[DrawAndRace] STARTING ONE-CLICK FULL PROJECT BUILDER...");
            Debug.Log("=========================================================================");

            // Step 1: Build All 3 Sports Cars
            CarPrefabBuilder.BuildAllCars();

            // Step 2: Setup Simple Real Track Scene
            TrackEditorSceneBuilder.BuildSimpleRealTrackScene();

            // Step 3: Setup Phase 4 Full Game Loop & UI
            Phase4UISceneBuilder.BuildPhase4UI();

            // Step 4: Setup Phase 5 Audio, FX & Mobile Touch Controls
            Phase5FXAudioSceneBuilder.BuildPhase5AudioAndFX();

            // Step 5: Attach PerformanceOptimizer
            GameObject perfObj = new GameObject("PerformanceOptimizer");
            PerformanceOptimizer perf = perfObj.AddComponent<PerformanceOptimizer>();
            perf.ApplyPerformanceSettings();

            // Step 6: Configure Editor Build Settings Scenes
            string scenePath = "Assets/Scenes/TrackEditorScene.unity";
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            // Save Scene
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("=========================================================================");
            Debug.Log("🏆 [DrawAndRace] CONGRATULATIONS! PROJECT IS 100% COMPLETE AND READY!");
            Debug.Log("🏆 Click PLAY ▶️ to test your high-end 3D racing game!");
            Debug.Log("=========================================================================");
        }
    }
}
#endif
