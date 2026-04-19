using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string LocalizationAssetPath = "Assets/PrototypeGenerated/Config/TutorialLocalization.asset";
        private const string LocalizationResourcePath = "PrototypeGenerated/Config/TutorialLocalization";
        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button tutorialButton;

        private Tween _cardTween;

        private void Awake()
        {
            EnsureLocalization();
            RefreshLocalizedText();

            startButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                SceneFlowController.LoadBossRush();
            });
            tutorialButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                SceneFlowController.LoadTutorial();
            });

            StyleButton(startButton, true);
            StyleButton(tutorialButton, false);
        }

        private void OnEnable()
        {
            LocalizationRuntime.LocaleChanged += RefreshLocalizedText;
            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.DOFade(1f, 0.22f).SetUpdate(true);
            }

            if (cardRoot != null)
            {
                cardRoot.localScale = Vector3.one * 0.96f;
                _cardTween?.Kill();
                _cardTween = cardRoot.DOScale(1f, 0.28f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        private void OnDestroy()
        {
            LocalizationRuntime.LocaleChanged -= RefreshLocalizedText;
            _cardTween?.Kill();
        }

        public void RefreshLocalizedText()
        {
            titleText.text = T("menu.title", "Parry Shooter");
            subtitleText.text = T("menu.subtitle", "Start vao boss rush. Tutorial vao scene huong dan rieng.");
            SetButtonLabel(startButton, T("menu.start", "Start"));
            SetButtonLabel(tutorialButton, T("menu.tutorial", "Tutorial"));
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
                localization = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(LocalizationAssetPath);
            }
#endif
            localization ??= Resources.Load<TutorialLocalizationAsset>(LocalizationResourcePath);
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

            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void StyleButton(Button button, bool primary)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = primary
                    ? new Color(0.75f, 0.95f, 1f, 0.98f)
                    : new Color(0.14f, 0.21f, 0.31f, 0.95f);
            }

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.fontStyle = FontStyles.Bold;
                label.color = primary
                    ? new Color(0.03f, 0.08f, 0.12f, 1f)
                    : PrototypeVisualUtility.TextPrimary;
            }
        }
    }
}
