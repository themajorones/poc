#if UNITY_EDITOR
using System.IO;
using CupHeadClone.Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CupHeadClone.PrototypeEditor
{
    public static class TutorialSceneBootstrapEditor
    {
        private const string BossRushScenePath = "Assets/Scenes/ParryBossRushPrototype.unity";
        private const string TutorialScenePath = "Assets/Scenes/Tutorial.unity";
        private const string BaseConfigPath = "Assets/PrototypeGenerated/Config/PrototypeCombatConfig.asset";
        private const string TutorialConfigPath = "Assets/PrototypeGenerated/Config/TutorialCombatConfig.asset";

        [MenuItem("Tools/ParryShooter/Repair Tutorial Scene")]
        public static void RepairTutorialScene()
        {
            BuildTutorialSceneInternal(true);
        }

        private static void BuildTutorialSceneInternal(bool showDialog)
        {
            EnsureTutorialConfig();
            TutorialLocalizationAssetEditor.CreateOrUpdateDefaultAsset();
            if (!File.Exists(BossRushScenePath))
            {
                EditorUtility.DisplayDialog("Boss Rush Missing", "ParryBossRushPrototype.unity is missing. Rebuild the prototype scene first.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath) != null)
            {
                AssetDatabase.DeleteAsset(TutorialScenePath);
            }

            AssetDatabase.CopyAsset(BossRushScenePath, TutorialScenePath);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(TutorialScenePath, OpenSceneMode.Single);
            var game = Object.FindFirstObjectByType<GameController>();
            var references = Object.FindFirstObjectByType<GameReferences>();
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var baseHud = Object.FindFirstObjectByType<HUDController>();
            var baseOverlay = Object.FindFirstObjectByType<OverlayController>();
            if (game == null || references == null || canvas == null || baseHud == null || baseOverlay == null)
            {
                EditorUtility.DisplayDialog("Tutorial Build Failed", "Copied scene is missing GameController, GameReferences, Canvas, HUDController or OverlayController.", "OK");
                return;
            }

            var tutorialConfig = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(TutorialConfigPath);
            var localization = AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(TutorialLocalizationAssetEditor.LocalizationAssetPath);
            SetSerialized(game, "config", tutorialConfig);
            SetSerialized(references, "config", tutorialConfig);
            SetSerialized(game, "gameplayMode", 1);
            SetSerialized(game, "autoStartOnLoad", false);

            DestroyExistingUi(canvas.transform, "HUD");
            DestroyExistingUi(canvas.transform, "Overlay");
            DestroyExistingUi(canvas.transform, "SkillButton");
            DestroyExistingUi(canvas.transform, "TutorialHUD");
            DestroyExistingUi(canvas.transform, "TutorialOverlay");

            var passiveHudBuild = BuildPassiveHud(canvas.transform);
            var passiveOverlayBuild = BuildPassiveOverlay(canvas.transform);
            var skillBuild = BuildActiveSkillButton(canvas.transform);
            var hudBuild = BuildTutorialHud(canvas.transform);
            var overlayBuild = BuildTutorialOverlay(canvas.transform);

            var controllersRoot = GetOrCreateRoot("TutorialSceneControllers");
            var tutorialGame = GetOrAddComponent<TutorialGameController>(controllersRoot.gameObject);
            var breakControllerObject = GetOrCreateChild(references.GameplayRoot != null ? references.GameplayRoot : controllersRoot.transform, "TutorialBreakLesson");
            var breakController = GetOrAddComponent<TutorialBreakLessonController>(breakControllerObject.gameObject);
            var runtimeRoot = GetOrCreateChild(references.GameplayRoot != null ? references.GameplayRoot : controllersRoot.transform, "TutorialRuntime");

            SetSerialized(tutorialGame, "gameController", game);
            SetSerialized(tutorialGame, "overlayController", overlayBuild.controller);
            SetSerialized(tutorialGame, "hudController", hudBuild.controller);
            SetSerialized(tutorialGame, "breakLessonController", breakController);
            SetSerialized(tutorialGame, "runtimeRoot", runtimeRoot);
            SetSerialized(tutorialGame, "localization", localization);
            SetSerialized(hudBuild.controller, "localization", localization);
            SetSerialized(game, "hudController", passiveHudBuild.controller);
            SetSerialized(game, "overlayController", passiveOverlayBuild.controller);
            SetSerialized(game, "skillButtonController", skillBuild.controller);
            SetSerialized(game, "tutorialController", tutorialGame);

            EditorSceneManager.SaveScene(scene, TutorialScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Tutorial Scene Ready",
                    "Assets/Scenes/Tutorial.unity was rebuilt and wired.\nOpen that scene and press Play.",
                    "OK");
            }
        }

        private static void EnsureTutorialConfig()
        {
            var baseConfig = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(BaseConfigPath);
            if (baseConfig == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(TutorialConfigPath) ?? "Assets/PrototypeGenerated/Config");

            var tutorialConfig = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(TutorialConfigPath);
            if (tutorialConfig == null)
            {
                tutorialConfig = ScriptableObject.CreateInstance<PrototypeCombatConfig>();
                AssetDatabase.CreateAsset(tutorialConfig, TutorialConfigPath);
            }

            EditorUtility.CopySerialized(baseConfig, tutorialConfig);
            tutorialConfig.player.maxHp = 1;
            tutorialConfig.player.parryOuterRadius = 29f;
            tutorialConfig.parry.successInvuln = 0.32f;
            tutorialConfig.parry.rageGain = 0f;
            tutorialConfig.parry.counterHitRageGain = 0f;
            tutorialConfig.skill.duration = 0.95f;
            tutorialConfig.skill.laserDps = 220f;
            tutorialConfig.boss.weakZoneRadius = 44f;
            tutorialConfig.boss.weakZoneRagePerSecond = TutorialLessonDefinitions.BreakWeakZoneRagePerSecond;
            tutorialConfig.autoShot.damage = 1.8f;
            EditorUtility.SetDirty(tutorialConfig);
        }

        private static (TutorialHUDController controller, RectTransform root) BuildTutorialHud(Transform canvasRoot)
        {
            DestroyExistingUi(canvasRoot, "TutorialHUD");
            var root = CreateUiObject("TutorialHUD", canvasRoot);
            Stretch(root);
            var controller = root.gameObject.AddComponent<TutorialHUDController>();
            var rootGroup = root.gameObject.AddComponent<CanvasGroup>();

            var top = CreatePanel("TopPanel", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(1000f, 210f), PrototypeVisualUtility.Panel);
            var bottom = CreatePanel("BottomPanel", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 78f), new Vector2(1000f, 170f), PrototypeVisualUtility.Panel.WithAlpha(0.86f));
            var footer = CreatePanel("FooterHint", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 212f), new Vector2(980f, 58f), new Color(0.03f, 0.08f, 0.14f, 0.72f));

            var lessonTag = CreateText("LessonTag", top, "TUTORIAL", 30, TextAlignmentOptions.TopLeft);
            lessonTag.rectTransform.anchorMin = new Vector2(0f, 1f);
            lessonTag.rectTransform.anchorMax = new Vector2(1f, 1f);
            lessonTag.rectTransform.offsetMin = new Vector2(28f, -52f);
            lessonTag.rectTransform.offsetMax = new Vector2(-28f, -12f);

            var objective = CreateText("Objective", top, string.Empty, 25, TextAlignmentOptions.TopLeft);
            objective.rectTransform.anchorMin = new Vector2(0f, 1f);
            objective.rectTransform.anchorMax = new Vector2(1f, 1f);
            objective.rectTransform.offsetMin = new Vector2(28f, -104f);
            objective.rectTransform.offsetMax = new Vector2(-28f, -44f);

            var hint = CreateText("Hint", top, string.Empty, 23, TextAlignmentOptions.TopLeft);
            hint.rectTransform.anchorMin = new Vector2(0f, 1f);
            hint.rectTransform.anchorMax = new Vector2(1f, 1f);
            hint.rectTransform.offsetMin = new Vector2(28f, -154f);
            hint.rectTransform.offsetMax = new Vector2(-28f, -94f);

            var progress = CreateText("Progress", top, string.Empty, 24, TextAlignmentOptions.TopLeft);
            progress.rectTransform.anchorMin = new Vector2(0f, 1f);
            progress.rectTransform.anchorMax = new Vector2(1f, 1f);
            progress.rectTransform.offsetMin = new Vector2(28f, -194f);
            progress.rectTransform.offsetMax = new Vector2(-28f, -134f);

            var hpRule = CreateText("HpRule", bottom, string.Empty, 24, TextAlignmentOptions.Left);
            hpRule.rectTransform.anchorMin = new Vector2(0f, 1f);
            hpRule.rectTransform.anchorMax = new Vector2(1f, 1f);
            hpRule.rectTransform.offsetMin = new Vector2(28f, -62f);
            hpRule.rectTransform.offsetMax = new Vector2(-28f, -18f);

            var rageBack = CreatePanel("RageBack", bottom, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(132f, -118f), new Vector2(700f, 18f), new Color(1f, 1f, 1f, 0.12f));
            var rageFill = CreateUiObject("RageFill", rageBack);
            Stretch(rageFill);
            var rageFillImage = rageFill.gameObject.AddComponent<Image>();
            rageFillImage.type = Image.Type.Filled;
            rageFillImage.fillMethod = Image.FillMethod.Horizontal;
            rageFillImage.fillAmount = 0f;
            rageFillImage.color = PrototypeVisualUtility.CounterGold;

            var rageLabel = CreateText("RageLabel", bottom, string.Empty, 24, TextAlignmentOptions.Right);
            rageLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            rageLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            rageLabel.rectTransform.offsetMin = new Vector2(28f, -140f);
            rageLabel.rectTransform.offsetMax = new Vector2(-28f, -96f);

            var footerText = CreateText("FooterText", footer, string.Empty, 20, TextAlignmentOptions.Center);
            Stretch(footerText.rectTransform);

            var banner = CreatePanel("Banner", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(620f, 120f), new Color(0.03f, 0.08f, 0.14f, 0.88f));
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            var bannerTitle = CreateText("BannerTitle", banner, "Tutorial", 34, TextAlignmentOptions.Center);
            bannerTitle.rectTransform.anchoredPosition = new Vector2(0f, 18f);
            bannerTitle.rectTransform.sizeDelta = new Vector2(560f, 44f);
            var bannerSubtitle = CreateText("BannerSubtitle", banner, string.Empty, 22, TextAlignmentOptions.Center);
            bannerSubtitle.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            bannerSubtitle.rectTransform.sizeDelta = new Vector2(560f, 40f);

            SetSerialized(controller, "rootGroup", rootGroup);
            SetSerialized(controller, "lessonTagText", lessonTag);
            SetSerialized(controller, "objectiveText", objective);
            SetSerialized(controller, "hintText", hint);
            SetSerialized(controller, "progressText", progress);
            SetSerialized(controller, "hpRuleText", hpRule);
            SetSerialized(controller, "rageLabelText", rageLabel);
            SetSerialized(controller, "footerHintText", footerText);
            SetSerialized(controller, "rageFill", rageFillImage);
            SetSerialized(controller, "bannerGroup", bannerGroup);
            SetSerialized(controller, "bannerTitle", bannerTitle);
            SetSerialized(controller, "bannerSubtitle", bannerSubtitle);
            return (controller, root);
        }

        private static (HUDController controller, RectTransform root) BuildPassiveHud(Transform canvasRoot)
        {
            var root = CreateUiObject("HUD", canvasRoot);
            Stretch(root);
            var rootGroup = root.gameObject.AddComponent<CanvasGroup>();
            var controller = root.gameObject.AddComponent<HUDController>();
            SetSerialized(controller, "passiveMode", true);
            SetSerialized(controller, "rootGroup", rootGroup);
            return (controller, root);
        }

        private static (OverlayController controller, RectTransform root) BuildPassiveOverlay(Transform canvasRoot)
        {
            var root = CreateUiObject("Overlay", canvasRoot);
            Stretch(root);
            var rootGroup = root.gameObject.AddComponent<CanvasGroup>();
            var controller = root.gameObject.AddComponent<OverlayController>();
            SetSerialized(controller, "passiveMode", true);
            SetSerialized(controller, "canvasGroup", rootGroup);
            return (controller, root);
        }

        private static (SkillButtonController controller, RectTransform root) BuildActiveSkillButton(Transform canvasRoot)
        {
            var root = CreateUiObject("SkillButton", canvasRoot);
            root.anchorMin = new Vector2(1f, 0f);
            root.anchorMax = new Vector2(1f, 0f);
            root.pivot = new Vector2(1f, 0f);
            root.anchoredPosition = new Vector2(-120f, 120f);
            root.sizeDelta = new Vector2(180f, 180f);

            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 0.72f, 0.32f, 0.08f);
            var button = root.gameObject.AddComponent<Button>();
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var label = CreateText("Label", root, "SKILL", 30, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);

            var controller = root.gameObject.AddComponent<SkillButtonController>();
            SetSerialized(controller, "button", button);
            SetSerialized(controller, "label", label);
            SetSerialized(controller, "canvasGroup", group);
            return (controller, root);
        }

        private static (TutorialOverlayController controller, RectTransform root) BuildTutorialOverlay(Transform canvasRoot)
        {
            DestroyExistingUi(canvasRoot, "TutorialOverlay");
            var root = CreateUiObject("TutorialOverlay", canvasRoot);
            Stretch(root);
            var background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.02f, 0.04f, 0.08f, 0.76f);
            var rootGroup = root.gameObject.AddComponent<CanvasGroup>();
            var controller = root.gameObject.AddComponent<TutorialOverlayController>();

            var card = CreatePanel("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 760f), new Color(0.04f, 0.08f, 0.14f, 0.94f));
            var title = CreateText("Title", card, string.Empty, 62, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -90f);
            title.rectTransform.sizeDelta = new Vector2(760f, 100f);

            var subtitle = CreateText("Subtitle", card, string.Empty, 30, TextAlignmentOptions.Center);
            subtitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -210f);
            subtitle.rectTransform.sizeDelta = new Vector2(760f, 180f);

            var tips = CreateText("Tips", card, string.Empty, 26, TextAlignmentOptions.TopLeft);
            tips.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            tips.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            tips.rectTransform.pivot = new Vector2(0.5f, 1f);
            tips.rectTransform.anchoredPosition = new Vector2(0f, -380f);
            tips.rectTransform.sizeDelta = new Vector2(760f, 220f);

            var primaryButton = CreateButton("PrimaryButton", card, "Continue", new Vector2(0f, -250f));
            var secondaryButton = CreateButton("SecondaryButton", card, "Secondary", new Vector2(0f, -360f));

            SetSerialized(controller, "rootGroup", rootGroup);
            SetSerialized(controller, "cardRoot", card);
            SetSerialized(controller, "titleText", title);
            SetSerialized(controller, "subtitleText", subtitle);
            SetSerialized(controller, "tipsText", tips);
            SetSerialized(controller, "primaryButton", primaryButton);
            SetSerialized(controller, "secondaryButton", secondaryButton);
            return (controller, root);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            var existing = GameObject.Find(name);
            return existing != null ? existing : new GameObject(name);
        }

        private static Transform GetOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var created = new GameObject(name).transform;
            created.SetParent(parent, false);
            return created;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            return existing != null ? existing : target.AddComponent<T>();
        }

        private static void DestroyExistingUi(Transform parent, string objectName)
        {
            var existing = parent.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var rt = CreateUiObject(name, parent);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return rt;
        }

        private static TMP_Text CreateText(string name, RectTransform parent, string text, float size, TextAlignmentOptions alignment)
        {
            var rt = CreateUiObject(name, parent);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = PrototypeVisualUtility.TextPrimary;
            return label;
        }

        private static Button CreateButton(string name, RectTransform parent, string labelText, Vector2 anchoredPosition)
        {
            var rt = CreateUiObject(name, parent);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(340f, 84f);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.75f, 0.95f, 1f, 0.98f);
            var button = rt.gameObject.AddComponent<Button>();
            var label = CreateText("Label", rt, labelText, 30, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetSerialized(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerialized(Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerialized(Object target, string propertyName, int enumValueIndex)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).enumValueIndex = enumValueIndex;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
