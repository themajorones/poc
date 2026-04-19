using UnityEngine;

namespace CupHeadClone.Prototype
{
    public static class RingVisualPresetRuntime
    {
        private const string LibraryResourcePath = "PrototypeGenerated/Config/RingVisualPresetLibrary";
        private static RingVisualPresetLibrary s_library;

        private static RingVisualPresetLibrary Library => s_library != null
            ? s_library
            : s_library = Resources.Load<RingVisualPresetLibrary>(LibraryResourcePath);

        public static RingVisualPreset DefensiveField => Library != null ? Library.DefensiveField : null;
        public static RingVisualPreset GlobalWave => Library != null ? Library.GlobalWave : null;
        public static RingVisualPreset BossShockwave => Library != null ? Library.BossShockwave : null;
        public static RingVisualPreset TransientImpact => Library != null ? Library.TransientImpact : null;
        public static RingVisualPreset WeakZone => Library != null ? Library.WeakZone : null;
    }
}
