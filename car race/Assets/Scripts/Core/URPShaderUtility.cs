using UnityEngine;

namespace DrawAndRace.Core
{
    public static class URPShaderUtility
    {
        private static Shader _litShader;
        private static Shader _unlitShader;
        private static Shader _uiShader;

        public static Shader GetLitShader()
        {
            if (_litShader != null) return _litShader;
            _litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_litShader == null) _litShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (_litShader == null) _litShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (_litShader == null) _litShader = Shader.Find("Standard");
            return _litShader;
        }

        public static Shader GetUnlitShader()
        {
            if (_unlitShader != null) return _unlitShader;
            _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (_unlitShader == null) _unlitShader = Shader.Find("Unlit/Color");
            if (_unlitShader == null) _unlitShader = Shader.Find("Sprites/Default");
            return _unlitShader;
        }

        public static Shader GetUIShader()
        {
            if (_uiShader != null) return _uiShader;
            _uiShader = Shader.Find("UI/Default");
            if (_uiShader == null) _uiShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
            if (_uiShader == null) _uiShader = Shader.Find("Sprites/Default");
            return _uiShader;
        }

        public static Material CreateLitMaterial(Color color, float metallic = 0.5f, float smoothness = 0.5f)
        {
            Material mat = new Material(GetLitShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        public static Material CreateEmissiveMaterial(Color color, float emissionIntensity = 2.0f)
        {
            Material mat = new Material(GetLitShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emissionIntensity);
            }
            return mat;
        }

        public static Material CreateUIMaterial()
        {
            Shader uiShader = GetUIShader();
            return uiShader != null ? new Material(uiShader) : Canvas.GetDefaultCanvasMaterial();
        }
    }
}
