using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CupHeadClone.Prototype
{
    public enum PlayerSkillKind
    {
        Laser,
        ExpandingGlobalRing,
        StickyProjectile
    }

    [CreateAssetMenu(fileName = "PlayerSkill", menuName = "ParryShooter/Player/Skill Definition")]
    public sealed class PlayerSkillDefinition : ScriptableObject
    {
        [BoxGroup("Skill")]
        [SerializeField] private PlayerSkillKind skillKind = PlayerSkillKind.Laser;
        [BoxGroup("Skill")]
        [SerializeField] private float duration = 1f;
        [BoxGroup("Skill")]
        [SerializeField] private float recoveryInvuln = 0.5f;
        [BoxGroup("Laser"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private float laneHitAllowance = 56f;
        [BoxGroup("Laser/Damage"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [LabelText("HP Damage Per Second")]
        [MinValue(0f)]
        [SerializeField] private float laserDps = 190f;
        [BoxGroup("Laser/Damage"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [LabelText("Stagger Damage Per Second")]
        [MinValue(0f)]
        [SerializeField] private float laserPoiseDps = 30f;
        [BoxGroup("Laser/Damage"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [LabelText("Rage Gain Per Second")]
        [MinValue(0f)]
        [SerializeField] private float rageGainPerSecond = 0.6f;
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private Color laserOuterColor = new(1f, 0.95f, 0.74f, 0.28f);
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private Color laserBeamColor = new(1f, 0.561f, 0.365f, 0.82f);
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private Color laserCoreColor = new(1f, 0.945f, 0.678f, 0.96f);
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [MinValue(0f)]
        [SerializeField] private float laserOuterWidth = 0.24f;
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [MinValue(0f)]
        [SerializeField] private float laserBeamWidth = 0.16f;
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [MinValue(0f)]
        [SerializeField] private float laserCoreWidth = 0.06f;
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [MinValue(0f)]
        [SerializeField] private float laserPulseAmplitude = 0.02f;
        [BoxGroup("Laser/Visual"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [MinValue(0f)]
        [SerializeField] private float laserPulseSpeed = 40f;
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private Color laserCastBurstColor = new(1f, 0.84f, 0.42f, 0.92f);
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private int laserCastBurstCount = 8;
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private float laserCastBurstRadius = 0.34f;
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private Color laserCastBurstSecondaryColor = new(1f, 0.95f, 0.74f, 0.86f);
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private int laserCastBurstSecondaryCount = 4;
        [BoxGroup("Laser Cast FX"), ShowIf("@skillKind == PlayerSkillKind.Laser")]
        [SerializeField] private float laserCastBurstSecondaryRadius = 0.22f;
        [BoxGroup("Cast Feel")]
        [SerializeField] private float castShakeIntensity = 0.1f;
        [BoxGroup("Cast Feel")]
        [SerializeField] private float castShakeDuration = 0.12f;
        [BoxGroup("Cast Feel")]
        [SerializeField] private float castHitStopDuration = 0.02f;
        [BoxGroup("Cast Feel")]
        [SerializeField] private float castHitStopScale = 0.04f;
        [BoxGroup("Cast Ring"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private Color castRingColor = new(0.98f, 0.8f, 0.4f, 0.72f);
        [BoxGroup("Cast Ring"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private float castRingRadius = 0.75f;
        [BoxGroup("Cast Ring"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private float castRingThickness = 0.12f;
        [BoxGroup("Cast Ring"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private float castRingDuration = 0.92f;
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private Color castBurstColor = new(1f, 0.84f, 0.42f, 0.92f);
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private int castBurstCount = 8;
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private float castBurstRadius = 0.34f;
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private Color castBurstSecondaryColor = new(1f, 0.95f, 0.74f, 0.86f);
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private int castBurstSecondaryCount = 4;
        [BoxGroup("Cast Burst"), ShowIf("@skillKind != PlayerSkillKind.Laser")]
        [SerializeField] private float castBurstSecondaryRadius = 0.22f;
        [BoxGroup("Skill")]
        [SerializeField] private bool lockMovementWhileCasting = true;
        [BoxGroup("Skill")]
        [SerializeField] private bool blocksAutoShotWhileCasting = true;
        [BoxGroup("Skill")]
        [SerializeField] private bool grantsActiveInvulnerabilityWhileCasting = true;
        [BoxGroup("Global Ring"), ShowIf("@skillKind == PlayerSkillKind.ExpandingGlobalRing")]
        [SerializeField] private float globalRingStartRadius = 18f;
        [BoxGroup("Global Ring"), ShowIf("@skillKind == PlayerSkillKind.ExpandingGlobalRing")]
        [SerializeField] private float globalRingFinalRadius = 520f;
        [BoxGroup("Global Ring"), ShowIf("@skillKind == PlayerSkillKind.ExpandingGlobalRing")]
        [SerializeField] private float globalRingThickness = 28f;
        [BoxGroup("Global Ring"), ShowIf("@skillKind == PlayerSkillKind.ExpandingGlobalRing")]
        [SerializeField] private Color globalRingColor = new(1f, 0.88f, 0.44f, 0.92f);
        [BoxGroup("Global Ring"), ShowIf("@skillKind == PlayerSkillKind.ExpandingGlobalRing")]
        [SerializeField] private int maxActiveGlobalRings = 1;
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Projectile")]
        [LabelText("Travel Speed")]
        [MinValue(0f)]
        [SerializeField] private float stickyProjectileSpeed = 670f;
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Projectile")]
        [LabelText("Hit Radius")]
        [MinValue(0f)]
        [SerializeField] private float stickyProjectileRadius = 6f;
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Projectile")]
        [LabelText("Projectile Lifetime")]
        [MinValue(0f)]
        [SerializeField] private float stickyProjectileLifetime = 2.1f;
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Visual")]
        [SerializeField] private Vector3 stickyProjectileScale = new(0.08f, 0.18f, 1f);
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Visual")]
        [SerializeField] private Color stickyProjectileColor = new(1f, 0.94f, 0.24f, 0.96f);
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Visual")]
        [SerializeField] private Color stickyProjectileGlowColor = new(1f, 0.84f, 0.18f, 0.18f);
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Visual")]
        [SerializeField] private Color stickyProjectileCoreColor = new(1f, 0.99f, 0.84f, 0.94f);
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Damage")]
        [LabelText("HP Damage Per Second")]
        [MinValue(0f)]
        [SerializeField] private float stickyProjectileDps = 36f;
        [BoxGroup("Sticky Projectile"), ShowIf("@skillKind == PlayerSkillKind.StickyProjectile")]
        [BoxGroup("Sticky Projectile/Damage")]
        [LabelText("Stagger Damage Per Second")]
        [MinValue(0f)]
        [SerializeField] private float stickyProjectilePoiseDps = 10f;
        [FormerlySerializedAs("castSfx")]
        [SerializeField, HideInInspector] private AudioCue legacyCastSfx = AudioCue.SkillCast;
        [BoxGroup("Audio"), PropertyOrder(200)]
        [SerializeField] private string castCueId = "SkillCast";

        public PlayerSkillKind SkillKind => skillKind;
        public float Duration => duration;
        public float RecoveryInvuln => recoveryInvuln;
        public float LaneHitAllowance => laneHitAllowance;
        public float LaserDps => laserDps;
        public float LaserPoiseDps => laserPoiseDps;
        public float RageGainPerSecond => rageGainPerSecond;
        public Color LaserOuterColor => laserOuterColor;
        public Color LaserBeamColor => laserBeamColor;
        public Color LaserCoreColor => laserCoreColor;
        public float LaserOuterWidth => laserOuterWidth;
        public float LaserBeamWidth => laserBeamWidth;
        public float LaserCoreWidth => laserCoreWidth;
        public float LaserPulseAmplitude => laserPulseAmplitude;
        public float LaserPulseSpeed => laserPulseSpeed;
        public Color LaserCastBurstColor => laserCastBurstColor;
        public int LaserCastBurstCount => laserCastBurstCount;
        public float LaserCastBurstRadius => laserCastBurstRadius;
        public Color LaserCastBurstSecondaryColor => laserCastBurstSecondaryColor;
        public int LaserCastBurstSecondaryCount => laserCastBurstSecondaryCount;
        public float LaserCastBurstSecondaryRadius => laserCastBurstSecondaryRadius;
        public float CastShakeIntensity => castShakeIntensity;
        public float CastShakeDuration => castShakeDuration;
        public float CastHitStopDuration => castHitStopDuration;
        public float CastHitStopScale => castHitStopScale;
        public Color CastRingColor => castRingColor;
        public float CastRingRadius => castRingRadius;
        public float CastRingThickness => castRingThickness;
        public float CastRingDuration => castRingDuration;
        public Color CastBurstColor => castBurstColor;
        public int CastBurstCount => castBurstCount;
        public float CastBurstRadius => castBurstRadius;
        public Color CastBurstSecondaryColor => castBurstSecondaryColor;
        public int CastBurstSecondaryCount => castBurstSecondaryCount;
        public float CastBurstSecondaryRadius => castBurstSecondaryRadius;
        public string CastCueId => castCueId;
        public bool LockMovementWhileCasting => lockMovementWhileCasting;
        public bool BlocksAutoShotWhileCasting => blocksAutoShotWhileCasting;
        public bool GrantsActiveInvulnerabilityWhileCasting => grantsActiveInvulnerabilityWhileCasting;
        public float GlobalRingStartRadius => globalRingStartRadius;
        public float GlobalRingFinalRadius => globalRingFinalRadius;
        public float GlobalRingThickness => globalRingThickness;
        public Color GlobalRingColor => globalRingColor;
        public int MaxActiveGlobalRings => Mathf.Max(1, maxActiveGlobalRings);
        public float StickyProjectileSpeed => stickyProjectileSpeed;
        public float StickyProjectileRadius => stickyProjectileRadius;
        public float StickyProjectileLifetime => stickyProjectileLifetime;
        public Vector3 StickyProjectileScale => stickyProjectileScale;
        public Color StickyProjectileColor => stickyProjectileColor;
        public Color StickyProjectileGlowColor => stickyProjectileGlowColor;
        public Color StickyProjectileCoreColor => stickyProjectileCoreColor;
        public float StickyProjectileDps => stickyProjectileDps;
        public float StickyProjectilePoiseDps => stickyProjectilePoiseDps;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(castCueId))
            {
                castCueId = legacyCastSfx.ToString();
            }
        }
    }
}
