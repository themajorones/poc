using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class OverlayController : MonoBehaviour
    {
        private const string LocalizationAssetPath = "Assets/PrototypeGenerated/Config/TutorialLocalization.asset";
        private const string LocalizationResourcePath = "PrototypeGenerated/Config/TutorialLocalization";
        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private bool passiveMode;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text bannerTitle;
        [SerializeField] private TMP_Text bannerSubtitle;
        [SerializeField] private CanvasGroup bannerGroup;
        [SerializeField] private Button startButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private RectTransform selectorRoot;
        [SerializeField] private Image selectorPreview;
        [SerializeField] private TMP_Text selectorName;
        [SerializeField] private Button previousCharacterButton;
        [SerializeField] private Button nextCharacterButton;

        private GameController _game;
        private float _bannerTimer;

        public void Initialize(GameController game)
        {
            _game = game;
            EnsureLocalization();
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            LocalizationRuntime.LocaleChanged += HandleLocaleChanged;
            if (passiveMode)
            {
                if (canvasGroup == null)
                {
                    canvasGroup = GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }

                RefreshState(_game.State);
                return;
            }

            EnsureTutorialButton();
            EnsureCharacterSelector();
            SetButtonLabel(startButton, T("bossrush.start", "Start Rush"));
            SetButtonLabel(tutorialButton, T("bossrush.tutorial", "Tutorial"));
            SetButtonLabel(restartButton, T("bossrush.restart", "Restart"));

            startButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                _game.StartRun();
            });
            tutorialButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                SceneFlowController.LoadTutorial();
            });
            restartButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                _game.RestartRun();
            });
            previousCharacterButton?.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                _game.SelectPreviousCharacter();
            });
            nextCharacterButton?.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                _game.SelectNextCharacter();
            });

            PolishLayout();
            _game.PlayerCharacterChanged -= HandlePlayerCharacterChanged;
            _game.PlayerCharacterChanged += HandlePlayerCharacterChanged;
            RefreshState(_game.State);
        }

        public void RefreshState(GameController.RunState state)
        {
            if (passiveMode)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                return;
            }

            var visible = state != GameController.RunState.Playing;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
            SetButtonLabel(startButton, T("bossrush.start", "Start Rush"));
            SetButtonLabel(tutorialButton, T("bossrush.tutorial", "Tutorial"));
            SetButtonLabel(restartButton, T("bossrush.restart", "Restart"));

            switch (state)
            {
                case GameController.RunState.Start:
                    titleText.text = T("bossrush.title.start", "Boss Rush");
                    subtitleText.text = T("bossrush.subtitle.start", "Bat dau boss rush hoac vao tutorial de hoc co che.");
                    startButton.gameObject.SetActive(true);
                    tutorialButton.gameObject.SetActive(true);
                    restartButton.gameObject.SetActive(false);
                    RefreshCharacterSelector(true);
                    break;
                case GameController.RunState.Win:
                    titleText.text = T("bossrush.title.win", "Boss Rush Cleared");
                    subtitleText.text = T("bossrush.subtitle.win", "Ban da ha tat ca boss. Bam Restart de choi lai.");
                    startButton.gameObject.SetActive(false);
                    tutorialButton.gameObject.SetActive(false);
                    restartButton.gameObject.SetActive(true);
                    RefreshCharacterSelector(false);
                    break;
                case GameController.RunState.Lose:
                    titleText.text = T("bossrush.title.lose", "Ship Destroyed");
                    subtitleText.text = T("bossrush.subtitle.lose", "Doc lane ky hon, uu tien phan don dan tim, roi dung skill khi Rage day.");
                    startButton.gameObject.SetActive(false);
                    tutorialButton.gameObject.SetActive(false);
                    restartButton.gameObject.SetActive(true);
                    RefreshCharacterSelector(false);
                    break;
            }
        }

        public void ShowBanner(string title, string subtitle)
        {
            if (passiveMode)
            {
                return;
            }

            bannerTitle.text = title;
            bannerSubtitle.text = subtitle;
            bannerGroup.alpha = 1f;
            bannerGroup.transform.localScale = Vector3.one;
            bannerGroup.DOKill();
            bannerGroup.transform.DOKill();
            bannerGroup.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 1);
            _bannerTimer = 1.6f;
        }

        private void PolishLayout()
        {
            var background = GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0.02f, 0.04f, 0.08f, 0.78f);
            }

            titleText.color = PrototypeVisualUtility.TextPrimary;
            titleText.fontStyle = FontStyles.Bold;
            subtitleText.color = PrototypeVisualUtility.TextMuted;
            bannerTitle.color = PrototypeVisualUtility.TextPrimary;
            bannerTitle.fontStyle = FontStyles.Bold;
            bannerSubtitle.color = PrototypeVisualUtility.TextMuted;

            StyleButton(startButton, true);
            StyleButton(tutorialButton, false);
            StyleButton(restartButton, false);
            StyleButton(previousCharacterButton, false);
            StyleButton(nextCharacterButton, false);

            bannerGroup.alpha = 0f;
            var bannerBg = bannerGroup.GetComponent<Image>();
            if (bannerBg != null)
            {
                bannerBg.color = PrototypeVisualUtility.Panel.WithAlpha(0.92f);
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

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = value;
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
                    ? new Color(0.404f, 0.835f, 1f, 0.96f)
                    : new Color(1f, 1f, 1f, 0.09f);
            }

            var text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.color = primary ? new Color(0.03f, 0.09f, 0.16f, 1f) : PrototypeVisualUtility.TextPrimary;
                text.fontStyle = FontStyles.Bold;
            }
        }

        private void EnsureTutorialButton()
        {
            if (tutorialButton != null || startButton == null)
            {
                return;
            }

            var source = restartButton != null ? restartButton : startButton;
            var tutorialObject = Instantiate(source.gameObject, source.transform.parent);
            tutorialObject.name = "TutorialButton";

            var tutorialRect = tutorialObject.GetComponent<RectTransform>();
            var sourceRect = source.GetComponent<RectTransform>();
            if (tutorialRect != null && sourceRect != null)
            {
                tutorialRect.anchorMin = sourceRect.anchorMin;
                tutorialRect.anchorMax = sourceRect.anchorMax;
                tutorialRect.pivot = sourceRect.pivot;
                tutorialRect.sizeDelta = sourceRect.sizeDelta;
                tutorialRect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(0f, -105f);
            }

            var label = tutorialObject.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = T("bossrush.tutorial", "Tutorial");
            }

            tutorialButton = tutorialObject.GetComponent<Button>();
        }

        private void EnsureCharacterSelector()
        {
            if (selectorRoot != null)
            {
                return;
            }

            var card = transform.Find("Card") as RectTransform;
            if (card == null)
            {
                return;
            }

            selectorRoot = new GameObject("CharacterSelector", typeof(RectTransform)).GetComponent<RectTransform>();
            selectorRoot.SetParent(card, false);
            selectorRoot.anchorMin = new Vector2(0.5f, 0.5f);
            selectorRoot.anchorMax = new Vector2(0.5f, 0.5f);
            selectorRoot.pivot = new Vector2(0.5f, 0.5f);
            selectorRoot.anchoredPosition = new Vector2(0f, -10f);
            selectorRoot.sizeDelta = new Vector2(560f, 250f);

            previousCharacterButton = CreateSelectorButton("PreviousCharacterButton", selectorRoot, "<", new Vector2(-220f, 0f));
            nextCharacterButton = CreateSelectorButton("NextCharacterButton", selectorRoot, ">", new Vector2(220f, 0f));

            var previewRoot = new GameObject("PreviewRoot", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            previewRoot.SetParent(selectorRoot, false);
            previewRoot.anchorMin = new Vector2(0.5f, 0.5f);
            previewRoot.anchorMax = new Vector2(0.5f, 0.5f);
            previewRoot.pivot = new Vector2(0.5f, 0.5f);
            previewRoot.anchoredPosition = new Vector2(0f, 8f);
            previewRoot.sizeDelta = new Vector2(220f, 180f);
            var previewBg = previewRoot.GetComponent<Image>();
            previewBg.color = new Color(1f, 1f, 1f, 0f);

            selectorPreview = new GameObject("Preview", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            selectorPreview.transform.SetParent(previewRoot, false);
            var previewRt = selectorPreview.rectTransform;
            previewRt.anchorMin = new Vector2(0.5f, 0.5f);
            previewRt.anchorMax = new Vector2(0.5f, 0.5f);
            previewRt.pivot = new Vector2(0.5f, 0.5f);
            previewRt.sizeDelta = new Vector2(120f, 120f);
            selectorPreview.preserveAspect = true;

            selectorName = new GameObject("CharacterName", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            selectorName.transform.SetParent(selectorRoot, false);
            var nameRt = selectorName.rectTransform;
            nameRt.anchorMin = new Vector2(0.5f, 0f);
            nameRt.anchorMax = new Vector2(0.5f, 0f);
            nameRt.pivot = new Vector2(0.5f, 0f);
            nameRt.anchoredPosition = new Vector2(0f, 0f);
            nameRt.sizeDelta = new Vector2(320f, 40f);
            selectorName.alignment = TextAlignmentOptions.Center;
            selectorName.fontSize = 30f;
            selectorName.color = PrototypeVisualUtility.TextPrimary;
            selectorName.fontStyle = FontStyles.Bold;
        }

        private void RefreshCharacterSelector(bool visible)
        {
            if (selectorRoot == null)
            {
                return;
            }

            var canSelect = visible && _game != null && _game.CanSelectCharacters;
            selectorRoot.gameObject.SetActive(canSelect);
            if (!canSelect)
            {
                return;
            }

            var selected = _game.SelectedCharacter;
            if (selectorName != null)
            {
                selectorName.text = selected != null ? selected.DisplayName : "Player";
            }

            if (selectorPreview != null)
            {
                var sprite = selected != null ? selected.PreviewSprite : null;
                if (sprite == null && selected != null && selected.PlayerPrefab != null)
                {
                    var authoring = selected.PlayerPrefab.GetComponent<PlayerAuthoring>();
                    sprite = authoring != null ? authoring.BodySprite : null;
                }

                selectorPreview.sprite = sprite;
                selectorPreview.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        }

        private static Button CreateSelectorButton(string name, RectTransform parent, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(72f, 72f);
            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.05f);
            var button = go.GetComponent<Button>();

            var text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            text.transform.SetParent(rect, false);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 40f;
            text.color = PrototypeVisualUtility.TextPrimary;
            text.fontStyle = FontStyles.Bold;
            text.text = label;
            return button;
        }

        private void Update()
        {
            if (_bannerTimer <= 0f)
            {
                return;
            }

            _bannerTimer -= Time.deltaTime;
            if (_bannerTimer <= 0f)
            {
                bannerGroup.DOFade(0f, 0.2f);
            }
        }

        private void OnDestroy()
        {
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            if (_game != null)
            {
                _game.PlayerCharacterChanged -= HandlePlayerCharacterChanged;
            }
        }

        private void HandleLocaleChanged()
        {
            RefreshState(_game != null ? _game.State : GameController.RunState.Start);
        }

        private void HandlePlayerCharacterChanged()
        {
            RefreshCharacterSelector(_game != null && _game.State == GameController.RunState.Start);
        }
    }
}
