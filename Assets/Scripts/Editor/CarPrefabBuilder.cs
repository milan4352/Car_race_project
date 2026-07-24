#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DrawAndRace.Vehicle;
using DrawAndRace.Core;

namespace DrawAndRace.Editor
{
    public static class CarPrefabBuilder
    {
        [MenuItem("DrawAndRace/Build All 3 Real-Life Sports Cars")]
        public static void BuildAllCars()
        {
            BuildCarPrefab_Red();
            BuildCarPrefab_Blue();
            BuildCarPrefab_Gold();
            Debug.Log("[CarPrefabBuilder] Successfully created all 3 real-life sports car prefabs in Assets/Art/Models/!");
        }

        [MenuItem("DrawAndRace/Build 3D Sports Car - Metallic Red Supercar")]
        public static GameObject BuildCarPrefab_Red()
        {
            return CreateCarModel("SportsCar_Red", new Color(0.85f, 0.08f, 0.08f), 0.92f, 0.95f, CarStyle.SupercarWithSpoiler);
        }

        [MenuItem("DrawAndRace/Build 3D Sports Car - Cobalt Blue GT Racer")]
        public static GameObject BuildCarPrefab_Blue()
        {
            return CreateCarModel("SportsCar_Blue", new Color(0.08f, 0.35f, 0.85f), 0.88f, 0.90f, CarStyle.GTRacerWithVents);
        }

        [MenuItem("DrawAndRace/Build 3D Sports Car - Liquid Gold Hypercar")]
        public static GameObject BuildCarPrefab_Gold()
        {
            return CreateCarModel("SportsCar_Gold", new Color(0.95f, 0.75f, 0.12f), 0.96f, 0.96f, CarStyle.WidebodyHypercar);
        }

        private enum CarStyle { SupercarWithSpoiler, GTRacerWithVents, WidebodyHypercar }

        private static GameObject CreateCarModel(string carName, Color paintColor, float metallic, float smoothness, CarStyle style)
        {
            // 1. Create Car Root GameObject
            GameObject carObj = new GameObject(carName);
            Rigidbody rb = carObj.AddComponent<Rigidbody>();
            rb.mass = 1420f;
            rb.drag = 0.04f;
            rb.angularDrag = 2.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Vehicle Logic Controllers
            CarPhysicsController physicsController = carObj.AddComponent<CarPhysicsController>();
            OffTrackPenaltyHandler penaltyHandler = carObj.AddComponent<OffTrackPenaltyHandler>();
            LapTracker lapTracker = carObj.AddComponent<LapTracker>();

            // 2. High-Fidelity 3D Body Mesh Container
            GameObject bodyContainer = new GameObject("CarBodyContainer");
            bodyContainer.transform.SetParent(carObj.transform, false);

            // Materials
            Material paintMat = URPShaderUtility.CreateLitMaterial(paintColor, metallic, smoothness);
            Material carbonMat = URPShaderUtility.CreateLitMaterial(new Color(0.1f, 0.1f, 0.12f), 0.8f, 0.85f);
            Material glassMat = URPShaderUtility.CreateLitMaterial(new Color(0.05f, 0.05f, 0.08f), 0.95f, 0.95f);
            Material headlightMat = URPShaderUtility.CreateEmissiveMaterial(new Color(0.95f, 0.98f, 1.0f), 3.5f);
            Material tailLightMat = URPShaderUtility.CreateEmissiveMaterial(new Color(1.0f, 0.05f, 0.05f), 4.5f);
            Material chromeMat = URPShaderUtility.CreateLitMaterial(new Color(0.88f, 0.88f, 0.9f), 0.95f, 0.9f);

            // Save Paint Material Asset
            if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials")) AssetDatabase.CreateFolder("Assets/Art", "Materials");
            AssetDatabase.CreateAsset(paintMat, $"Assets/Art/Materials/{carName}_Paint.mat");

            // Main Lower Chassis
            GameObject mainChassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainChassis.name = "MainChassis";
            mainChassis.transform.SetParent(bodyContainer.transform, false);
            mainChassis.transform.localScale = new Vector3(2.05f, 0.5f, 4.5f);
            mainChassis.transform.localPosition = new Vector3(0, 0.42f, 0);
            mainChassis.GetComponent<MeshRenderer>().sharedMaterial = paintMat;

            // Hood Section
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.name = "Hood";
            hood.transform.SetParent(bodyContainer.transform, false);
            hood.transform.localScale = new Vector3(1.92f, 0.32f, 1.4f);
            hood.transform.localPosition = new Vector3(0, 0.52f, 1.25f);
            hood.transform.localRotation = Quaternion.Euler(-5f, 0, 0);
            hood.GetComponent<MeshRenderer>().sharedMaterial = paintMat;

            // Cabin Roof
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "CabinRoof";
            cabin.transform.SetParent(bodyContainer.transform, false);
            cabin.transform.localScale = new Vector3(1.6f, 0.52f, 1.85f);
            cabin.transform.localPosition = new Vector3(0, 0.88f, -0.2f);
            cabin.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;

            // Windshield
            GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(bodyContainer.transform, false);
            windshield.transform.localScale = new Vector3(1.55f, 0.42f, 0.55f);
            windshield.transform.localPosition = new Vector3(0, 0.86f, 0.65f);
            windshield.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            windshield.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            // Headlights & Tail Lights
            GameObject hlL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hlL.name = "Headlight_L";
            hlL.transform.SetParent(bodyContainer.transform, false);
            hlL.transform.localScale = new Vector3(0.45f, 0.14f, 0.1f);
            hlL.transform.localPosition = new Vector3(-0.75f, 0.52f, 2.22f);
            hlL.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

            GameObject hlR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hlR.name = "Headlight_R";
            hlR.transform.SetParent(bodyContainer.transform, false);
            hlR.transform.localScale = new Vector3(0.45f, 0.14f, 0.1f);
            hlR.transform.localPosition = new Vector3(0.75f, 0.52f, 2.22f);
            hlR.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

            GameObject tlL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tlL.name = "TailLight_L";
            tlL.transform.SetParent(bodyContainer.transform, false);
            tlL.transform.localScale = new Vector3(0.65f, 0.14f, 0.1f);
            tlL.transform.localPosition = new Vector3(-0.7f, 0.52f, -2.26f);
            tlL.GetComponent<MeshRenderer>().sharedMaterial = tailLightMat;

            GameObject tlR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tlR.name = "TailLight_R";
            tlR.transform.SetParent(bodyContainer.transform, false);
            tlR.transform.localScale = new Vector3(0.65f, 0.14f, 0.1f);
            tlR.transform.localPosition = new Vector3(0.7f, 0.52f, -2.26f);
            tlR.GetComponent<MeshRenderer>().sharedMaterial = tailLightMat;

            // Style-Specific Custom Aerodynamic Parts
            if (style == CarStyle.SupercarWithSpoiler)
            {
                // Rear Carbon GT Spoiler Wing
                GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wing.name = "RearGTSpoiler";
                wing.transform.SetParent(bodyContainer.transform, false);
                wing.transform.localScale = new Vector3(1.95f, 0.08f, 0.45f);
                wing.transform.localPosition = new Vector3(0, 1.15f, -2.05f);
                wing.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;

                // Struts
                GameObject strutL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strutL.transform.SetParent(wing.transform, false);
                strutL.transform.localScale = new Vector3(0.05f, 3.5f, 0.3f);
                strutL.transform.localPosition = new Vector3(-0.35f, -1.8f, 0);
                strutL.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;

                GameObject strutR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strutR.transform.SetParent(wing.transform, false);
                strutR.transform.localScale = new Vector3(0.05f, 3.5f, 0.3f);
                strutR.transform.localPosition = new Vector3(0.35f, -1.8f, 0);
                strutR.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
            }
            else if (style == CarStyle.GTRacerWithVents)
            {
                // Hood Vents & Racing Splitter
                GameObject splitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                splitter.name = "FrontSplitter";
                splitter.transform.SetParent(bodyContainer.transform, false);
                splitter.transform.localScale = new Vector3(2.1f, 0.08f, 0.4f);
                splitter.transform.localPosition = new Vector3(0, 0.2f, 2.3f);
                splitter.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
            }
            else if (style == CarStyle.WidebodyHypercar)
            {
                // Roof Scoop & Side Skirts
                GameObject roofScoop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roofScoop.name = "RoofAirScoop";
                roofScoop.transform.SetParent(bodyContainer.transform, false);
                roofScoop.transform.localScale = new Vector3(0.45f, 0.2f, 0.6f);
                roofScoop.transform.localPosition = new Vector3(0, 1.2f, 0.1f);
                roofScoop.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
            }

            // 3. Wheel Assemblies
            GameObject wheelsContainer = new GameObject("Wheels");
            wheelsContainer.transform.SetParent(carObj.transform, false);

            WheelCollider flCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FL", new Vector3(-0.95f, 0.35f, 1.4f), chromeMat, out Transform flMesh);
            WheelCollider frCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FR", new Vector3(0.95f, 0.35f, 1.4f), chromeMat, out Transform frMesh);
            WheelCollider rlCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RL", new Vector3(-0.95f, 0.35f, -1.4f), chromeMat, out Transform rlMesh);
            WheelCollider rrCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RR", new Vector3(0.95f, 0.35f, -1.4f), chromeMat, out Transform rrMesh);

            // 4. Assign Wheel Collider References to Controller
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

            string prefabPath = $"Assets/Art/Models/{carName}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(carObj, prefabPath);
            Debug.Log($"[CarPrefabBuilder] 3D Car Model '{carName}' saved to {prefabPath}!");

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

            GameObject wheelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheelVisual.name = $"{name}_Visual";
            wheelVisual.transform.SetParent(wheelObj.transform, false);
            wheelVisual.transform.localScale = new Vector3(0.76f, 0.18f, 0.76f);
            wheelVisual.transform.localRotation = Quaternion.Euler(0, 0, 90f);

            Material tireMat = URPShaderUtility.CreateLitMaterial(new Color(0.08f, 0.08f, 0.09f), 0.2f, 0.4f);
            wheelVisual.GetComponent<MeshRenderer>().sharedMaterial = tireMat;

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
