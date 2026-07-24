using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawAndRace.Core
{
    /// <summary>
    /// Programmatically verifies and configures high-graphics rendering settings
    /// for the Universal Render Pipeline (URP) on startup.
    /// </summary>
    public class RenderPipelineConfigurator : MonoBehaviour
    {
        [Header("Target Graphics Settings")]
        [SerializeField] private bool _enableSoftShadows = true;
        [SerializeField] private float _shadowDistance = 150f;
        [SerializeField] private int _shadowCascadeCount = 4;
        [SerializeField] private bool _enableHDR = true;

        private void Awake()
        {
            ApplyHighQualityPipelineSettings();
        }

        public void ApplyHighQualityPipelineSettings()
        {
            var currentPipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (currentPipeline != null)
            {
                currentPipeline.shadowDistance = _shadowDistance;
                currentPipeline.shadowCascadeCount = _shadowCascadeCount;
                currentPipeline.supportsHDR = _enableHDR;
                Debug.Log($"[DrawAndRace] URP High-Graphics settings configured: ShadowDistance={_shadowDistance}m, Cascades={_shadowCascadeCount}, HDR={_enableHDR}");
            }
            else
            {
                Debug.LogWarning("[DrawAndRace] Current Render Pipeline is not UniversalRenderPipelineAsset. Ensure URP asset is assigned in Project Settings.");
            }
        }
    }
}
