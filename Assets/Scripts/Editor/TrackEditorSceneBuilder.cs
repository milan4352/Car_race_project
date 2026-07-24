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
        [MenuItem("DrawAndRace/Build Track Editor Scene")]
        public static void BuildScene()
        {
            // 1. Create New Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Directional Light & Global Volume
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject volumeObj = new GameObject("Global Volume");
            UnityEngine.Rendering.Volume volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
            volume.isGlobal = true;

            // 3. Create Ground Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.localScale = new Vector3(50, 1, 50); // 500m x 500m
            Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.2f, 0.5f, 0.2f); // Grass green
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

            // 4. Create Main Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camObj.AddComponent<UniversalAdditionalCameraData>();
            camObj.transform.SetPositionAndRotation(new Vector3(0, 80, -60), Quaternion.Euler(55, 0, 0)); // Top-down angled editor view

            // 5. Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // 6. Create UI Canvas for Track Drawing
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
            img.color = new Color(0, 0, 0, 0.05f); // Transparent raycast target

            TrackDrawingCanvas drawingCanvas = drawPanel.AddComponent<TrackDrawingCanvas>();

            // 7. Create Track Editor Manager & Subcomponents
            GameObject trackEditorObj = new GameObject("TrackEditorManager");
            TrackSplineGenerator splineGen = trackEditorObj.AddComponent<TrackSplineGenerator>();
            RoadMeshExtruder roadExtruder = trackEditorObj.AddComponent<RoadMeshExtruder>();
            CheckpointGenerator checkpointGen = trackEditorObj.AddComponent<CheckpointGenerator>();
            ProceduralEnvironmentScatterer scatterer = trackEditorObj.AddComponent<ProceduralEnvironmentScatterer>();

            TrackEditorManager manager = trackEditorObj.AddComponent<TrackEditorManager>();

            // Create and assign Asphalt Material
            Material asphaltMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            asphaltMat.color = new Color(0.15f, 0.15f, 0.15f); // Dark asphalt
            asphaltMat.SetFloat("_Smoothness", 0.3f);

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
