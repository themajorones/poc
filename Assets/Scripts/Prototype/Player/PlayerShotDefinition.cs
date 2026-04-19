using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CupHeadClone.Prototype
{
    public enum PlayerShotKind
    {
        Straight,
        Spreadshot,
        Chaser
    }

    [CreateAssetMenu(fileName = "PlayerShot", menuName = "ParryShooter/Player/Shot Definition")]
    public sealed class PlayerShotDefinition : ScriptableObject
    {
        [BoxGroup("Shot")]
        [SerializeField] private PlayerShotKind shotKind = PlayerShotKind.Straight;
        [BoxGroup("Shot")]
        [SerializeField] private float interval = 0.12f;
        [BoxGroup("Shot")]
        [SerializeField] private float speed = 670f;
        [BoxGroup("Shot")]
        [SerializeField] private float damage = 3.5f;
        [BoxGroup("Shot")]
        [SerializeField] private float poiseDamage = 0.7f;
        [BoxGroup("Shot")]
        [SerializeField] private float radius = 4f;
        [BoxGroup("Shot")]
        [SerializeField] private float lifetime = 2.1f;
        [BoxGroup("Shot")]
        [SerializeField] private Vector2 spawnOffset = new(0f, -18f);
        [BoxGroup("Visual")]
        [SerializeField] private Vector3 projectileScale = new(0.065f, 0.18f, 1f);
        [BoxGroup("Visual")]
        [SerializeField] private Color projectileColor = new(0.91f, 0.98f, 1f, 0.9f);
        [BoxGroup("Visual")]
        [SerializeField] private Color glowColor = new(0.34f, 0.95f, 1f, 0.1f);
        [BoxGroup("Visual")]
        [SerializeField] private Color coreColor = new(1f, 1f, 1f, 0.92f);
        [FormerlySerializedAs("fireSfx")]
        [SerializeField, HideInInspector] private AudioCue legacyFireSfx = AudioCue.PlayerShoot;
        [BoxGroup("Audio")]
        [SerializeField] private string fireCueId = "PlayerShoot";
        [BoxGroup("Spread"), ShowIf("@shotKind == PlayerShotKind.Spreadshot")]
        [SerializeField] private int spreadPelletCount = 4;
        [BoxGroup("Spread"), ShowIf("@shotKind == PlayerShotKind.Spreadshot")]
        [SerializeField] private float spreadAngleDegrees = 42f;
        [BoxGroup("Chaser"), ShowIf("@shotKind == PlayerShotKind.Chaser")]
        [SerializeField] private float chaserHomingDuration = 0.9f;
        [BoxGroup("Chaser"), ShowIf("@shotKind == PlayerShotKind.Chaser")]
        [SerializeField] private float chaserTurnRate = 520f;

        public PlayerShotKind ShotKind => shotKind;
        public float Interval => interval;
        public float Speed => speed;
        public float Damage => damage;
        public float PoiseDamage => poiseDamage;
        public float Radius => radius;
        public float Lifetime => lifetime;
        public Vector2 SpawnOffset => spawnOffset;
        public Vector3 ProjectileScale => projectileScale;
        public Color ProjectileColor => projectileColor;
        public Color GlowColor => glowColor;
        public Color CoreColor => coreColor;
        public string FireCueId => fireCueId;
        public int SpreadPelletCount => Mathf.Max(1, spreadPelletCount);
        public float SpreadAngleDegrees => spreadAngleDegrees;
        public float ChaserHomingDuration => Mathf.Max(0f, chaserHomingDuration);
        public float ChaserTurnRate => Mathf.Max(0f, chaserTurnRate);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(fireCueId))
            {
                fireCueId = legacyFireSfx.ToString();
            }
        }
    }
}
