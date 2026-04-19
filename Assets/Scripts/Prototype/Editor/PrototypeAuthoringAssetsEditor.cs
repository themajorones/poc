#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CupHeadClone.Prototype.Editor
{
    public static class PrototypeAuthoringAssetsEditor
    {
        private const string GeneratedRoot = "Assets/PrototypeGenerated";
        private const string DataRoot = GeneratedRoot + "/Data";
        private const string MoveRoot = DataRoot + "/BossMoves";

        [MenuItem("Tools/ParryShooter/Seed Boss Move Assets")]
        public static void SeedBossMoveAssets()
        {
            SeedBossMoveAssetsInternal();
            AssetDatabase.Refresh();
        }

        private static void SeedBossMoveAssetsInternal()
        {
            EnsureFolders();
            var moveLookup = new Dictionary<BossMoveKind, BossMoveDefinition>();
            foreach (BossMoveKind moveKind in System.Enum.GetValues(typeof(BossMoveKind)))
            {
                var moveId = BossPatternDefinitions.GetMoveId(moveKind);
                var assetPath = $"{MoveRoot}/{moveId}.asset";
                var moveAsset = AssetDatabase.LoadAssetAtPath<BossMoveDefinition>(assetPath);
                if (moveAsset == null)
                {
                    moveAsset = ScriptableObject.CreateInstance<BossMoveDefinition>();
                    AssetDatabase.CreateAsset(moveAsset, assetPath);
                }

                var serializedMove = new SerializedObject(moveAsset);
                serializedMove.FindProperty("moveKind").enumValueIndex = (int)moveKind;
                serializedMove.FindProperty("displayLabel").stringValue = BossPatternDefinitions.GetLabel(moveKind);
                if (moveKind == BossMoveKind.ShockwaveTriple)
                {
                    serializedMove.FindProperty("shockwaveTripleRingCount").intValue = 3;
                    serializedMove.FindProperty("shockwaveTripleRingInterval").floatValue = 1f;
                    serializedMove.FindProperty("shockwaveTripleRingSpeed").floatValue = 265f;
                }
                serializedMove.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(moveAsset);

                moveLookup[moveKind] = moveAsset;
            }

            var config = FindConfigAsset();
            if (config != null)
            {
                AssignMoveAssetsToBosses(config, moveLookup);
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
        }

        private static void AssignMoveAssetsToBosses(PrototypeCombatConfig config, Dictionary<BossMoveKind, BossMoveDefinition> moveLookup)
        {
            if (config.bosses == null)
            {
                return;
            }

            foreach (var boss in config.bosses)
            {
                if (boss == null || boss.moveQueue == null || boss.moveQueue.Count == 0)
                {
                    continue;
                }

                boss.moveDefinitions ??= new List<BossMoveDefinition>();
                if (boss.moveDefinitions.Count > 0)
                {
                    continue;
                }

                foreach (var moveId in boss.moveQueue)
                {
                    if (TryMapMoveId(moveId, out var moveKind) && moveLookup.TryGetValue(moveKind, out var moveAsset))
                    {
                        boss.moveDefinitions.Add(moveAsset);
                    }
                }
            }
        }

        private static bool TryMapMoveId(string moveId, out BossMoveKind moveKind)
        {
            foreach (BossMoveKind candidate in System.Enum.GetValues(typeof(BossMoveKind)))
            {
                if (BossPatternDefinitions.GetMoveId(candidate) == moveId)
                {
                    moveKind = candidate;
                    return true;
                }
            }

            moveKind = default;
            return false;
        }

        private static PrototypeCombatConfig FindConfigAsset()
        {
            var guids = AssetDatabase.FindAssets("t:PrototypeCombatConfig");
            if (guids.Length == 0)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "PrototypeGenerated");
            EnsureFolder(GeneratedRoot, "Data");
            EnsureFolder(DataRoot, "BossMoves");
        }

        private static void EnsureFolder(string parent, string name)
        {
            var target = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
#endif
