using System.IO;
using UnityEditor;
using UnityEngine;
using CupHeadClone.Prototype;

namespace CupHeadClone.PrototypeEditor
{
    public static class PlayerLoadoutSetupEditor
    {
        private const string RootFolder = "Assets/PrototypeGenerated/Config/Player";

        [MenuItem("Tools/ParryShooter/Create Default Player Loadout")]
        public static void CreateDefaultPlayerLoadout()
        {
            EnsureFolder("Assets/PrototypeGenerated");
            EnsureFolder("Assets/PrototypeGenerated/Config");
            EnsureFolder(RootFolder);

            var shot = CreateOrLoad<PlayerShotDefinition>($"{RootFolder}/DefaultPlayerShot.asset");
            var counter = CreateOrLoad<PlayerCounterShotDefinition>($"{RootFolder}/DefaultPlayerCounterShot.asset");
            var skill = CreateOrLoad<PlayerSkillDefinition>($"{RootFolder}/DefaultPlayerSkill.asset");
            var parry = CreateOrLoad<PlayerParryEffectDefinition>($"{RootFolder}/DefaultPlayerParryEffect.asset");
            var loadout = CreateOrLoad<PlayerLoadoutDefinition>($"{RootFolder}/DefaultPlayerLoadout.asset");

            ConfigureDefaultShot(shot);
            ConfigureDefaultParrySpecial(counter);
            ConfigureDefaultSkill(skill);

            var so = new SerializedObject(loadout);
            so.FindProperty("primaryShot").objectReferenceValue = shot;
            so.FindProperty("counterShot").objectReferenceValue = counter;
            so.FindProperty("skill").objectReferenceValue = skill;
            so.FindProperty("parryEffect").objectReferenceValue = parry;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loadout);

            AssignLoadoutToConfigs(loadout);
            AssignLoadoutToPlayerPrefabs(loadout);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(loadout);
        }

        [MenuItem("Tools/ParryShooter/Create Spread Ring Variant Loadout")]
        public static void CreateSpreadRingVariantLoadout()
        {
            EnsureFolder("Assets/PrototypeGenerated");
            EnsureFolder("Assets/PrototypeGenerated/Config");
            EnsureFolder(RootFolder);

            var shot = CreateOrLoad<PlayerShotDefinition>($"{RootFolder}/Spreadshot.asset");
            var parryVisual = CreateOrLoad<PlayerParryEffectDefinition>($"{RootFolder}/DefaultPlayerParryEffect.asset");
            var parrySpecial = CreateOrLoad<PlayerCounterShotDefinition>($"{RootFolder}/DefensiveRingParrySpecial.asset");
            var skill = CreateOrLoad<PlayerSkillDefinition>($"{RootFolder}/ExpandingGlobalRing.asset");
            var loadout = CreateOrLoad<PlayerLoadoutDefinition>($"{RootFolder}/SpreadRingLoadout.asset");

            ConfigureSpreadshot(shot);
            ConfigureDefensiveRing(parrySpecial);
            ConfigureExpandingGlobalRing(skill);

            var loadoutSo = new SerializedObject(loadout);
            loadoutSo.FindProperty("primaryShot").objectReferenceValue = shot;
            loadoutSo.FindProperty("counterShot").objectReferenceValue = parrySpecial;
            loadoutSo.FindProperty("skill").objectReferenceValue = skill;
            loadoutSo.FindProperty("parryEffect").objectReferenceValue = parryVisual;
            loadoutSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(shot);
            EditorUtility.SetDirty(parryVisual);
            EditorUtility.SetDirty(parrySpecial);
            EditorUtility.SetDirty(skill);
            EditorUtility.SetDirty(loadout);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(loadout);
        }

        [MenuItem("Tools/ParryShooter/Create Chaser Molotov Loadout")]
        public static void CreateChaserMolotovLoadout()
        {
            EnsureFolder("Assets/PrototypeGenerated");
            EnsureFolder("Assets/PrototypeGenerated/Config");
            EnsureFolder(RootFolder);

            var shot = CreateOrLoad<PlayerShotDefinition>($"{RootFolder}/ChaserShot.asset");
            var parryVisual = CreateOrLoad<PlayerParryEffectDefinition>($"{RootFolder}/DefaultPlayerParryEffect.asset");
            var parrySpecial = CreateOrLoad<PlayerCounterShotDefinition>($"{RootFolder}/MolotovParrySpecial.asset");
            var skill = CreateOrLoad<PlayerSkillDefinition>($"{RootFolder}/StickyProjectileSkill.asset");
            var loadout = CreateOrLoad<PlayerLoadoutDefinition>($"{RootFolder}/ChaserMolotovLoadout.asset");

            ConfigureChaserShot(shot);
            ConfigureMolotov(parrySpecial);
            ConfigureStickyProjectileSkill(skill);

            var loadoutSo = new SerializedObject(loadout);
            loadoutSo.FindProperty("primaryShot").objectReferenceValue = shot;
            loadoutSo.FindProperty("counterShot").objectReferenceValue = parrySpecial;
            loadoutSo.FindProperty("skill").objectReferenceValue = skill;
            loadoutSo.FindProperty("parryEffect").objectReferenceValue = parryVisual;
            loadoutSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(shot);
            EditorUtility.SetDirty(parryVisual);
            EditorUtility.SetDirty(parrySpecial);
            EditorUtility.SetDirty(skill);
            EditorUtility.SetDirty(loadout);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(loadout);
        }

        private static void AssignLoadoutToConfigs(PlayerLoadoutDefinition loadout)
        {
            var guids = AssetDatabase.FindAssets("t:PrototypeCombatConfig");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var config = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(path);
                if (config == null)
                {
                    continue;
                }

                var serializedObject = new SerializedObject(config);
                serializedObject.FindProperty("playerLoadout").objectReferenceValue = loadout;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
            }
        }

        private static void AssignLoadoutToPlayerPrefabs(PlayerLoadoutDefinition loadout)
        {
            var guids = AssetDatabase.FindAssets("t:PrototypeCombatConfig");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var config = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(path);
                if (config == null || config.playerPrefab == null)
                {
                    continue;
                }

                var prefabPath = AssetDatabase.GetAssetPath(config.playerPrefab);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    var authoring = root.GetComponentInChildren<PlayerAuthoring>(true);
                    if (authoring == null)
                    {
                        continue;
                    }

                    var serializedObject = new SerializedObject(authoring);
                    serializedObject.FindProperty("playerLoadout").objectReferenceValue = loadout;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ConfigureSpreadshot(PlayerShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerShotKind.Spreadshot;
            serializedObject.FindProperty("interval").floatValue = 0.14f;
            serializedObject.FindProperty("speed").floatValue = 410f;
            serializedObject.FindProperty("damage").floatValue = 1.24f;
            serializedObject.FindProperty("poiseDamage").floatValue = 0.3f;
            serializedObject.FindProperty("radius").floatValue = 4.5f;
            serializedObject.FindProperty("lifetime").floatValue = 0.58f;
            serializedObject.FindProperty("projectileScale").vector3Value = new Vector3(0.09f, 0.14f, 1f);
            serializedObject.FindProperty("projectileColor").colorValue = new Color(1f, 0.34f, 0.28f, 0.95f);
            serializedObject.FindProperty("glowColor").colorValue = new Color(1f, 0.34f, 0.28f, 0.14f);
            serializedObject.FindProperty("coreColor").colorValue = new Color(1f, 0.92f, 0.9f, 0.92f);
            serializedObject.FindProperty("fireCueId").stringValue = AudioCue.PlayerShootSpreadshot.ToString();
            serializedObject.FindProperty("spreadPelletCount").intValue = 4;
            serializedObject.FindProperty("spreadAngleDegrees").floatValue = 38f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureChaserShot(PlayerShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerShotKind.Chaser;
            serializedObject.FindProperty("interval").floatValue = 0.16f;
            serializedObject.FindProperty("speed").floatValue = 560f;
            serializedObject.FindProperty("damage").floatValue = 2.6f;
            serializedObject.FindProperty("poiseDamage").floatValue = 0.55f;
            serializedObject.FindProperty("radius").floatValue = 4.5f;
            serializedObject.FindProperty("lifetime").floatValue = 1.9f;
            serializedObject.FindProperty("projectileScale").vector3Value = new Vector3(0.09f, 0.14f, 1f);
            serializedObject.FindProperty("projectileColor").colorValue = new Color(1f, 0.9f, 0.18f, 0.96f);
            serializedObject.FindProperty("glowColor").colorValue = new Color(1f, 0.84f, 0.16f, 0.18f);
            serializedObject.FindProperty("coreColor").colorValue = new Color(1f, 0.98f, 0.88f, 0.94f);
            serializedObject.FindProperty("fireCueId").stringValue = AudioCue.PlayerShootChaser.ToString();
            serializedObject.FindProperty("chaserHomingDuration").floatValue = 0.9f;
            serializedObject.FindProperty("chaserTurnRate").floatValue = 480f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDefensiveRing(PlayerCounterShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerCounterShotKind.DefensiveRing;
            serializedObject.FindProperty("defensiveRingStartRadius").floatValue = 22f;
            serializedObject.FindProperty("defensiveRingFinalRadius").floatValue = 84f;
            serializedObject.FindProperty("defensiveRingThickness").floatValue = 18f;
            serializedObject.FindProperty("defensiveRingGrowDuration").floatValue = 0.25f;
            serializedObject.FindProperty("defensiveRingHoldDuration").floatValue = 4f;
            serializedObject.FindProperty("defensiveRingFadeDuration").floatValue = 0.55f;
            serializedObject.FindProperty("defensiveRingColor").colorValue = new Color(0.95f, 0.74f, 0.26f, 0.84f);
            serializedObject.FindProperty("hitCueId").stringValue = AudioCue.PlayerParrySpecialDefensiveRing.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureMolotov(PlayerCounterShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerCounterShotKind.Molotov;
            serializedObject.FindProperty("speed").floatValue = 430f;
            serializedObject.FindProperty("damage").floatValue = 0f;
            serializedObject.FindProperty("poiseDamage").floatValue = 0f;
            serializedObject.FindProperty("radius").floatValue = 10f;
            serializedObject.FindProperty("lifetime").floatValue = 1.6f;
            serializedObject.FindProperty("spawnOffset").vector2Value = new Vector2(0f, -8f);
            serializedObject.FindProperty("projectileScale").vector3Value = new Vector3(0.16f, 0.16f, 1f);
            serializedObject.FindProperty("travelScale").vector3Value = new Vector3(0.16f, 0.16f, 1f);
            serializedObject.FindProperty("projectileColor").colorValue = new Color(0.98f, 0.4f, 0.08f, 1f);
            serializedObject.FindProperty("glowColor").colorValue = new Color(1f, 0.3f, 0.08f, 0.18f);
            serializedObject.FindProperty("coreColor").colorValue = new Color(1f, 0.9f, 0.74f, 0.92f);
            serializedObject.FindProperty("molotovTravelDistance").floatValue = 180f;
            serializedObject.FindProperty("molotovFireZoneRadius").floatValue = 42f;
            serializedObject.FindProperty("molotovFireZoneDuration").floatValue = 3f;
            serializedObject.FindProperty("molotovTickInterval").floatValue = 0.5f;
            serializedObject.FindProperty("molotovDamagePerTick").floatValue = 10f;
            serializedObject.FindProperty("molotovPoiseDamagePerTick").floatValue = 4f;
            serializedObject.FindProperty("molotovFireZoneColor").colorValue = new Color(1f, 0.44f, 0.08f, 0.3f);
            serializedObject.FindProperty("molotovFireZoneGlowColor").colorValue = new Color(1f, 0.24f, 0.06f, 0.52f);
            serializedObject.FindProperty("hitCueId").stringValue = AudioCue.PlayerParrySpecialMolotov.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureExpandingGlobalRing(PlayerSkillDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("skillKind").enumValueIndex = (int)PlayerSkillKind.ExpandingGlobalRing;
            serializedObject.FindProperty("duration").floatValue = 0.65f;
            serializedObject.FindProperty("recoveryInvuln").floatValue = 0.15f;
            serializedObject.FindProperty("lockMovementWhileCasting").boolValue = false;
            serializedObject.FindProperty("blocksAutoShotWhileCasting").boolValue = false;
            serializedObject.FindProperty("grantsActiveInvulnerabilityWhileCasting").boolValue = false;
            serializedObject.FindProperty("globalRingStartRadius").floatValue = 20f;
            serializedObject.FindProperty("globalRingFinalRadius").floatValue = 560f;
            serializedObject.FindProperty("globalRingThickness").floatValue = 30f;
            serializedObject.FindProperty("castRingColor").colorValue = new Color(1f, 0.26f, 0.22f, 0.72f);
            serializedObject.FindProperty("castBurstColor").colorValue = new Color(1f, 0.22f, 0.18f, 0.92f);
            serializedObject.FindProperty("castBurstSecondaryColor").colorValue = new Color(1f, 0.58f, 0.52f, 0.86f);
            serializedObject.FindProperty("globalRingColor").colorValue = new Color(1f, 0.18f, 0.16f, 0.94f);
            serializedObject.FindProperty("castCueId").stringValue = AudioCue.PlayerSkillGlobalRingCast.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureStickyProjectileSkill(PlayerSkillDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("skillKind").enumValueIndex = (int)PlayerSkillKind.StickyProjectile;
            serializedObject.FindProperty("duration").floatValue = 0.08f;
            serializedObject.FindProperty("recoveryInvuln").floatValue = 0.1f;
            serializedObject.FindProperty("lockMovementWhileCasting").boolValue = false;
            serializedObject.FindProperty("blocksAutoShotWhileCasting").boolValue = false;
            serializedObject.FindProperty("grantsActiveInvulnerabilityWhileCasting").boolValue = false;
            serializedObject.FindProperty("stickyProjectileSpeed").floatValue = 620f;
            serializedObject.FindProperty("stickyProjectileRadius").floatValue = 8f;
            serializedObject.FindProperty("stickyProjectileLifetime").floatValue = 2.1f;
            serializedObject.FindProperty("stickyProjectileScale").vector3Value = new Vector3(0.1f, 0.22f, 1f);
            serializedObject.FindProperty("stickyProjectileColor").colorValue = new Color(1f, 0.94f, 0.24f, 0.96f);
            serializedObject.FindProperty("stickyProjectileGlowColor").colorValue = new Color(1f, 0.84f, 0.18f, 0.18f);
            serializedObject.FindProperty("stickyProjectileCoreColor").colorValue = new Color(1f, 0.99f, 0.84f, 0.94f);
            serializedObject.FindProperty("stickyProjectileDps").floatValue = 38f;
            serializedObject.FindProperty("stickyProjectilePoiseDps").floatValue = 10f;
            serializedObject.FindProperty("castCueId").stringValue = AudioCue.PlayerSkillStickyProjectileCast.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDefaultShot(PlayerShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerShotKind.Straight;
            serializedObject.FindProperty("fireCueId").stringValue = AudioCue.PlayerShootStraight.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDefaultParrySpecial(PlayerCounterShotDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("shotKind").enumValueIndex = (int)PlayerCounterShotKind.StraightCounter;
            serializedObject.FindProperty("hitCueId").stringValue = AudioCue.PlayerParrySpecialCounter.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDefaultSkill(PlayerSkillDefinition asset)
        {
            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("skillKind").enumValueIndex = (int)PlayerSkillKind.Laser;
            serializedObject.FindProperty("castCueId").stringValue = AudioCue.PlayerSkillLaserCast.ToString();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
