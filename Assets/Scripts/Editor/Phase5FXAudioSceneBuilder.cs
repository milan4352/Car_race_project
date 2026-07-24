#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DrawAndRace.Core;
using DrawAndRace.UI;
using DrawAndRace.Vehicle;

namespace DrawAndRace.Editor
{
    public static class Phase5FXAudioSceneBuilder
    {
        [MenuItem("DrawAndRace/4. Setup Phase 5 Audio, FX & Mobile Touch Controls", false, 4)]
        public static void BuildPhase5AudioAndFX()
        {
            // 1. Find or Attach Audio & FX Controllers to Active Car
            CarPhysicsController car = Object.FindObjectOfType<CarPhysicsController>();
            if (car != null)
            {
                RacingAudioController audioController = car.GetComponent<RacingAudioController>();
                if (audioController == null) car.gameObject.AddComponent<RacingAudioController>();

                RacingFXController fxController = car.GetComponent<RacingFXController>();
                if (fxController == null) car.gameObject.AddComponent<RacingFXController>();

                Debug.Log("[Phase5FXAudioSceneBuilder] Attached RacingAudioController & RacingFXController to active vehicle!");
            }
            else
            {
                Debug.LogWarning("[Phase5FXAudioSceneBuilder] No active CarPhysicsController found in scene. Please run Menu 2 (Setup Simple Real Track Scene) first.");
            }

            // 2. Find Canvas & Create Mobile Input UI Panel
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                MobileInputController mobileController = canvas.GetComponent<MobileInputController>();
                if (mobileController == null) canvas.gameObject.AddComponent<MobileInputController>();
                Debug.Log("[Phase5FXAudioSceneBuilder] MobileInputController added to UI Canvas!");
            }

            Debug.Log("[Phase5FXAudioSceneBuilder] Phase 5 Sound SFX Engine, FX Particles, and Mobile Touch Controls successfully set up!");
        }
    }
}
#endif
