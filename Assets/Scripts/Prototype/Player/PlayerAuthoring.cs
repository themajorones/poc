using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerAuthoring : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerLoadoutDefinition playerLoadout;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer coreRenderer;
        [SerializeField] private SpriteRenderer parryShieldRenderer;
        [SerializeField] private SpriteRenderer skillAuraRenderer;
        [SerializeField] private SpriteRenderer laserBeamRenderer;
        [SerializeField] private SpriteRenderer laserCoreRenderer;

        [Header("Presentation Tuning")]
        [SerializeField] private float idleGlowScale = 0.38f;
        [SerializeField] private float parryGlowScale = 0.48f;
        [SerializeField] private float laserMuzzleOffset = 0.22f;

        [Header("Gameplay Anchors")]
        [SerializeField] private Transform hurtboxAnchor;
        [SerializeField] private Transform parryZoneAnchor;

        public SpriteRenderer BodyRenderer => bodyRenderer;
        public SpriteRenderer CoreRenderer => coreRenderer;
        public SpriteRenderer ParryShieldRenderer => parryShieldRenderer;
        public SpriteRenderer SkillAuraRenderer => skillAuraRenderer;
        public SpriteRenderer LaserBeamRenderer => laserBeamRenderer;
        public SpriteRenderer LaserCoreRenderer => laserCoreRenderer;
        public PlayerLoadoutDefinition PlayerLoadout => playerLoadout;
        public float IdleGlowScale => idleGlowScale > 0f ? idleGlowScale : 0.38f;
        public float ParryGlowScale => parryGlowScale > 0f ? parryGlowScale : 0.48f;
        public float LaserMuzzleOffset => laserMuzzleOffset;
        public Transform HurtboxAnchor => hurtboxAnchor;
        public Transform ParryZoneAnchor => parryZoneAnchor;
        public Sprite BodySprite => bodyRenderer != null ? bodyRenderer.sprite : null;

        public float GetHitboxRadius(PrototypeCombatConfig config)
        {
            if (coreRenderer != null)
            {
                return GetRadiusFromRenderer(coreRenderer, config.player.hitboxRadius, config.pixelsPerUnit);
            }

            return GetRadiusFromAnchor(hurtboxAnchor, config.player.hitboxRadius, config.pixelsPerUnit);
        }

        public float GetParryRadius(PrototypeCombatConfig config)
        {
            return GetRadiusFromAnchor(parryZoneAnchor, config.player.parryOuterRadius, config.pixelsPerUnit);
        }

        private static float GetRadiusFromAnchor(Transform anchor, float fallbackRadius, float pixelsPerUnit)
        {
            if (anchor == null)
            {
                return fallbackRadius;
            }

            return Mathf.Max(1f, anchor.lossyScale.x * pixelsPerUnit * 0.5f);
        }

        private static float GetRadiusFromRenderer(SpriteRenderer renderer, float fallbackRadius, float pixelsPerUnit)
        {
            if (renderer == null)
            {
                return fallbackRadius;
            }

            var scale = renderer.transform.lossyScale;
            var sprite = renderer.sprite;
            if (sprite == null)
            {
                return Mathf.Max(1f, scale.x * pixelsPerUnit * 0.5f);
            }

            var spriteWidthWorld = sprite.rect.width / sprite.pixelsPerUnit * scale.x;
            return Mathf.Max(1f, spriteWidthWorld * pixelsPerUnit * 0.5f);
        }
    }
}
