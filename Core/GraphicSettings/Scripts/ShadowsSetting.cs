using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace GraphicSettings
{
    public enum ShadowCascadesOption
    {
        NoCascades,
        TwoCascades,
        FourCascades
    }

    public class ShadowsSetting : MonoBehaviour, IGraphicSetting
    {
        public const string SAVE_KEY = "GRAPHIC_SETTING_SHADOWS";
        public ShadowCascadesOption setting = ShadowCascadesOption.NoCascades;
        public Toggle toggle;
        public Button button;
        public bool applyImmediately = true;
        public bool ApplyImmediately { get { return applyImmediately; } set { applyImmediately = value; } }

        private bool _isOn;

        private void Start()
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(GetURPShadowQuality() == setting);
                toggle.onValueChanged.AddListener(OnToggle);
            }
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggle);
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        public void OnToggle(bool isOn)
        {
            this._isOn = isOn;
            if (isOn)
                OnClick();
        }

        public void OnClick()
        {
            if (ApplyImmediately)
            {
                _isOn = true;
                Apply();
            }
        }

        public void Apply()
        {
            if (_isOn)
            {
                SetURPShadowQuality(setting);
                PlayerPrefs.SetInt(SAVE_KEY, (int)setting);
                PlayerPrefs.Save();
                QualityLevelSetting.MarkAsCustomLevel();
            }
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY) && QualityLevelSetting.IsCustomQualityLevel())
            {
                ShadowCascadesOption savedQuality = (ShadowCascadesOption)PlayerPrefs.GetInt(SAVE_KEY);
                ShadowsSetting shadowsSetting = FindObjectOfType<ShadowsSetting>();
                if (shadowsSetting != null)
                    shadowsSetting.SetURPShadowQuality(savedQuality);
            }
        }

        private ShadowCascadesOption GetURPShadowQuality()
        {
            UniversalRenderPipelineAsset urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                return (GraphicSettings.ShadowCascadesOption)urpAsset.shadowCascadeOption;
            }
            return ShadowCascadesOption.NoCascades;
        }

        private void SetURPShadowQuality(ShadowCascadesOption quality)
        {
            UniversalRenderPipelineAsset urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                switch (quality)
                {
                    case ShadowCascadesOption.NoCascades:
                        urpAsset.shadowCascadeOption = UnityEngine.Rendering.Universal.ShadowCascadesOption.NoCascades;
                        urpAsset.shadowDistance = 0f;
                        break;
                    case ShadowCascadesOption.TwoCascades:
                        urpAsset.shadowCascadeOption = UnityEngine.Rendering.Universal.ShadowCascadesOption.TwoCascades;
                        urpAsset.shadowDistance = 100f;
                        break;
                    case ShadowCascadesOption.FourCascades:
                        urpAsset.shadowCascadeOption = UnityEngine.Rendering.Universal.ShadowCascadesOption.FourCascades;
                        urpAsset.shadowDistance = 500f;
                        break;
                }
            }
        }
    }
}
