#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DrawAndRace.Vehicle;
using DrawAndRace.Core;

namespace DrawAndRace.Editor
{
    public static class CarPrefabBuilder
    {
        [MenuItem("DrawAndRace/Build 3D Sports Car Prefab")]
        public static GameObject BuildCarPrefab()
        {
            // 1. Create Car Root GameObject
            GameObject carObj = new GameObject("SportsCar");
            Rigidbody rb = carObj.AddComponent<Rigidbody>();
            rb.mass = 1450f;
            rb.drag = 0.05f;
            rb.angularDrag = 2.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Add Vehicle Controllers
            CarPhysicsController physicsController = carObj.AddComponent<CarPhysicsController>();
            OffTrackPenaltyHandler penaltyHandler = carObj.AddComponent<OffTrackPenaltyHandler>();
            LapTracker lapTracker = carObj.AddComponent<LapTracker>();

            // 2. High-Fidelity 3D Sports Car Body (Aerodynamic Proportions: 4.5m x 2.0m x 1.15m)
            GameObject bodyContainer = new GameObject("CarBodyContainer");
            bodyContainer.transform.SetParent(carObj.transform, false);

            // Metallic Red Paint Material
            Material metallicPaintMat = URPShaderUtility.CreateLitMaterial(new Color(0.85f, 0.08f, 0.08f), 0.9f, 0.92f); // Candy Apple Metallic Red
            Material carbonRoofMat = URPShaderUtility.CreateLitMaterial(new Color(0.1f, 0.1f, 0.12f), 0.7f, 0.8f);     // Carbon Fiber Roof
            Material glassMat = URPShaderUtility.CreateLitMaterial(new Color(0.05f, 0.05f, 0.08f), 0.95f, 0.95f);      // Tinted Obsidian Glass
            Material headlightMat = URPShaderUtility.CreateEmissiveMaterial(new Color(0.95f, 0.95f, 1.0f), 3.0f);     // Xenon Headlights (Glowing)
            Material tailLightMat = URPShaderUtility.CreateEmissiveMaterial(new Color(1.0f, 0.05f, 0.05f), 4.0f);      // Crimson LED Tail Lights (Glowing)
            Material chromeMat = URPShaderUtility.CreateLitMaterial(new Color(0.85f, 0.85f, 0.88f), 0.95f, 0.9f);       // Chrome Exhaust & Trim

            // Save Materials
            if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials")) AssetDatabase.CreateFolder("Assets/Art", "Materials");
            AssetDatabase.CreateAsset(metallicPaintMat, "Assets/Art/Materials/CarPaintMetallicRed.mat");

            // Main Lower Body Chassis
            GameObject mainBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBody.name = "MainChassis";
            mainBody.transform.SetParent(bodyContainer.transform, false);
            mainBody.transform.localScale = new Vector3(2.0f, 0.55f, 4.5f);
            mainBody.transform.localPosition = new Vector3(0, 0.45f, 0);
            mainBody.GetComponent<MeshRenderer>().sharedMaterial = metallicPaintMat;

            // Hood Slope
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.name = "FrontHood";
            hood.transform.SetParent(bodyContainer.transform, false);
            hood.transform.localScale = new Vector3(1.9f, 0.35f, 1.4f);
            hood.transform.localPosition = new Vector3(0, 0.55f, 1.2f);
            hood.transform.localRotation = Quaternion.Euler(-5f, 0, 0);
            hood.GetComponent<MeshRenderer>().sharedMaterial = metallicPaintMat;

            // Aerodynamic Cabin Roof
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "CabinRoof";
            cabin.transform.SetParent(bodyContainer.transform, false);
            cabin.transform.localScale = new Vector3(1.65f, 0.55f, 1.9f);
            cabin.transform.localPosition = new Vector3(0, 0.92f, -0.2f);
            cabin.GetComponent<MeshRenderer>().sharedMaterial = carbonRoofMat;

            // Tinted Windshield
            GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(bodyContainer.transform, false);
            windshield.transform.localScale = new Vector3(1.6f, 0.45f, 0.6f);
            windshield.transform.localPosition = new Vector3(0, 0.9f, 0.65f);
            windshield.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            windshield.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            // Dual Glowing Headlights
            GameObject hlLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hlLeft.name = "Headlight_L";
            hlLeft.transform.SetParent(bodyContainer.transform, false);
            hlLeft.transform.localScale = new Vector3(0.45f, 0.15f, 0.1f);
            hlLeft.transform.localPosition = new Vector3(-0.75f, 0.55f, 2.22f);
            hlLeft.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

            GameObject hlRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hlRight.name = "Headlight_R";
            hlRight.transform.SetParent(bodyContainer.transform, false);
            hlRight.transform.localScale = new Vector3(0.45f, 0.15f, 0.1f);
            hlRight.transform.localPosition = new Vector3(0.75f, 0.55f, 2.22f);
            hlRight.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

            // Glowing Crimson LED Tail Lights
            GameObject tlLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tlLeft.name = "TailLight_L";
            tlLeft.transform.SetParent(bodyContainer.transform, false);
            tlLeft.transform.localScale = new Vector3(0.6f, 0.15f, 0.1f);
            tlLeft.transform.localPosition = new Vector3(-0.7f, 0.55f, -2.26f);
            tlLeft.GetComponent<MeshRenderer>().sharedMaterial = tailLightMat;

            GameObject tlRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tlRight.name = "TailLight_R";
            tlRight.transform.SetParent(bodyContainer.transform, false);
            tlRight.transform.localScale = new Vector3(0.6f, 0.15f, 0.1f);
            tlRight.transform.localPosition = new Vector3(0.7f, 0.55f, -2.26f);
            tlRight.GetComponent<MeshRenderer>().sharedMaterial = tailLightMat;

            // 3. Create Wheel Colliders & Visual Wheel Assemblies
            GameObject wheelsContainer = new GameObject("Wheels");
            wheelsContainer.transform.SetParent(carObj.transform, false);

            WheelCollider flCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FL", new Vector3(-0.95f, 0.35f, 1.4f), chromeMat, out Transform flMesh);
            WheelCollider frCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FR", new Vector3(0.95f, 0.35f, 1.4f), chromeMat, out Transform frMesh);
            WheelCollider rlCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RL", new Vector3(-0.95f, 0.35f, -1.4f), chromeMat, out Transform rlMesh);
            WheelCollider rrCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RR", new Vector3(0.95f, 0.35f, -1.4f), chromeMat, out Transform rrMesh);

            // 4. Assign Wheel Collider References to CarPhysicsController
            SerializedObject serializedController = new SerializedObject(physicsController);
            serializedController.FindProperty("_frontLeftWheel").objectReferenceValue = flCollider;
            serializedController.FindProperty("_frontRightWheel").objectReferenceValue = frCollider;
            serializedController.FindProperty("_rearLeftWheel").objectReferenceValue = rlCollider;
            serializedController.FindProperty("_rearRightWheel").objectReferenceValue = rrCollider;

            serializedController.FindProperty("_frontLeftTransform").objectReferenceValue = flMesh;
            serializedController.FindProperty("_frontRightTransform").objectReferenceValue = frMesh;
            serializedController.FindProperty("_rearLeftTransform").objectReferenceValue = rlMesh;
            serializedController.FindProperty("_rearRightTransform").objectReferenceValue = rrMesh;
            serializedController.ApplyModifiedProperties();

            // 5. Save Prefab
            if (!AssetDatabase.IsValidFolder("Assets/Art/Models")) AssetDatabase.CreateFolder("Assets/Art", "Models");

            string prefabPath = "Assets/Art/Models/SportsCarPrefab.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(carObj, prefabPath);
            Debug.Log($"[CarPrefabBuilder] Hyper-Realistic 3D Sports Car Prefab created at {prefabPath}!");

            return carObj;
        }

        private static WheelCollider CreateWheelNode(Transform parent, string name, Vector3 localPos, Material rimMat, out Transform visualTransform)
        {
            GameObject wheelObj = new GameObject(name);
            wheelObj.transform.SetParent(parent, false);
            wheelObj.transform.localPosition = localPos;

            WheelCollider collider = wheelObj.AddComponent<WheelCollider>();
            collider.radius = 0.38f;
            collider.suspensionDistance = 0.18f;
            collider.wheelDampingRate = 1.0f;

            JointSpring spring = collider.suspensionSpring;
            spring.spring = 38000f;
            spring.damper = 4800f;
            spring.targetPosition = 0.5f;
            collider.suspensionSpring = spring;

            // Visual Cylinder Wheel Mesh (Black Rubber Tire + Alloy Rim Core)
            GameObject wheelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheelVisual.name = $"{name}_Visual";
            wheelVisual.transform.SetParent(wheelObj.transform, false);
            wheelVisual.transform.localScale = new Vector3(0.76f, 0.18f, 0.76f);
            wheelVisual.transform.localRotation = Quaternion.Euler(0, 0, 90f);

            Material tireMat = URPShaderUtility.CreateLitMaterial(new Color(0.08f, 0.08f, 0.09f), 0.2f, 0.4f); // Black Rubber Tire
            wheelVisual.GetComponent<MeshRenderer>().sharedMaterial = tireMat;

            // Alloy Rim Core
            GameObject rimCore = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rimCore.name = "AlloyRim";
            rimCore.transform.SetParent(wheelVisual.transform, false);
            rimCore.transform.localScale = new Vector3(0.6f, 1.02f, 0.6f);
            rimCore.GetComponent<MeshRenderer>().sharedMaterial = rimMat;

            visualTransform = wheelVisual.transform;
            return collider;
        }
    }
}
#endif
