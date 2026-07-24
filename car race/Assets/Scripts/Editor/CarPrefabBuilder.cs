#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DrawAndRace.Vehicle;

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
            rb.mass = 1400f;
            rb.drag = 0.05f;
            rb.angularDrag = 2.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Add Vehicle Components
            CarPhysicsController physicsController = carObj.AddComponent<CarPhysicsController>();
            OffTrackPenaltyHandler penaltyHandler = carObj.AddComponent<OffTrackPenaltyHandler>();
            LapTracker lapTracker = carObj.AddComponent<LapTracker>();

            // 2. Create Car Body Mesh Visuals
            GameObject bodyMeshObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyMeshObj.name = "CarBodyMesh";
            bodyMeshObj.transform.SetParent(carObj.transform, false);
            bodyMeshObj.transform.localScale = new Vector3(2.0f, 0.9f, 4.4f);
            bodyMeshObj.transform.localPosition = new Vector3(0, 0.6f, 0);

            // Car Roof Visual
            GameObject cabinObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinObj.name = "CarCabin";
            cabinObj.transform.SetParent(bodyMeshObj.transform, false);
            cabinObj.transform.localScale = new Vector3(0.85f, 0.7f, 0.5f);
            cabinObj.transform.localPosition = new Vector3(0, 0.7f, -0.1f);

            // Create Metallic Red Car Paint Material
            Material carPaintMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            carPaintMat.color = new Color(0.85f, 0.1f, 0.1f); // Metallic Red
            carPaintMat.SetFloat("_Metallic", 0.85f);
            carPaintMat.SetFloat("_Smoothness", 0.9f);

            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
                AssetDatabase.CreateFolder("Assets/Art", "Materials");
            }
            AssetDatabase.CreateAsset(carPaintMat, "Assets/Art/Materials/CarPaintMetallicRed.mat");

            bodyMeshObj.GetComponent<MeshRenderer>().sharedMaterial = carPaintMat;
            cabinObj.GetComponent<MeshRenderer>().sharedMaterial = carPaintMat;

            // 3. Create Wheel Colliders & Visual Wheel Meshes
            GameObject wheelsContainer = new GameObject("Wheels");
            wheelsContainer.transform.SetParent(carObj.transform, false);

            WheelCollider flCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FL", new Vector3(-0.95f, 0.35f, 1.4f), out Transform flMesh);
            WheelCollider frCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_FR", new Vector3(0.95f, 0.35f, 1.4f), out Transform frMesh);
            WheelCollider rlCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RL", new Vector3(-0.95f, 0.35f, -1.4f), out Transform rlMesh);
            WheelCollider rrCollider = CreateWheelNode(wheelsContainer.transform, "Wheel_RR", new Vector3(0.95f, 0.35f, -1.4f), out Transform rrMesh);

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
            if (!AssetDatabase.IsValidFolder("Assets/Art/Models"))
            {
                AssetDatabase.CreateFolder("Assets/Art", "Models");
            }

            string prefabPath = "Assets/Art/Models/SportsCarPrefab.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(carObj, prefabPath);
            Debug.Log($"[CarPrefabBuilder] 3D Sports Car Prefab successfully created at {prefabPath}!");

            return carObj;
        }

        private static WheelCollider CreateWheelNode(Transform parent, string name, Vector3 localPos, out Transform visualTransform)
        {
            GameObject wheelObj = new GameObject(name);
            wheelObj.transform.SetParent(parent, false);
            wheelObj.transform.localPosition = localPos;

            WheelCollider collider = wheelObj.AddComponent<WheelCollider>();
            collider.radius = 0.35f;
            collider.suspensionDistance = 0.2f;
            collider.wheelDampingRate = 1.0f;

            JointSpring spring = collider.suspensionSpring;
            spring.spring = 35000f;
            spring.damper = 4500f;
            spring.targetPosition = 0.5f;
            collider.suspensionSpring = spring;

            // Visual Cylinder Wheel Mesh
            GameObject wheelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheelVisual.name = $"{name}_Visual";
            wheelVisual.transform.SetParent(wheelObj.transform, false);
            wheelVisual.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);
            wheelVisual.transform.localRotation = Quaternion.Euler(0, 0, 90f);

            // Wheel Tire Material (Black)
            Material tireMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tireMat.color = new Color(0.08f, 0.08f, 0.08f);
            wheelVisual.GetComponent<MeshRenderer>().sharedMaterial = tireMat;

            visualTransform = wheelVisual.transform;
            return collider;
        }
    }
}
#endif
