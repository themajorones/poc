using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CupHeadClone.Prototype
{
    public enum PlayerCounterShotKind
    {
        StraightCounter,
        DefensiveRing,
        Molotov
    }

    [CreateAssetMenu(fileName = "PlayerParrySpecial", menuName = "ParryShooter/Player/Parry Special Definition")]
    public sealed class PlayerCounterShotDefinition : ScriptableObject
    {
        [TitleGroup("Parry Special", "Triggered by a successful projectile parry.")]
        [BoxGroup("Parry Special/Behavior")]
        [LabelText("Parry Special Kind")]
        [SerializeField] private PlayerCounterShotKind shotKind = PlayerCounterShotKind.StraightCounter;

        [BoxGroup("Parry Special/Projectile")]
        [LabelText("Travel Speed")]
        [SerializeField] private float speed = 920f;
        [BoxGroup("Parry Special/Projectile")]
        [SerializeField] private float damage = 30f;
        [BoxGroup("Parry Special/Projectile")]
        [LabelText("Poise Damage")]
        [SerializeField] private float poiseDamage = 32f;
        [BoxGroup("Parry Special/Projectile")]
        [SerializeField] private float radius = 8f;
        [BoxGroup("Parry Special/Projectile")]
        [SerializeField] private float lifetime = 2.2f;
        [BoxGroup("Parry Special/Projectile")]
        [LabelText("Spawn Offset")]
        [SerializeField] private Vector2 spawnOffset = new(0f, -8f);
        [BoxGroup("Parry Special/Visual")]
        [LabelText("Spawn Scale")]
        [SerializeField] private Vector3 projectileScale = new(0.14f, 0.3f, 1f);
        [BoxGroup("Parry Special/Visual")]
        [LabelText("Travel Scale")]
        [SerializeField] private Vector3 travelScale = new(0.12f, 0.3f, 1f);
        [BoxGroup("Parry Special/Visual")]
        [LabelText("Projectile Color")]
        [SerializeField] private Color projectileColor = new(1f, 0.78f, 0.26f, 1f);
        [BoxGroup("Parry Special/Visual")]
        [LabelText("Glow Color")]
        [SerializeField] private Color glowColor = new(1f, 0.78f, 0.26f, 0.12f);
        [BoxGroup("Parry Special/Visual")]
        [LabelText("Core Color")]
        [SerializeField] private Color coreColor = new(1f, 0.95f, 0.74f, 0.9f);
        [BoxGroup("Parry Special/Rewards")]
        [LabelText("Rage Gain On Hit")]
        [SerializeField] private float counterHitRageGain = 10f;
        [FormerlySerializedAs("hitSfx")]
        [SerializeField, HideInInspector] private AudioCue legacyHitSfx = AudioCue.CounterHit;
        [BoxGroup("Parry Special/Audio")]
        [LabelText("Hit Cue Id")]
        [SerializeField] private string hitCueId = "CounterHit";
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Travel Distance")]
        [SerializeField] private float molotovTravelDistance = 180f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Fire Zone Radius")]
        [SerializeField] private float molotovFireZoneRadius = 42f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Fire Zone Duration")]
        [SerializeField] private float molotovFireZoneDuration = 3f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Tick Interval")]
        [SerializeField] private float molotovTickInterval = 0.5f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Damage Per Tick")]
        [SerializeField] private float molotovDamagePerTick = 10f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Poise Damage Per Tick")]
        [SerializeField] private float molotovPoiseDamagePerTick = 4f;
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Fire Zone Color")]
        [SerializeField] private Color molotovFireZoneColor = new(1f, 0.44f, 0.08f, 0.3f);
        [BoxGroup("Parry Special/Molotov")]
        [ShowIf("@shotKind == PlayerCounterShotKind.Molotov")]
        [LabelText("Fire Zone Glow")]
        [SerializeField] private Color molotovFireZoneGlowColor = new(1f, 0.24f, 0.06f, 0.52f);
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Start Radius")]
        [SerializeField] private float defensiveRingStartRadius = 22f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Final Radius")]
        [SerializeField] private float defensiveRingFinalRadius = 84f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Thickness")]
        [SerializeField] private float defensiveRingThickness = 18f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Grow Duration")]
        [SerializeField] private float defensiveRingGrowDuration = 0.25f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Hold Duration")]
        [SerializeField] private float defensiveRingHoldDuration = 4f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Fade Duration")]
        [SerializeField] private float defensiveRingFadeDuration = 0.55f;
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Ring Color")]
        [SerializeField] private Color defensiveRingColor = new(0.95f, 0.74f, 0.26f, 0.84f);
        [BoxGroup("Parry Special/Defensive Ring")]
        [ShowIf("@shotKind == PlayerCounterShotKind.DefensiveRing")]
        [LabelText("Max Active Rings")]
        [MinValue(1)]
        [SerializeField] private int maxActiveDefensiveRings = 2;

        public PlayerCounterShotKind ShotKind => shotKind;
        public float Speed => speed;
        public float Damage => damage;
        public float PoiseDamage => poiseDamage;
        public float Radius => radius;
        public float Lifetime => lifetime;
        public Vector2 SpawnOffset => spawnOffset;
        public Vector3 ProjectileScale => projectileScale;
        public Vector3 TravelScale => travelScale;
        public Color ProjectileColor => projectileColor;
        public Color GlowColor => glowColor;
        public Color CoreColor => coreColor;
        public float CounterHitRageGain => counterHitRageGain;
        public string HitCueId => hitCueId;
        public float MolotovTravelDistance => Mathf.Max(1f, molotovTravelDistance);
        public float MolotovFireZoneRadius => Mathf.Max(1f, molotovFireZoneRadius);
        public float MolotovFireZoneDuration => Mathf.Max(0.1f, molotovFireZoneDuration);
        public float MolotovTickInterval => Mathf.Max(0.05f, molotovTickInterval);
        public float MolotovDamagePerTick => molotovDamagePerTick;
        public float MolotovPoiseDamagePerTick => molotovPoiseDamagePerTick;
        public Color MolotovFireZoneColor => molotovFireZoneColor;
        public Color MolotovFireZoneGlowColor => molotovFireZoneGlowColor;
        public float DefensiveRingStartRadius => defensiveRingStartRadius;
        public float DefensiveRingFinalRadius => defensiveRingFinalRadius;
        public float DefensiveRingThickness => defensiveRingThickness;
        public float DefensiveRingGrowDuration => defensiveRingGrowDuration;
        public float DefensiveRingHoldDuration => defensiveRingHoldDuration;
        public float DefensiveRingFadeDuration => defensiveRingFadeDuration;
        public Color DefensiveRingColor => defensiveRingColor;
        public int MaxActiveDefensiveRings => Mathf.Max(1, maxActiveDefensiveRings);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(hitCueId))
            {
                hitCueId = legacyHitSfx.ToString();
            }
        }
    }
}
