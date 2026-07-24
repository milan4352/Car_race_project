#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using DrawAndRace.TrackEditor;
using DrawAndRace.Core;

namespace DrawAndRace.Editor
{
    public static class TrackEditorSceneBuilder
    {
        [MenuItem("DrawAndRace/Setup 3D Track Editor Scene")]
        public static void BuildScene()
        {
            // 1. Create New Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Directional Sun Light & Global Volume
            GameObject lightObj = new GameObject("Directional Sun Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1.0f, 0.96f, 0.88f); // Warm sunlight
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject volumeObj = new GameObject("Global Volume");
            UnityEngine.Rendering.Volume volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
            volume.isGlobal = true;

            // 3. Create Grass Terrain Ground Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.localScale = new Vector3(80, 1, 80); // 800m x 800m
            Material grassMat = URPShaderUtility.CreateLitMaterial(new Color(0.18f, 0.45f, 0.18f), 0.1f, 0.3f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = grassMat;

            // 4. Create Main Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.45f, 0.65f, 0.85f);
            camObj.AddComponent<UniversalAdditionalCameraData>();
            camObj.transform.SetPositionAndRotation(new Vector3(0, 90, -70), Quaternion.Euler(55, 0, 0)); // Top-down angled editor view

            // 5. Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // 6. Create UI Canvas for Track Drawing (with default UI material to prevent magenta pink shader bug)
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Add Drawing Panel Image
            GameObject drawPanel = new GameObject("DrawingCanvasPanel");
            drawPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = drawPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image img = drawPanel.AddComponent<Image>();
            img.material = Canvas.GetDefaultCanvasMaterial(); // Fix URP Pink Canvas Shader
            img.color = new Color(0, 0, 0, 0.02f); // Transparent raycast target

            TrackDrawingCanvas drawingCanvas = drawPanel.AddComponent<TrackDrawingCanvas>();

            // 7. Create Track Editor Manager & Subcomponents
            GameObject trackEditorObj = new GameObject("TrackEditorManager");
            TrackSplineGenerator splineGen = trackEditorObj.AddComponent<TrackSplineGenerator>();
            RoadMeshExtruder roadExtruder = trackEditorObj.AddComponent<RoadMeshExtruder>();
            CheckpointGenerator checkpointGen = trackEditorObj.AddComponent<CheckpointGenerator>();
            ProceduralEnvironmentScatterer scatterer = trackEditorObj.AddComponent<ProceduralEnvironmentScatterer>();

            TrackEditorManager manager = trackEditorObj.AddComponent<TrackEditorManager>();

            // Create and assign Asphalt Material
            Material asphaltMat = URPShaderUtility.CreateLitMaterial(new Color(0.12f, 0.12f, 0.14f), 0.35f, 0.45f);

            // Save Material asset
            if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials")) AssetDatabase.CreateFolder("Assets/Art", "Materials");
            AssetDatabase.CreateAsset(asphaltMat, "Assets/Art/Materials/AsphaltPBR.mat");

            SerializedObject serializedExtruder = new SerializedObject(roadExtruder);
            serializedExtruder.FindProperty("_roadMaterial").objectReferenceValue = asphaltMat;
            serializedExtruder.ApplyModifiedProperties();

            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("_drawingCanvas").objectReferenceValue = drawingCanvas;
            serializedManager.FindProperty("_editorCamera").objectReferenceValue = cam;
            serializedManager.ApplyModifiedProperties();

            // 8. Save Scene as TrackEditorScene.unity
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            string scenePath = "Assets/Scenes/TrackEditorScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[DrawAndRace] TrackEditorScene successfully built and saved to {scenePath}!");
        }
    }
}
#endif
