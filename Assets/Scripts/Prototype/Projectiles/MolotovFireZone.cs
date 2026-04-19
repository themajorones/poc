using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class MolotovFireZone : ProjectileBase
    {
        private SpriteRenderer _glow;
        private SpriteRenderer _core;
        private float _damagePerTick;
        private float _poiseDamagePerTick;
        private float _tickInterval;
        private float _tickTimer;
        private float _totalDuration;
        private Color _baseColor;
        private Color _glowColor;

        public override bool PreserveOnProjectileClear => true;

        protected override void BuildVisuals()
        {
            _glow = PrototypeVisualUtility.EnsureSpriteChild(transform, "Glow", PrototypeVisualUtility.CircleSprite, new Color(1f, 0.4f, 0.08f, 0.12f), 9);
            _core = PrototypeVisualUtility.EnsureSpriteChild(transform, "Core", PrototypeVisualUtility.CircleSprite, new Color(1f, 0.84f, 0.6f, 0.18f), 11);
        }

        public void Spawn(Vector2 logicalPosition, PlayerCounterShotDefinition counterDefinition)
        {
            gameObject.SetActive(true);
            LogicalPosition = logicalPosition;
            Velocity = Vector2.zero;
            Radius = counterDefinition != null ? counterDefinition.MolotovFireZoneRadius : 42f;
            Lifetime = counterDefinition != null ? counterDefinition.MolotovFireZoneDuration : 3f;
            _totalDuration = Lifetime;
            _damagePerTick = counterDefinition != null ? counterDefinition.MolotovDamagePerTick : 10f;
            _poiseDamagePerTick = counterDefinition != null ? counterDefinition.MolotovPoiseDamagePerTick : 4f;
            _tickInterval = counterDefinition != null ? counterDefinition.MolotovTickInterval : 0.5f;
            _tickTimer = 0f;
            _baseColor = counterDefinition != null ? counterDefinition.MolotovFireZoneColor : new Color(1f, 0.44f, 0.08f, 0.3f);
            _glowColor = counterDefinition != null ? counterDefinition.MolotovFireZoneGlowColor : new Color(1f, 0.24f, 0.06f, 0.52f);
            Renderer.color = _baseColor;
            var scale = Radius / Game.Config.pixelsPerUnit * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);
            if (_glow != null)
            {
                _glow.color = _glowColor;
            }

            if (_core != null)
            {
                _core.color = Color.Lerp(_baseColor, Color.white, 0.35f);
            }

            SyncTransform();
        }

        protected override void Tick(float dt)
        {
            Lifetime -= dt;
            if (Lifetime <= 0f)
            {
                Game.DespawnProjectile(this);
                return;
            }

            _tickTimer -= dt;
            while (_tickTimer <= 0f)
            {
                _tickTimer += _tickInterval;
                if (Game.Boss != null && Game.Boss.Active && OverlapsBoss(Game.Boss))
                {
                    Game.Boss.ApplyDamage(_damagePerTick, _poiseDamagePerTick, LogicalPosition);
                }
            }

            var lifetimeRatio = Mathf.Clamp01(Lifetime / Mathf.Max(0.01f, _totalDuration));
            Renderer.color = _baseColor.WithAlpha(_baseColor.a * Mathf.Lerp(0.42f, 1f, lifetimeRatio));
            if (_glow != null)
            {
                _glow.color = _glowColor.WithAlpha(_glowColor.a * Mathf.Lerp(0.35f, 1f, lifetimeRatio));
            }

            SyncTransform();
        }

        private bool OverlapsBoss(BossController bossController)
        {
            var center = bossController.ContactCenterLogical;
            var extents = bossController.ContactHalfExtents;
            var closestX = Mathf.Clamp(LogicalPosition.x, center.x - extents.x, center.x + extents.x);
            var closestY = Mathf.Clamp(LogicalPosition.y, center.y - extents.y, center.y + extents.y);
            var delta = LogicalPosition - new Vector2(closestX, closestY);
            return delta.sqrMagnitude <= Radius * Radius;
        }
    }
}
