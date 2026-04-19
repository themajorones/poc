#if UNITY_EDITOR
using CupHeadClone.Prototype;
using DarkTonic.PoolBoss;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CupHeadClone.PrototypeEditor
{
    public static class PoolBossTemplateAuthoringEditor
    {
        private const string RingConfigFolder = "Assets/Resources/PrototypeGenerated/Config";
        private const string RingPresetFolder = RingConfigFolder + "/Rings";
        private const string RingLibraryPath = RingConfigFolder + "/RingVisualPresetLibrary.asset";
        private static readonly Color DefensiveEdge = new(0.78f, 1f, 0.98f, 1f);
        private static readonly Color DefensiveBody = new(0.5f, 0.95f, 1f, 1f);
        private static readonly Color DefensiveGlow = new(0.18f, 0.82f, 1f, 1f);
        private static readonly Color DefensiveTrail = new(0.62f, 0.98f, 1f, 1f);
        private static readonly Color GlobalEdge = new(0.98f, 0.8f, 1f, 1f);
        private static readonly Color GlobalBody = new(0.88f, 0.56f, 1f, 1f);
        private static readonly Color GlobalGlow = new(0.52f, 0.18f, 1f, 1f);
        private static readonly Color GlobalTrail = new(0.82f, 0.56f, 1f, 1f);
        private static readonly Color ShockwaveEdge = new(0.97f, 0.74f, 1f, 1f);
        private static readonly Color ShockwaveBody = new(0.86f, 0.5f, 1f, 1f);
        private static readonly Color ShockwaveGlow = new(0.45f, 0.12f, 1f, 1f);
        private static readonly Color ShockwaveTrail = new(0.75f, 0.43f, 1f, 1f);
        private static readonly Color ImpactEdge = new(1f, 0.92f, 0.98f, 1f);
        private static readonly Color ImpactBody = new(1f, 0.62f, 0.9f, 1f);
        private static readonly Color ImpactGlow = new(1f, 0.28f, 0.74f, 1f);
        private static readonly Color ImpactTrail = new(1f, 0.72f, 0.94f, 1f);
        private static readonly Color WeakZoneEdge = new(1f, 0.94f, 0.58f, 1f);
        private static readonly Color WeakZoneBody = new(1f, 0.82f, 0.2f, 1f);
        private static readonly Color WeakZoneGlow = new(1f, 0.6f, 0.08f, 1f);
        private static readonly Color WeakZoneTrail = new(1f, 0.9f, 0.38f, 1f);

        [MenuItem("Tools/ParryShooter/Setup Player Field Ring Templates")]
        public static void SetupPlayerFieldRingTemplates()
        {
            var poolBoss = Object.FindFirstObjectByType<PoolBoss>();
            if (poolBoss == null)
            {
                EditorUtility.DisplayDialog("PoolBoss Missing", "Create or open a scene that already has a PoolBoss object first.", "OK");
                return;
            }

            var root = poolBoss.transform;
            var ringTemplate = GetOrCreateChild(root, "GameplayRing");
            ConfigureRingTemplate(ringTemplate, 16);
            EnsurePoolItem(poolBoss, ringTemplate, 8, 24);
            var fireZoneTemplate = GetOrCreateChild(root, "MolotovFireZone");
            ConfigureMolotovFireZoneTemplate(fireZoneTemplate, 15);
            EnsurePoolItem(poolBoss, fireZoneTemplate, 10, 40);
            CreateOrUpdateRingPresetAssets();
            EditorUtility.SetDirty(poolBoss);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(poolBoss.gameObject.scene);
            }

            EditorUtility.DisplayDialog("Templates Ready", "GameplayRing and MolotovFireZone templates are ready and registered in PoolBoss.", "OK");
        }

        private static Transform GetOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void ConfigureRingTemplate(Transform template, int sortingOrder)
        {
            var lineRenderer = template.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = template.gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 48;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.numCapVertices = 2;
            lineRenderer.numCornerVertices = 2;
            lineRenderer.sortingOrder = sortingOrder;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.startColor = new Color(1f, 1f, 1f, 0f);
            lineRenderer.endColor = new Color(1f, 1f, 1f, 0f);

            EnsureLayer(template, "Glow", sortingOrder);
            EnsureLayer(template, "Trail", sortingOrder + 1);
            EnsureLayer(template, "Body", sortingOrder + 2);
            EnsureLayer(template, "Edge", sortingOrder + 3);
            for (var i = 0; i < 4; i++)
            {
                EnsureSpark(template, $"Spark_{i}", sortingOrder + 4);
            }

            if (template.GetComponent<PlayerFieldRingVisual>() == null)
            {
                template.gameObject.AddComponent<PlayerFieldRingVisual>();
            }

            var poolableInfo = template.GetComponent<PoolableInfo>();
            if (poolableInfo == null)
            {
                poolableInfo = template.gameObject.AddComponent<PoolableInfo>();
            }

            poolableInfo.AllowInScenePoolables = false;
            template.gameObject.SetActive(false);
            EditorUtility.SetDirty(template.gameObject);
        }

        private static void ConfigureMolotovFireZoneTemplate(Transform template, int sortingOrder)
        {
            var renderer = template.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = template.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = PrototypeVisualUtility.CircleSprite;
            renderer.color = new Color(1f, 0.44f, 0.08f, 0f);
            renderer.sortingOrder = sortingOrder;

            if (template.GetComponent<MolotovFireZone>() == null)
            {
                template.gameObject.AddComponent<MolotovFireZone>();
            }

            var poolableInfo = template.GetComponent<PoolableInfo>();
            if (poolableInfo == null)
            {
                poolableInfo = template.gameObject.AddComponent<PoolableInfo>();
            }

            poolableInfo.AllowInScenePoolables = false;
            template.gameObject.SetActive(false);
            EditorUtility.SetDirty(template.gameObject);
        }

        private static void EnsurePoolItem(PoolBoss poolBoss, Transform template, int preload, int hardLimit)
        {
            if (poolBoss == null || template == null)
            {
                return;
            }

            PoolBossItem existing = null;
            for (var i = 0; i < poolBoss.poolItems.Count; i++)
            {
                var item = poolBoss.poolItems[i];
                if (item != null && item.prefabTransform == template)
                {
                    existing = item;
                    break;
                }
            }

            if (existing == null)
            {
                existing = new PoolBossItem();
                poolBoss.poolItems.Add(existing);
            }

            existing.prefabSource = PoolBoss.PrefabSource.Prefab;
            existing.prefabTransform = template;
            existing.gameObject = template.gameObject;
            existing.instancesToPreload = Mathf.Max(1, preload);
            existing.allowInstantiateMore = true;
            existing.itemHardLimit = Mathf.Max(existing.instancesToPreload, hardLimit);
            existing.allowRecycle = false;
            existing.categoryName = PoolBoss.NoCategory;
            existing.isTemporary = false;
        }

        [MenuItem("Tools/ParryShooter/Create Or Update Ring Visual Presets")]
        public static void CreateOrUpdateRingPresetAssets()
        {
            Directory.CreateDirectory(RingConfigFolder);
            Directory.CreateDirectory(RingPresetFolder);

            var defensive = CreateOrUpdatePreset("DefensiveField", 1f, 0.5f, 0.11f, 0.18f, 1.18f, 2.35f, 0.48f, 5.8f, 1, 0.85f, 0.8f, 0.42f, 1.12f, 1.06f, DefensiveEdge, DefensiveBody, DefensiveGlow, DefensiveTrail);
            var global = CreateOrUpdatePreset("GlobalWave", 1f, 0.54f, 0.1f, 0.18f, 1.2f, 2.5f, 0.42f, 4.8f, 0, 0.62f, 1.08f, 0.68f, 1.08f, 1.03f, GlobalEdge, GlobalBody, GlobalGlow, GlobalTrail);
            var shockwave = CreateOrUpdatePreset("BossShockwave", 1f, 0.56f, 0.12f, 0.2f, 1.22f, 2.6f, 0.46f, 4.6f, 0, 0.92f, 0.92f, 0.7f, 1.1f, 1.05f, ShockwaveEdge, ShockwaveBody, ShockwaveGlow, ShockwaveTrail);
            var impact = CreateOrUpdatePreset("TransientImpact", 1f, 0.52f, 0.14f, 0.22f, 1.12f, 2.45f, 0.52f, 5.6f, 1, 1.2f, 0.72f, 0.34f, 1.1f, 1.14f, ImpactEdge, ImpactBody, ImpactGlow, ImpactTrail);
            var weakZone = CreateOrUpdatePreset("WeakZone", 0.88f, 0.18f, 0.12f, 0.08f, 1.08f, 1.6f, 0.38f, 4.2f, 2, 0.46f, 0.84f, 0.44f, 1.34f, 1f, WeakZoneEdge, WeakZoneBody, WeakZoneGlow, WeakZoneTrail);

            var library = AssetDatabase.LoadAssetAtPath<RingVisualPresetLibrary>(RingLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<RingVisualPresetLibrary>();
                AssetDatabase.CreateAsset(library, RingLibraryPath);
            }

            SetObjectReference(library, "defensiveField", defensive);
            SetObjectReference(library, "globalWave", global);
            SetObjectReference(library, "bossShockwave", shockwave);
            SetObjectReference(library, "transientImpact", impact);
            SetObjectReference(library, "weakZone", weakZone);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = library;
        }

        private static RingVisualPreset CreateOrUpdatePreset(
            string name,
            float edgeAlpha,
            float bodyAlpha,
            float glowAlpha,
            float trailAlpha,
            float thicknessScale,
            float glowWidthScale,
            float trailInsetScale,
            float pulseSpeed,
            int sparkCount,
            float sparkSpeed,
            float sparkScale,
            float sparkAlpha,
            float telegraphGlowBoost,
            float impactFlashBoost,
            Color edgeColor,
            Color bodyColor,
            Color glowColor,
            Color trailColor)
        {
            var path = $"{RingPresetFolder}/{name}.asset";
            var preset = AssetDatabase.LoadAssetAtPath<RingVisualPreset>(path);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<RingVisualPreset>();
                AssetDatabase.CreateAsset(preset, path);
            }

            SetFloat(preset, "edgeAlpha", edgeAlpha);
            SetFloat(preset, "bodyAlpha", bodyAlpha);
            SetFloat(preset, "glowAlpha", glowAlpha);
            SetFloat(preset, "trailAlpha", trailAlpha);
            SetFloat(preset, "thicknessScale", thicknessScale);
            SetFloat(preset, "glowWidthScale", glowWidthScale);
            SetFloat(preset, "trailInsetScale", trailInsetScale);
            SetFloat(preset, "pulseSpeed", pulseSpeed);
            SetInt(preset, "sparkCount", sparkCount);
            SetFloat(preset, "sparkSpeed", sparkSpeed);
            SetFloat(preset, "sparkScale", sparkScale);
            SetFloat(preset, "sparkAlpha", sparkAlpha);
            SetFloat(preset, "telegraphGlowBoost", telegraphGlowBoost);
            SetFloat(preset, "impactFlashBoost", impactFlashBoost);
            SetColor(preset, "edgeColor", edgeColor);
            SetColor(preset, "bodyColor", bodyColor);
            SetColor(preset, "glowColor", glowColor);
            SetColor(preset, "trailColor", trailColor);
            EditorUtility.SetDirty(preset);
            return preset;
        }

        private static void EnsureLayer(Transform parent, string name, int sortingOrder)
        {
            var child = GetOrCreateChild(parent, name);
            var renderer = child.GetComponent<LineRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<LineRenderer>();
            }

            renderer.loop = true;
            renderer.useWorldSpace = true;
            renderer.positionCount = 128;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 12;
            renderer.numCornerVertices = 12;
            renderer.sortingOrder = sortingOrder;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.startWidth = 0.1f;
            renderer.endWidth = 0.1f;
            renderer.startColor = new Color(1f, 1f, 1f, 0f);
            renderer.endColor = new Color(1f, 1f, 1f, 0f);
        }

        private static void EnsureSpark(Transform parent, string name, int sortingOrder)
        {
            var child = GetOrCreateChild(parent, name);
            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = PrototypeVisualUtility.ArcSprite;
            renderer.color = new Color(1f, 1f, 1f, 0f);
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetColor(Object target, string propertyName, Color value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).colorValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
