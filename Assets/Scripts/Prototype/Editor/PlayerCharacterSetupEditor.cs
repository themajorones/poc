#if UNITY_EDITOR
using System.IO;
using CupHeadClone.Prototype;
using UnityEditor;
using UnityEngine;

namespace CupHeadClone.PrototypeEditor
{
    public static class PlayerCharacterSetupEditor
    {
        private const string CharacterFolder = "Assets/PrototypeGenerated/Config/Characters";
        private const string ConfigPath = "Assets/PrototypeGenerated/Config/PrototypeCombatConfig.asset";
        private const string BluePrefabPath = "Assets/PrototypeGenerated/Prefabs/PlayerPrefab/Blue.prefab";
        private const string RedPrefabPath = "Assets/PrototypeGenerated/Prefabs/PlayerPrefab/Red.prefab";

        [MenuItem("Tools/ParryShooter/Setup Player Characters")]
        public static void SetupPlayerCharacters()
        {
            Directory.CreateDirectory(CharacterFolder);

            var config = AssetDatabase.LoadAssetAtPath<PrototypeCombatConfig>(ConfigPath);
            if (config == null)
            {
                EditorUtility.DisplayDialog("Config Missing", "PrototypeCombatConfig.asset was not found.", "OK");
                return;
            }

            var bluePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BluePrefabPath);
            var redPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RedPrefabPath);
            if (bluePrefab == null || redPrefab == null)
            {
                EditorUtility.DisplayDialog("Player Prefabs Missing", "Blue.prefab or Red.prefab was not found under Assets/PrototypeGenerated/Prefabs/PlayerPrefab/.", "OK");
                return;
            }

            var blueMarker = EnsureTutorialMarker(BluePrefabPath);
            var blueCharacter = CreateOrLoad<PlayerCharacterDefinition>($"{CharacterFolder}/BlueCharacter.asset");
            var redCharacter = CreateOrLoad<PlayerCharacterDefinition>($"{CharacterFolder}/RedCharacter.asset");
            var roster = CreateOrLoad<PlayerCharacterRoster>($"{CharacterFolder}/PlayerCharacterRoster.asset");

            ConfigureCharacter(blueCharacter, "blue", "Blue", bluePrefab, 3, 100f);
            ConfigureCharacter(redCharacter, "red", "Red", redPrefab, 3, 100f);
            ConfigureRoster(roster, blueCharacter, redCharacter);

            var configSo = new SerializedObject(config);
            configSo.FindProperty("characterRoster").objectReferenceValue = roster;
            configSo.FindProperty("tutorialBluePlayerPrefab").objectReferenceValue = blueMarker;
            configSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(config);
            EditorUtility.SetDirty(blueCharacter);
            EditorUtility.SetDirty(redCharacter);
            EditorUtility.SetDirty(roster);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(roster);
        }

        private static void ConfigureCharacter(PlayerCharacterDefinition character, string id, string displayName, GameObject prefab, int maxHp, float rageMax)
        {
            var so = new SerializedObject(character);
            so.FindProperty("characterId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("playerPrefab").objectReferenceValue = prefab;
            so.FindProperty("maxHp").intValue = maxHp;
            so.FindProperty("rageMax").floatValue = rageMax;

            var preview = prefab != null ? prefab.GetComponent<PlayerAuthoring>() : null;
            if (preview != null)
            {
                so.FindProperty("previewSprite").objectReferenceValue = preview.BodySprite;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRoster(PlayerCharacterRoster roster, PlayerCharacterDefinition blue, PlayerCharacterDefinition red)
        {
            var so = new SerializedObject(roster);
            so.FindProperty("defaultIndex").intValue = 0;
            var list = so.FindProperty("characters");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = blue;
            list.GetArrayElementAtIndex(1).objectReferenceValue = red;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TutorialBluePlayerMarker EnsureTutorialMarker(string prefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var marker = root.GetComponent<TutorialBluePlayerMarker>();
                if (marker == null)
                {
                    marker = root.AddComponent<TutorialBluePlayerMarker>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponent<TutorialBluePlayerMarker>();
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
    }
}
#endif
