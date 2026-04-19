#if UNITY_EDITOR
using System.IO;
using CupHeadClone.Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CupHeadClone.PrototypeEditor
{
    public static class PrototypeBootstrapperEditor
    {
        private const string RootFolder = "Assets/PrototypeGenerated";
        private const string ConfigPath = RootFolder + "/Config/PrototypeCombatConfig.asset";
        private const string ScenePath = "Assets/Scenes/ParryBossRushPrototype.unity";
        [MenuItem("Tools/ParryShooter/Rebuild Prototype Scene")]
        public static void RebuildPrototypeScene()
        {
            EnsureFolders();
            var config = LoadOrCreateConfig();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ParryBossRushPrototype";

            var cameraRoot = new GameObject("CameraRoot").transform;
            cameraRoot.position = new Vector3(0f, 0f, -10f);
            var cameraShakePivot = new GameObject("CameraShakePivot").transform;
            cameraShakePivot.SetParent(cameraRoot, false);
            var camera = cameraShakePivot.gameObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.07f, 0.13f);
            camera.orthographic = true;

            var bootstrap = new GameObject("Bootstrap");
            var references = bootstrap.AddComponent<GameReferences>();
            var gameController = bootstrap.AddComponent<GameController>();

            var managers = new GameObject("Managers").transform;
            var gameplayRoot = new GameObject("GameplayRoot").transform;
            var playerRoot = new GameObject("PlayerRoot").transform;
            var bossRoot = new GameObject("BossRoot").transform;
            var projectileRoot = new GameObject("ProjectileRoot").transform;
            var vfxRoot = new GameObject("VFXRoot").transform;
            var pickupRoot = new GameObject("PickupRoot").transform;
            var boundsRoot = new GameObject("BoundsRoot").transform;
            var uiRoot = new GameObject("UI").transform;

            playerRoot.SetParent(gameplayRoot, false);
            bossRoot.SetParent(gameplayRoot, false);
            projectileRoot.SetParent(gameplayRoot, false);
            vfxRoot.SetParent(gameplayRoot, false);
            pickupRoot.SetParent(gameplayRoot, false);
            boundsRoot.SetParent(gameplayRoot, false);

            var playerInput = CreateManager<PlayerInputController>(managers, "PlayerInput");
            playerInput.gameObject.AddComponent<InputAdapter>();
            var rage = CreateManager<RageSystem>(managers, "RageSystem");
            var skill = CreateManager<SkillController>(managers, "SkillController");
            var bossPattern = CreateManager<BossPatternController>(managers, "BossPatternController");
            var bossRush = CreateManager<BossRushController>(managers, "BossRushController");
            var poolController = CreateManager<PrototypePoolController>(managers, "PrototypePoolController");
            var shake = CreateManager<ScreenShakeController>(managers, "ScreenShakeController");
            var vfx = CreateManager<VFXPoolController>(managers, "VFXPoolController");
            var parryFx = CreateManager<ParryFeedbackController>(managers, "ParryFeedbackController");

            var player = playerRoot.gameObject.AddComponent<PlayerController>();
            playerRoot.gameObject.AddComponent<PlayerView>();
            var playerCombat = playerRoot.gameObject.AddComponent<PlayerCombat>();
            var boss = bossRoot.gameObject.AddComponent<BossController>();
            bossRoot.gameObject.AddComponent<BossView>();
            var recovery = pickupRoot.gameObject.AddComponent<RecoveryCoreController>();

            BuildBounds(boundsRoot, config);
            var canvas = BuildCanvas(uiRoot);
            var hud = BuildHud(canvas.transform);
            var overlay = BuildOverlay(canvas.transform);
            var skillButton = BuildSkillButton(canvas.transform);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            AssignReferences(
                config,
                camera,
                canvas,
                bootstrap,
                references,
                gameController,
                cameraRoot,
                gameplayRoot,
                playerRoot,
                bossRoot,
                projectileRoot,
                vfxRoot,
                pickupRoot,
                boundsRoot,
                playerInput,
                player,
                playerCombat,
                boss,
                bossPattern,
                rage,
                skill,
                bossRush,
                recovery,
                poolController,
                shake,
                vfx,
                parryFx,
                hud.controller,
                overlay.controller,
                skillButton.controller);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Prototype Ready", "Scene and config were rebuilt.\nOpen Assets/Scenes/ParryBossRushPrototype.unity and press Play.", "OK");
        }

        [MenuItem("Tools/ParryShooter/Repair Boss Rush Start Overlay")]
        public static void RepairBossRushStartOverlay()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Boss Rush Missing", "Assets/Scenes/ParryBossRushPrototype.unity is missing.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var game = Object.FindFirstObjectByType<GameController>();
            var references = Object.FindFirstObjectByType<GameReferences>();
            if (game == null || references == null)
            {
                EditorUtility.DisplayDialog("Repair Failed", "Scene is missing GameController or GameReferences.", "OK");
                return;
            }

            var uiRoot = GameObject.Find("UI");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("UI");
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                canvas = BuildCanvas(uiRoot.transform);
            }

            DestroyChildIfExists(canvas.transform, "Overlay");
            var overlay = BuildOverlay(canvas.transform);
            EnsureEventSystem();

            SetSerialized(references, "uiCanvas", canvas);
            SetSerialized(game, "overlayController", overlay.controller);
            SetSerialized(game, "autoStartOnLoad", false);
            SetSerialized(overlay.controller, "passiveMode", false);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Boss Rush Overlay Repaired", "Start panel and Tutorial button were rebuilt in ParryBossRushPrototype.", "OK");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/PrototypeGenerated");
            Directory.CreateDirectory("Assets/PrototypeGenerated/Config");
        }

        private static PrototypeCombatConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(ConfigPath);
            if (config != null)
            {
                config.ResetToDefaults();
                EditorUtility.SetDirty(config);
                return config;
            }

            config = ScriptableObject.CreateInstance<PrototypeCombatConfig>();
            config.ResetToDefaults();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        private static T CreateManager<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
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

        private static void BuildBounds(Transform root, PrototypeCombatConfig config)
        {
            var bg = new GameObject("Background");
            bg.transform.SetParent(root, false);
            var sprite = bg.AddComponent<SpriteRenderer>();
            sprite.sprite = PrototypeVisualUtility.SquareSprite;
            sprite.color = new Color(0.02f, 0.06f, 0.12f, 1f);
            sprite.transform.localScale = new Vector3(config.logicalWidth / config.pixelsPerUnit, config.logicalHeight / config.pixelsPerUnit, 1f);
            sprite.sortingOrder = -50;

            for (var i = 1; i < 4; i++)
            {
                var lane = new GameObject($"LaneGuide_{i}");
                lane.transform.SetParent(root, false);
                var guide = lane.AddComponent<SpriteRenderer>();
                guide.sprite = PrototypeVisualUtility.SquareSprite;
                guide.color = new Color(1f, 1f, 1f, 0.05f);
                guide.transform.localScale = new Vector3(0.01f, 6.8f, 1f);
                var logical = new Vector2(config.logicalWidth / 4f * i, config.logicalHeight * 0.57f);
                lane.transform.position = new Vector3((logical.x - config.logicalWidth * 0.5f) / config.pixelsPerUnit, (config.logicalHeight * 0.5f - logical.y) / config.pixelsPerUnit, 0f);
                guide.sortingOrder = -40;
            }
        }

        private static Canvas BuildCanvas(Transform root)
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(root, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            return canvas;
        }

        private static (HUDController controller, RectTransform root) BuildHud(Transform parent)
        {
            var go = CreateUiObject("HUD", parent);
            var rootGroup = go.gameObject.AddComponent<CanvasGroup>();
            var controller = go.gameObject.AddComponent<HUDController>();

            var top = CreatePanel("TopPanel", go, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1040f, 180f), new Color(0.02f, 0.06f, 0.11f, 0.72f));
            var bottom = CreatePanel("BottomPanel", go, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1040f, 180f), new Color(0.02f, 0.06f, 0.11f, 0.72f));

            var bossLabel = CreateText("BossLabel", top, "BOSS", 40, TextAlignmentOptions.Left);
            bossLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            bossLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            bossLabel.rectTransform.offsetMin = new Vector2(24f, -56f);
            bossLabel.rectTransform.offsetMax = new Vector2(-24f, -8f);

            var hpFill = CreateBar("BossHp", top, new Vector2(24f, -96f), new Vector2(992f, 20f), new Color(1f, 0.55f, 0.67f, 1f));
            var poiseFill = CreateBar("BossPoise", top, new Vector2(24f, -132f), new Vector2(992f, 16f), new Color(0.78f, 0.49f, 1f, 1f));
            var breakLabel = CreateText("BreakLabel", top, string.Empty, 28, TextAlignmentOptions.Right);
            breakLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            breakLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            breakLabel.rectTransform.offsetMin = new Vector2(24f, 12f);
            breakLabel.rectTransform.offsetMax = new Vector2(-24f, 44f);

            var moveLabel = CreateText("MoveLabel", bottom, "Move: REST", 28, TextAlignmentOptions.Right);
            moveLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            moveLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            moveLabel.rectTransform.offsetMin = new Vector2(24f, -56f);
            moveLabel.rectTransform.offsetMax = new Vector2(-24f, -12f);

            var rageFill = CreateBar("Rage", bottom, new Vector2(180f, -120f), new Vector2(640f, 18f), new Color(1f, 0.78f, 0.4f, 1f));
            var hpPips = new Image[3];
            for (var i = 0; i < hpPips.Length; i++)
            {
                var pip = CreateImage($"Hp_{i}", bottom, new Color(0.56f, 0.95f, 0.78f, 1f));
                pip.sprite = PrototypeVisualUtility.CircleSprite;
                var rt = pip.rectTransform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.sizeDelta = new Vector2(24f, 24f);
                rt.anchoredPosition = new Vector2(70f + i * 34f, -108f);
                hpPips[i] = pip;
            }

            SetSerialized(controller, "bossLabel", bossLabel);
            SetSerialized(controller, "moveLabel", moveLabel);
            SetSerialized(controller, "breakLabel", breakLabel);
            SetSerialized(controller, "bossHpFill", hpFill);
            SetSerialized(controller, "bossPoiseFill", poiseFill);
            SetSerialized(controller, "rageFill", rageFill);
            SetSerialized(controller, "hpPips", hpPips);
            SetSerialized(controller, "rootGroup", rootGroup);
            return (controller, go);
        }

        private static (OverlayController controller, RectTransform root) BuildOverlay(Transform parent)
        {
            var go = CreateUiObject("Overlay", parent);
            var canvasGroup = go.gameObject.AddComponent<CanvasGroup>();
            var bg = go.gameObject.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.08f, 0.75f);
            Stretch(go);

            var controller = go.gameObject.AddComponent<OverlayController>();
            var card = CreatePanel("Card", go, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(900f, 760f), new Color(0.03f, 0.07f, 0.13f, 0.9f));
            var title = CreateText("Title", card, "Parry Boss Rush Prototype", 64, TextAlignmentOptions.Center);
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
            subtitle.rectTransform.sizeDelta = new Vector2(760f, 220f);

            var startButton = CreateButton("StartButton", card, "Start Rush", new Vector2(0f, -170f));
            var tutorialButton = CreateButton("TutorialButton", card, "Tutorial", new Vector2(0f, -275f));
            var restartButton = CreateButton("RestartButton", card, "Restart", new Vector2(0f, -380f));

            var banner = CreateUiObject("Banner", parent);
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.alpha = 0f;
            var panel = banner.gameObject.AddComponent<Image>();
            panel.color = new Color(0.05f, 0.09f, 0.16f, 0.8f);
            banner.anchorMin = new Vector2(0.5f, 1f);
            banner.anchorMax = new Vector2(0.5f, 1f);
            banner.pivot = new Vector2(0.5f, 1f);
            banner.anchoredPosition = new Vector2(0f, -160f);
            banner.sizeDelta = new Vector2(600f, 120f);
            var bannerTitle = CreateText("BannerTitle", banner, "BOSS 1", 34, TextAlignmentOptions.Center);
            bannerTitle.rectTransform.anchoredPosition = new Vector2(0f, 16f);
            bannerTitle.rectTransform.sizeDelta = new Vector2(560f, 50f);
            var bannerSubtitle = CreateText("BannerSubtitle", banner, string.Empty, 24, TextAlignmentOptions.Center);
            bannerSubtitle.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            bannerSubtitle.rectTransform.sizeDelta = new Vector2(560f, 40f);

            SetSerialized(controller, "canvasGroup", canvasGroup);
            SetSerialized(controller, "titleText", title);
            SetSerialized(controller, "subtitleText", subtitle);
            SetSerialized(controller, "bannerTitle", bannerTitle);
            SetSerialized(controller, "bannerSubtitle", bannerSubtitle);
            SetSerialized(controller, "bannerGroup", bannerGroup);
            SetSerialized(controller, "startButton", startButton);
            SetSerialized(controller, "tutorialButton", tutorialButton);
            SetSerialized(controller, "restartButton", restartButton);
            return (controller, go);
        }

        private static (SkillButtonController controller, RectTransform root) BuildSkillButton(Transform parent)
        {
            var go = CreateUiObject("SkillButton", parent);
            var image = go.gameObject.AddComponent<Image>();
            image.sprite = PrototypeVisualUtility.CircleSprite;
            image.color = new Color(1f, 0.76f, 0.35f, 1f);
            var button = go.gameObject.AddComponent<Button>();
            var group = go.gameObject.AddComponent<CanvasGroup>();
            go.anchorMin = new Vector2(1f, 0f);
            go.anchorMax = new Vector2(1f, 0f);
            go.pivot = new Vector2(1f, 0f);
            go.anchoredPosition = new Vector2(-120f, 120f);
            go.sizeDelta = new Vector2(180f, 180f);
            var label = CreateText("Label", go, "SKILL", 30, TextAlignmentOptions.Center);
            label.rectTransform.sizeDelta = new Vector2(150f, 100f);
            var controller = go.gameObject.AddComponent<SkillButtonController>();
            SetSerialized(controller, "button", button);
            SetSerialized(controller, "label", label);
            SetSerialized(controller, "canvasGroup", group);
            return (controller, go);
        }

        private static void AssignReferences(
            PrototypeCombatConfig config,
            Camera camera,
            Canvas canvas,
            GameObject bootstrap,
            GameReferences references,
            GameController gameController,
            Transform cameraRoot,
            Transform gameplayRoot,
            Transform playerRoot,
            Transform bossRoot,
            Transform projectileRoot,
            Transform vfxRoot,
            Transform pickupRoot,
            Transform boundsRoot,
            PlayerInputController playerInput,
            PlayerController player,
            PlayerCombat playerCombat,
            BossController boss,
            BossPatternController bossPattern,
            RageSystem rage,
            SkillController skill,
            BossRushController bossRush,
            RecoveryCoreController recovery,
            PrototypePoolController poolController,
            ScreenShakeController shake,
            VFXPoolController vfx,
            ParryFeedbackController parryFx,
            HUDController hud,
            OverlayController overlay,
            SkillButtonController skillButton)
        {
            SetSerialized(references, "config", config);
            SetSerialized(references, "gameplayCamera", camera);
            SetSerialized(references, "uiCanvas", canvas);
            SetSerialized(references, "gameplayRoot", gameplayRoot);
            SetSerialized(references, "playerRoot", playerRoot);
            SetSerialized(references, "bossRoot", bossRoot);
            SetSerialized(references, "projectileRoot", projectileRoot);
            SetSerialized(references, "vfxRoot", vfxRoot);
            SetSerialized(references, "pickupRoot", pickupRoot);
            SetSerialized(references, "boundsRoot", boundsRoot);

            SetSerialized(gameController, "config", config);
            SetSerialized(gameController, "references", references);
            SetSerialized(gameController, "playerInput", playerInput);
            SetSerialized(gameController, "player", player);
            SetSerialized(gameController, "playerCombat", playerCombat);
            SetSerialized(gameController, "boss", boss);
            SetSerialized(gameController, "bossPatternController", bossPattern);
            SetSerialized(gameController, "rageSystem", rage);
            SetSerialized(gameController, "skillController", skill);
            SetSerialized(gameController, "bossRushController", bossRush);
            SetSerialized(gameController, "recoveryCoreController", recovery);
            SetSerialized(gameController, "poolController", poolController);
            SetSerialized(gameController, "screenShakeController", shake);
            SetSerialized(gameController, "vfxPoolController", vfx);
            SetSerialized(gameController, "parryFeedbackController", parryFx);
            SetSerialized(gameController, "hudController", hud);
            SetSerialized(gameController, "overlayController", overlay);
            SetSerialized(gameController, "skillButtonController", skillButton);
            SetSerialized(shake, "shakeTarget", cameraRoot.childCount > 0 ? cameraRoot.GetChild(0) : cameraRoot);
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void DestroyChildIfExists(Transform parent, string name)
        {
            if (parent == null)
            {
                return;
            }

            var child = parent.Find(name);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
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
            label.color = new Color(0.91f, 0.95f, 1f, 1f);
            return label;
        }

        private static Image CreateBar(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, Color fillColor)
        {
            var bg = CreateUiObject(name + "_BG", parent);
            bg.anchorMin = new Vector2(0f, 1f);
            bg.anchorMax = new Vector2(0f, 1f);
            bg.anchoredPosition = anchoredPosition;
            bg.sizeDelta = size;
            var bgImage = bg.gameObject.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.12f);

            var fill = CreateUiObject(name + "_Fill", bg);
            Stretch(fill);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;
            return fillImage;
        }

        private static Image CreateImage(string name, RectTransform parent, Color color)
        {
            var rt = CreateUiObject(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button CreateButton(string name, RectTransform parent, string labelText, Vector2 anchoredPosition)
        {
            var rt = CreateUiObject(name, parent);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(320f, 84f);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.38f, 0.82f, 1f, 1f);
            var button = rt.gameObject.AddComponent<Button>();
            var label = CreateText("Label", rt, labelText, 30, TextAlignmentOptions.Center);
            label.rectTransform.sizeDelta = rt.sizeDelta;
            return button;
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

        private static void SetSerialized(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
