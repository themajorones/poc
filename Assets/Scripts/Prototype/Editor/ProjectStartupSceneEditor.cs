#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CupHeadClone.PrototypeEditor
{
    public static class ProjectStartupSceneEditor
    {
        private const string BossRushScenePath = "Assets/Scenes/ParryBossRushPrototype.unity";
        private const string StartupSessionKey = "ParryShooter.Editor.OpenBossRushOnProjectLoad";

        [InitializeOnLoadMethod]
        private static void OpenBossRushOnProjectLoad()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(StartupSessionKey, false))
                {
                    return;
                }

                SessionState.SetBool(StartupSessionKey, true);

                if (!File.Exists(BossRushScenePath))
                {
                    return;
                }

                var activeScene = EditorSceneManager.GetActiveScene();
                if (activeScene.path == BossRushScenePath)
                {
                    return;
                }

                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                EditorSceneManager.OpenScene(BossRushScenePath, OpenSceneMode.Single);
            };
        }
    }
}
#endif
