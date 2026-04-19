using UnityEngine;
using UnityEngine.Serialization;

namespace CupHeadClone.Prototype
{
    public enum PlayerParryEffectKind
    {
        DefaultBurst,
        DefensiveRing
    }

    [CreateAssetMenu(fileName = "PlayerParryEffect", menuName = "ParryShooter/Player/Parry Effect Definition")]
    public sealed class PlayerParryEffectDefinition : ScriptableObject
    {
        [SerializeField] private PlayerParryEffectKind effectKind = PlayerParryEffectKind.DefaultBurst;
        [SerializeField] private float shakeIntensity = 0.08f;
        [SerializeField] private float shakeDuration = 0.08f;
        [SerializeField] private float hitStopDuration = 0.02f;
        [SerializeField] private float hitStopScale = 0.05f;
        [SerializeField] private Color playerRingColor = new(0.7f, 0.56f, 1f, 0.5f);
        [SerializeField] private float playerRingRadius = 0.14f;
        [SerializeField] private float playerRingThickness = 0.1f;
        [SerializeField] private float playerRingDuration = 0.32f;
        [SerializeField] private Color bulletRingColor = new(1f, 0.82f, 0.45f, 0.5f);
        [SerializeField] private float bulletRingRadius = 0.16f;
        [SerializeField] private float bulletRingThickness = 0.08f;
        [SerializeField] private float bulletRingDuration = 0.34f;
        [SerializeField] private Color bulletBurstColor = new(1f, 0.82f, 0.45f, 1f);
        [SerializeField] private int bulletBurstCount = 10;
        [SerializeField] private float bulletBurstRadius = 0.24f;
        [SerializeField] private Color playerBurstColor = new(0.83f, 0.6f, 1f, 1f);
        [SerializeField] private int playerBurstCount = 4;
        [SerializeField] private float playerBurstRadius = 0.16f;
        [FormerlySerializedAs("successSfx")]
        [SerializeField, HideInInspector] private AudioCue legacySuccessSfx = AudioCue.ParrySuccess;
        [SerializeField] private string successCueId = "ParrySuccess";
        [SerializeField] private float defensiveRingStartRadius = 22f;
        [SerializeField] private float defensiveRingFinalRadius = 76f;
        [SerializeField] private float defensiveRingThickness = 18f;
        [SerializeField] private float defensiveRingGrowDuration = 0.22f;
        [SerializeField] private float defensiveRingHoldDuration = 4f;
        [SerializeField] private float defensiveRingFadeDuration = 0.45f;
        [SerializeField] private Color defensiveRingColor = new(0.83f, 0.6f, 1f, 0.82f);

        public PlayerParryEffectKind EffectKind => effectKind;
        public float ShakeIntensity => shakeIntensity;
        public float ShakeDuration => shakeDuration;
        public float HitStopDuration => hitStopDuration;
        public float HitStopScale => hitStopScale;
        public Color PlayerRingColor => playerRingColor;
        public float PlayerRingRadius => playerRingRadius;
        public float PlayerRingThickness => playerRingThickness;
        public float PlayerRingDuration => playerRingDuration;
        public Color BulletRingColor => bulletRingColor;
        public float BulletRingRadius => bulletRingRadius;
        public float BulletRingThickness => bulletRingThickness;
        public float BulletRingDuration => bulletRingDuration;
        public Color BulletBurstColor => bulletBurstColor;
        public int BulletBurstCount => bulletBurstCount;
        public float BulletBurstRadius => bulletBurstRadius;
        public Color PlayerBurstColor => playerBurstColor;
        public int PlayerBurstCount => playerBurstCount;
        public float PlayerBurstRadius => playerBurstRadius;
        public string SuccessCueId => successCueId;
        public float DefensiveRingStartRadius => defensiveRingStartRadius;
        public float DefensiveRingFinalRadius => defensiveRingFinalRadius;
        public float DefensiveRingThickness => defensiveRingThickness;
        public float DefensiveRingGrowDuration => defensiveRingGrowDuration;
        public float DefensiveRingHoldDuration => defensiveRingHoldDuration;
        public float DefensiveRingFadeDuration => defensiveRingFadeDuration;
        public Color DefensiveRingColor => defensiveRingColor;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(successCueId))
            {
                successCueId = legacySuccessSfx.ToString();
            }
        }
    }
}
