using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text masterLabel;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text sfxLabel;
        [SerializeField] private TMP_Text languageLabel;
        [SerializeField] private TMP_Text languageValueText;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button languagePrevButton;
        [SerializeField] private Button languageNextButton;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button mainMenuButton;

        private bool _open;
        private float _previousTimeScale = 1f;

        private void Awake()
        {
            EnsureLocalization();
            BindButtons();
            RefreshLanguageValue();
            RefreshTexts();
            RefreshSliders();
            SetOpen(false, false);
        }

        public void Open()
        {
            var game = FindFirstObjectByType<GameController>();
            game?.PlayerInput.CancelPointerTracking();
            SetOpen(true, true);
            AudioManager.Instance?.PlaySfx(AudioCue.UiOpen);
        }

        public void Close()
        {
            var game = FindFirstObjectByType<GameController>();
            game?.PlayerInput.CancelPointerTracking();
            SetOpen(false, true);
            AudioManager.Instance?.PlaySfx(AudioCue.UiClose);
        }

        private void BindButtons()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveAllListeners();
                openButton.onClick.AddListener(Open);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                    var game = FindFirstObjectByType<GameController>();
                    game?.PlayerInput.CancelPointerTracking();
                    SetOpen(false, true);
                    SceneFlowController.LoadBossRush(false);
                });
            }

            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveAllListeners();
                masterSlider.onValueChanged.AddListener(value =>
                {
                    ProjectSettingsState.MasterVolume = value;
                    ProjectSettingsState.Save();
                    AudioManager.Instance?.ApplySettings();
                });
            }

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(value =>
                {
                    ProjectSettingsState.MusicVolume = value;
                    ProjectSettingsState.Save();
                    AudioManager.Instance?.ApplySettings();
                });
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(value =>
                {
                    ProjectSettingsState.SfxVolume = value;
                    ProjectSettingsState.Save();
                    AudioManager.Instance?.ApplySettings();
                });
            }

            if (languagePrevButton != null)
            {
                languagePrevButton.onClick.RemoveAllListeners();
                languagePrevButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                    CycleLanguage(-1);
                });
            }

            if (languageNextButton != null)
            {
                languageNextButton.onClick.RemoveAllListeners();
                languageNextButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                    CycleLanguage(1);
                });
            }
        }

        private void CycleLanguage(int direction = 1)
        {
            var locales = GetOrderedUniqueLocales();
            if (locales.Count == 0)
            {
                return;
            }

            var currentIndex = 0;

            for (var i = 0; i < locales.Count; i++)
            {
                if (NormalizeLocaleCode(locales[i].LocaleCode) == NormalizeLocaleCode(ProjectSettingsState.Locale))
                {
                    currentIndex = i;
                    break;
                }
            }

            var nextIndex = (currentIndex + direction + locales.Count) % locales.Count;
            LocalizationRuntime.ApplyLocale(localization, locales[nextIndex].LocaleCode);
            AudioManager.Instance?.ApplySettings();
            RefreshTexts();
        }

        private void RefreshLanguageValue()
        {
            if (languageValueText == null || localization == null)
            {
                return;
            }

            var locales = GetOrderedUniqueLocales();
            for (var i = 0; i < locales.Count; i++)
            {
                if (NormalizeLocaleCode(locales[i].LocaleCode) == NormalizeLocaleCode(ProjectSettingsState.Locale))
                {
                    languageValueText.text = GetLocaleDisplayName(locales[i]);
                    return;
                }
            }
        }

        private List<TutorialLocaleTable> GetOrderedUniqueLocales()
        {
            var result = new List<TutorialLocaleTable>();
            if (localization == null)
            {
                return result;
            }

            var locales = localization.Locales;
            AddLocaleIfMissing(result, locales, "en");
            AddLocaleIfMissing(result, locales, "vi");

            for (var i = 0; i < locales.Count; i++)
            {
                var locale = locales[i];
                if (locale == null || string.IsNullOrWhiteSpace(locale.LocaleCode))
                {
                    continue;
                }

                var normalizedCode = NormalizeLocaleCode(locale.LocaleCode);
                var exists = false;
                for (var j = 0; j < result.Count; j++)
                {
                    if (NormalizeLocaleCode(result[j].LocaleCode) == normalizedCode)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    result.Add(locale);
                }
            }

            return result;
        }

        private static void AddLocaleIfMissing(List<TutorialLocaleTable> result, System.Collections.Generic.IReadOnlyList<TutorialLocaleTable> source, string localeCode)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var locale = source[i];
                if (locale == null || NormalizeLocaleCode(locale.LocaleCode) != localeCode)
                {
                    continue;
                }

                result.Add(locale);
                return;
            }
        }

        private static string NormalizeLocaleCode(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return string.Empty;
            }

            var normalized = localeCode.Trim().ToLowerInvariant();
            return normalized switch
            {
                "english" => "en",
                "vietnamese" => "vi",
                "tiengviet" => "vi",
                "tiếngviệt" => "vi",
                "tieng viet" => "vi",
                "tiếng việt" => "vi",
                _ => normalized
            };
        }

        private static string GetLocaleDisplayName(TutorialLocaleTable locale)
        {
            return NormalizeLocaleCode(locale.LocaleCode) switch
            {
                "en" => "English",
                "vi" => "Tiếng Việt",
                _ => string.IsNullOrWhiteSpace(locale.DisplayName) ? locale.LocaleCode : locale.DisplayName
            };
        }

        private void RefreshTexts()
        {
            if (titleText != null) titleText.text = T("settings.title", "Settings");
            if (masterLabel != null) masterLabel.text = T("settings.master", "Master Volume");
            if (musicLabel != null) musicLabel.text = T("settings.music", "Music");
            if (sfxLabel != null) sfxLabel.text = T("settings.sfx", "SFX");
            if (languageLabel != null) languageLabel.text = T("settings.language", "Language");
            SetButtonLabel(openButton, T("settings.open", "Settings"));
            SetButtonLabel(closeButton, T("settings.close", "Close"));
            SetButtonLabel(mainMenuButton, T("settings.main_menu", "Main Menu"));
            RefreshLanguageValue();
        }

        private void RefreshSliders()
        {
            if (masterSlider != null) masterSlider.SetValueWithoutNotify(ProjectSettingsState.MasterVolume);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(ProjectSettingsState.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(ProjectSettingsState.SfxVolume);
        }

        private void SetOpen(bool value, bool pauseGameplay)
        {
            _open = value;
            if (rootGroup != null)
            {
                rootGroup.alpha = value ? 1f : 0f;
                rootGroup.blocksRaycasts = value;
                rootGroup.interactable = value;
            }

            if (pauseGameplay)
            {
                if (value)
                {
                    _previousTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                }
                else
                {
                    Time.timeScale = _previousTimeScale <= 0f ? 1f : _previousTimeScale;
                }
            }
        }

        private string T(string key, string fallback)
        {
            return localization != null ? localization.Get(key, fallback) : fallback;
        }

        private void EnsureLocalization()
        {
#if UNITY_EDITOR
            if (localization == null)
            {
                localization = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>("Assets/PrototypeGenerated/Config/TutorialLocalization.asset");
            }
#endif
            localization ??= Resources.Load<TutorialLocalizationAsset>("PrototypeGenerated/Config/TutorialLocalization");
            if (localization != null && ProjectSettingsState.HasSavedLocale)
            {
                localization.ActiveLocale = ProjectSettingsState.Locale;
            }
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
