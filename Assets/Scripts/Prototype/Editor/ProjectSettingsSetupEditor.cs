#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CupHeadClone.Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.PrototypeEditor
{
    public static class ProjectSettingsSetupEditor
    {
        private const string AudioCatalogPath = "Assets/PrototypeGenerated/Config/ProjectAudioCatalog.asset";

        [MenuItem("Tools/ParryShooter/Create Audio Catalog")]
        public static void CreateAudioCatalog()
        {
            Directory.CreateDirectory("Assets/PrototypeGenerated");
            Directory.CreateDirectory("Assets/PrototypeGenerated/Config");

            var asset = AssetDatabase.LoadAssetAtPath<ProjectAudioCatalog>(AudioCatalogPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ProjectAudioCatalog>();
                AssetDatabase.CreateAsset(asset, AudioCatalogPath);
            }

            asset.EnsureEntries(BuildDefaultEntries());
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }

        [MenuItem("Tools/ParryShooter/Setup Settings In Open Scene")]
        public static void SetupSettingsInOpenScene()
        {
            TutorialLocalizationAssetEditor.CreateOrUpdateDefaultAsset();
            CreateAudioCatalog();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Canvas Missing", "Open a scene with a Canvas first.", "OK");
                return;
            }

            var managersRoot = GameObject.Find("Managers") ?? new GameObject("Managers");
            var audioManager = Object.FindFirstObjectByType<AudioManager>();
            if (audioManager == null)
            {
                var audioObject = new GameObject("AudioManager");
                audioObject.transform.SetParent(managersRoot.transform, false);
                audioManager = audioObject.AddComponent<AudioManager>();
            }

            SetSerialized(audioManager, "catalog", AssetDatabase.LoadAssetAtPath<ProjectAudioCatalog>(AudioCatalogPath));
            SetSerialized(audioManager, "localization", AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(TutorialLocalizationAssetEditor.LocalizationAssetPath));

            var existingButton = canvas.transform.Find("SettingsButton");
            if (existingButton != null)
            {
                Object.DestroyImmediate(existingButton.gameObject);
            }

            var existingPanel = canvas.transform.Find("SettingsPanel");
            if (existingPanel != null)
            {
                Object.DestroyImmediate(existingPanel.gameObject);
            }

            var button = BuildSettingsButton(canvas.transform);
            var panel = BuildSettingsPanel(canvas.transform);

            SetSerialized(panel.controller, "localization", AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(TutorialLocalizationAssetEditor.LocalizationAssetPath));
            SetSerialized(panel.controller, "openButton", button.button);

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<AudioCueEntry> BuildDefaultEntries()
        {
            return new List<AudioCueEntry>
            {
                new(AudioCue.MenuMusic, 0.65f, 1f, 1f, true),
                new(AudioCue.BossRushMusic, 0.72f, 1f, 1f, true),
                new(AudioCue.UiClick, 0.7f, 0.98f, 1.02f),
                new(AudioCue.UiOpen, 0.75f, 0.98f, 1.02f),
                new(AudioCue.UiClose, 0.7f, 0.98f, 1.02f),
                new(AudioCue.PlayerShoot, 0.35f, 0.96f, 1.04f),
                new(AudioCue.PlayerShootStraight, 0.35f, 0.96f, 1.04f),
                new(AudioCue.PlayerShootSpreadshot, 0.4f, 0.96f, 1.04f),
                new(AudioCue.PlayerShootChaser, 0.4f, 0.98f, 1.02f),
                new(AudioCue.ParrySuccess, 0.9f, 0.94f, 1.06f),
                new(AudioCue.CounterHit, 0.82f, 0.96f, 1.04f),
                new(AudioCue.PlayerParrySpecialCounter, 0.82f, 0.96f, 1.04f),
                new(AudioCue.PlayerParrySpecialDefensiveRing, 0.82f, 0.98f, 1.02f),
                new(AudioCue.PlayerParrySpecialMolotov, 0.82f, 0.98f, 1.02f),
                new(AudioCue.MolotovFireZone, 0.72f, 0.98f, 1.02f),
                new(AudioCue.PlayerHit, 0.86f, 0.96f, 1.04f),
                new(AudioCue.PlayerDeath, 0.9f, 1f, 1f),
                new(AudioCue.SkillCast, 0.95f, 1f, 1f),
                new(AudioCue.PlayerSkillLaserCast, 0.95f, 1f, 1f),
                new(AudioCue.PlayerSkillGlobalRingCast, 0.95f, 1f, 1f),
                new(AudioCue.PlayerSkillStickyProjectileCast, 0.95f, 1f, 1f),
                new(AudioCue.BossBreak, 0.95f, 0.98f, 1.02f),
                new(AudioCue.LessonComplete, 0.82f, 0.98f, 1.02f),
                new(AudioCue.TutorialComplete, 0.92f, 1f, 1f)
            };
        }

        private static (SettingsPanelController controller, Button button) BuildSettingsButton(Transform parent)
        {
            var go = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(28f, -28f);
            rect.sizeDelta = new Vector2(180f, 68f);
            go.GetComponent<Image>().color = new Color(0.14f, 0.21f, 0.31f, 0.94f);
            var button = go.GetComponent<Button>();
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(go.transform, false);
            var text = label.GetComponent<TextMeshProUGUI>();
            text.text = "Settings";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 28f;
            text.color = PrototypeVisualUtility.TextPrimary;
            text.fontStyle = FontStyles.Bold;
            var textRect = label.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return (null, button);
        }

        private static (SettingsPanelController controller, RectTransform root) BuildSettingsPanel(Transform parent)
        {
            var root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            var controller = root.AddComponent<SettingsPanelController>();

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 760f);
            card.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.14f, 0.96f);

            var y = -80f;
            var title = CreateLabel("Title", card.transform, "Settings", 56f, new Vector2(0f, y), new Vector2(620f, 60f));
            y -= 110f;
            var masterLabel = CreateLabel("MasterLabel", card.transform, "Master Volume", 28f, new Vector2(-120f, y), new Vector2(360f, 40f), TextAlignmentOptions.Left);
            var masterSlider = CreateSlider("MasterSlider", card.transform, new Vector2(90f, y), new Vector2(360f, 32f));
            y -= 90f;
            var musicLabel = CreateLabel("MusicLabel", card.transform, "Music", 28f, new Vector2(-120f, y), new Vector2(360f, 40f), TextAlignmentOptions.Left);
            var musicSlider = CreateSlider("MusicSlider", card.transform, new Vector2(90f, y), new Vector2(360f, 32f));
            y -= 90f;
            var sfxLabel = CreateLabel("SfxLabel", card.transform, "SFX", 28f, new Vector2(-120f, y), new Vector2(360f, 40f), TextAlignmentOptions.Left);
            var sfxSlider = CreateSlider("SfxSlider", card.transform, new Vector2(90f, y), new Vector2(360f, 32f));
            y -= 90f;
            var languageLabel = CreateLabel("LanguageLabel", card.transform, "Language", 28f, new Vector2(-180f, y), new Vector2(240f, 40f), TextAlignmentOptions.Left);
            var languagePrevButton = CreateButton("LanguagePrevButton", card.transform, "<", new Vector2(-20f, y), new Vector2(72f, 72f));
            var languageValue = CreateLabel("LanguageValue", card.transform, "English", 28f, new Vector2(120f, y), new Vector2(220f, 48f));
            var languageNextButton = CreateButton("LanguageNextButton", card.transform, ">", new Vector2(260f, y), new Vector2(72f, 72f));

            var mainMenuButton = CreateButton("MainMenuButton", card.transform, "Main Menu", new Vector2(0f, -300f), new Vector2(320f, 72f));
            var closeButton = CreateButton("CloseButton", card.transform, "Close", new Vector2(0f, -392f), new Vector2(260f, 72f));

            SetSerialized(controller, "rootGroup", root.GetComponent<CanvasGroup>());
            SetSerialized(controller, "titleText", title);
            SetSerialized(controller, "masterLabel", masterLabel);
            SetSerialized(controller, "musicLabel", musicLabel);
            SetSerialized(controller, "sfxLabel", sfxLabel);
            SetSerialized(controller, "languageLabel", languageLabel);
            SetSerialized(controller, "languageValueText", languageValue);
            SetSerialized(controller, "masterSlider", masterSlider);
            SetSerialized(controller, "musicSlider", musicSlider);
            SetSerialized(controller, "sfxSlider", sfxSlider);
            SetSerialized(controller, "languagePrevButton", languagePrevButton);
            SetSerialized(controller, "languageNextButton", languageNextButton);
            SetSerialized(controller, "closeButton", closeButton);
            SetSerialized(controller, "mainMenuButton", mainMenuButton);

            return (controller, rootRect);
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text, float fontSize, Vector2 anchoredPosition, Vector2 size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = PrototypeVisualUtility.TextPrimary;
            label.fontStyle = FontStyles.Bold;
            return label;
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillRect = fillArea.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(0f, 0f);
            fillRect.offsetMax = new Vector2(-18f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.4f, 0.84f, 1f, 0.96f);
            fillImage.type = Image.Type.Sliced;
            var fillImageRect = fill.GetComponent<RectTransform>();
            fillImageRect.anchorMin = Vector2.zero;
            fillImageRect.anchorMax = Vector2.one;
            fillImageRect.offsetMin = Vector2.zero;
            fillImageRect.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(0f, 0f);
            handleAreaRect.offsetMax = new Vector2(0f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, size.y + 6f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private static Button CreateButton(string name, Transform parent, string text, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.75f, 0.95f, 1f, 0.98f);
            var label = CreateLabel("Label", go.transform, text, 28f, Vector2.zero, size);
            label.color = new Color(0.03f, 0.08f, 0.12f, 1f);
            label.fontStyle = FontStyles.Bold;
            return go.GetComponent<Button>();
        }

        private static void SetSerialized(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
