using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class RingVisualPresetLibrary : ScriptableObject
    {
        [SerializeField] private RingVisualPreset defensiveField;
        [SerializeField] private RingVisualPreset globalWave;
        [SerializeField] private RingVisualPreset bossShockwave;
        [SerializeField] private RingVisualPreset transientImpact;
        [SerializeField] private RingVisualPreset weakZone;

        public RingVisualPreset DefensiveField => defensiveField;
        public RingVisualPreset GlobalWave => globalWave;
        public RingVisualPreset BossShockwave => bossShockwave;
        public RingVisualPreset TransientImpact => transientImpact;
        public RingVisualPreset WeakZone => weakZone;
    }
}
