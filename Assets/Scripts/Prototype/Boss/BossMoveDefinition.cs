using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public enum BossMoveKind
    {
        AimedFan,
        StaggerWave,
        OffsetRain,
        LaneBarrage,
        TwinLance,
        SweepBloom,
        CrossBurst,
        CheckerDrop,
        Pinwheel,
        SideSnakes,
        WedgePress,
        SplitCurtain,
        CometCurtain,
        PulseGrid,
        OrbitMinefall,
        PrismFork,
        CrownRain,
        CrushColumns,
        HelixGate,
        FinalConvergence,
        ShockwaveTriple,
        ParryCharge,
        TrackingParryOrb
    }

    [CreateAssetMenu(fileName = "BossMove", menuName = "ParryShooter/Boss Move")]
    public sealed class BossMoveDefinition : ScriptableObject
    {
        [BoxGroup("Identity")]
        [SerializeField] private BossMoveKind moveKind;
        [BoxGroup("Identity")]
        [SerializeField] private string displayLabel;
        [BoxGroup("Identity")]
        [PreviewField(72, ObjectFieldAlignment.Left)]
        [SerializeField] private Sprite previewSprite;

        [BoxGroup("Timing")]
        [SerializeField] private bool overrideTiming;
        [BoxGroup("Timing"), ShowIf(nameof(overrideTiming))]
        [SerializeField] private float duration = 1f;
        [BoxGroup("Timing"), ShowIf(nameof(overrideTiming))]
        [SerializeField] private float shotCooldown = 0.1f;

        [BoxGroup("Authoring"), MultiLineProperty(3)]
        [SerializeField] private string notes;

        [BoxGroup("Tracking Parry Orb"), ShowIf("@moveKind == BossMoveKind.TrackingParryOrb")]
        [SerializeField] private float trackingParryOrbWindup = 0.5f;
        [BoxGroup("Tracking Parry Orb"), ShowIf("@moveKind == BossMoveKind.TrackingParryOrb")]
        [SerializeField] private float trackingParryOrbSpeed = 110f;
        [BoxGroup("Tracking Parry Orb"), ShowIf("@moveKind == BossMoveKind.TrackingParryOrb")]
        [SerializeField] private float trackingParryOrbRadius = 30f;
        [BoxGroup("Tracking Parry Orb"), ShowIf("@moveKind == BossMoveKind.TrackingParryOrb")]
        [SerializeField] private float trackingParryOrbLifetime = 6f;
        [BoxGroup("Tracking Parry Orb"), ShowIf("@moveKind == BossMoveKind.TrackingParryOrb")]
        [SerializeField] private float trackingParryOrbTurnRate = 190f;

        [BoxGroup("Shockwave Triple"), ShowIf("@moveKind == BossMoveKind.ShockwaveTriple")]
        [MinValue(1)]
        [SerializeField] private int shockwaveTripleRingCount = 3;
        [BoxGroup("Shockwave Triple"), ShowIf("@moveKind == BossMoveKind.ShockwaveTriple")]
        [MinValue(0.05f)]
        [SerializeField] private float shockwaveTripleRingInterval = 1f;
        [BoxGroup("Shockwave Triple"), ShowIf("@moveKind == BossMoveKind.ShockwaveTriple")]
        [MinValue(1f)]
        [SerializeField] private float shockwaveTripleRingSpeed = 265f;

        public BossMoveKind MoveKind => moveKind;
        public string MoveId => BossPatternDefinitions.GetMoveId(moveKind);
        public string DisplayLabel => string.IsNullOrWhiteSpace(displayLabel) ? BossPatternDefinitions.GetLabel(moveKind) : displayLabel;
        public Sprite PreviewSprite => previewSprite;
        public bool OverrideTiming => overrideTiming;
        public float Duration => duration;
        public float ShotCooldown => shotCooldown;
        public string Notes => notes;
        public float TrackingParryOrbWindup => trackingParryOrbWindup;
        public float TrackingParryOrbSpeed => trackingParryOrbSpeed;
        public float TrackingParryOrbRadius => trackingParryOrbRadius;
        public float TrackingParryOrbLifetime => trackingParryOrbLifetime;
        public float TrackingParryOrbTurnRate => trackingParryOrbTurnRate;
        public int ShockwaveTripleRingCount => Mathf.Max(1, shockwaveTripleRingCount);
        public float ShockwaveTripleRingInterval => Mathf.Max(0.05f, shockwaveTripleRingInterval);
        public float ShockwaveTripleRingSpeed => Mathf.Max(1f, shockwaveTripleRingSpeed);
    }
}
