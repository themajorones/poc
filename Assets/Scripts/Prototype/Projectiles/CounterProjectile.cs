using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class CounterProjectile : ProjectileBase
    {
        private SpriteRenderer _glow;
        private SpriteRenderer _core;
        private float _damage;
        private float _poiseDamage;
        private float _rageGainOnHit;
        private float _molotovTravelRemaining;
        private PlayerCounterShotDefinition _counterDefinition;

        public override bool PreserveOnProjectileClear => true;

        protected override void BuildVisuals()
        {
            _glow = PrototypeVisualUtility.EnsureSpriteChild(transform, "Glow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.CounterGold.WithAlpha(0.14f), 9);
            _core = PrototypeVisualUtility.EnsureSpriteChild(transform, "Core", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.LaserCore, 11);
        }

        public void Spawn(Vector2 logicalPosition, PlayerCounterShotDefinition counterDefinition)
        {
            _counterDefinition = counterDefinition ?? (Game.PlayerLoadout != null ? Game.PlayerLoadout.CounterShot : null);
            gameObject.SetActive(true);
            LogicalPosition = logicalPosition;
            Velocity = Vector2.up * -(_counterDefinition != null ? _counterDefinition.Speed : Game.Config.parry.counterBulletSpeed);
            Radius = _counterDefinition != null ? _counterDefinition.Radius : 8f;
            Lifetime = _counterDefinition != null ? _counterDefinition.Lifetime : 2.2f;
            _damage = _counterDefinition != null ? _counterDefinition.Damage : Game.Config.parry.counterDamage;
            _poiseDamage = _counterDefinition != null ? _counterDefinition.PoiseDamage : Game.Config.parry.counterPoiseDamage;
            _rageGainOnHit = _counterDefinition != null ? _counterDefinition.CounterHitRageGain : Game.Config.parry.counterHitRageGain;
            _molotovTravelRemaining = _counterDefinition != null ? _counterDefinition.MolotovTravelDistance : 0f;
            Renderer.color = _counterDefinition != null ? _counterDefinition.ProjectileColor : PrototypeVisualUtility.CounterGold;
            transform.localScale = _counterDefinition != null ? _counterDefinition.ProjectileScale : new Vector3(0.14f, 0.3f, 1f);
            transform.localRotation = Quaternion.identity;
            if (_glow != null)
            {
                _glow.color = _counterDefinition != null ? _counterDefinition.GlowColor : PrototypeVisualUtility.CounterGold.WithAlpha(0.12f);
            }

            if (_core != null)
            {
                _core.color = _counterDefinition != null ? _counterDefinition.CoreColor : PrototypeVisualUtility.LaserCore.WithAlpha(0.9f);
            }

            SyncTransform();
        }

        protected override void Tick(float dt)
        {
            LogicalPosition += Velocity * dt;
            Lifetime -= dt;
            var counterDefinition = _counterDefinition ?? (Game.PlayerLoadout != null ? Game.PlayerLoadout.CounterShot : null);
            var targetScale = counterDefinition != null ? counterDefinition.TravelScale : new Vector3(0.12f, 0.3f, 1f);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, dt * 8f);

            if (counterDefinition != null && counterDefinition.ShotKind == PlayerCounterShotKind.Molotov)
            {
                _molotovTravelRemaining -= Velocity.magnitude * dt;
                if (_molotovTravelRemaining <= 0f)
                {
                    Game.SpawnMolotovFireZone(LogicalPosition, counterDefinition);
                    AudioManager.Instance?.PlaySfx(counterDefinition.HitCueId);
                    Game.DespawnProjectile(this);
                    return;
                }

                if (IsOutOfBounds(-30f, Game.Config.logicalWidth + 30f, -30f, Game.Config.logicalHeight + 30f))
                {
                    Game.DespawnProjectile(this);
                    return;
                }

                SyncTransform();
                return;
            }

            if (IsOutOfBounds(-30f, Game.Config.logicalWidth + 30f, -30f, Game.Config.logicalHeight + 30f))
            {
                Game.DespawnProjectile(this);
                return;
            }

            if (Game.TryHandleCounterProjectile(this))
            {
                Game.DespawnProjectile(this);
                return;
            }

            if (Game.Boss == null || !Game.Boss.Active)
            {
                SyncTransform();
                return;
            }

            var bossPosition = Game.Boss.ContactCenterLogical;
            var bossHalfExtents = Game.Boss.ContactHalfExtents;
            var hitBoss =
                Mathf.Abs(LogicalPosition.x - bossPosition.x) <= bossHalfExtents.x + Radius &&
                Mathf.Abs(LogicalPosition.y - bossPosition.y) <= bossHalfExtents.y + Radius;

            if (hitBoss)
            {
                Game.Boss.ApplyDamage(_damage, _poiseDamage, LogicalPosition);
                Game.RageSystem.Add(_rageGainOnHit);
                Game.ParryFeedbackController.PlayCounterHit(LogicalPosition);
                Game.DespawnProjectile(this);
                return;
            }

            SyncTransform();
        }
    }
}
