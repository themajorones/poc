using UnityEngine.SceneManagement;

namespace CupHeadClone.Prototype
{
    public static class SceneFlowController
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string BossRushSceneName = "ParryBossRushPrototype";
        public const string TutorialSceneName = "Tutorial";

        private static bool _pendingBossRushAutoStart;

        public static void LoadMainMenu()
        {
            _pendingBossRushAutoStart = false;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public static void LoadBossRush(bool autoStart = true)
        {
            _pendingBossRushAutoStart = autoStart;
            SceneManager.LoadScene(BossRushSceneName);
        }

        public static void LoadTutorial()
        {
            _pendingBossRushAutoStart = false;
            SceneManager.LoadScene(TutorialSceneName);
        }

        public static bool ConsumePendingBossRushAutoStart()
        {
            var shouldAutoStart = _pendingBossRushAutoStart;
            _pendingBossRushAutoStart = false;
            return shouldAutoStart;
        }
    }
}
