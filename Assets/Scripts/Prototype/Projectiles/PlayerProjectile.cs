using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerProjectile : ProjectileBase
    {
        private enum ProjectileMode
        {
            PrimaryShot,
            StickySkill
        }

        private SpriteRenderer _glow;
        private SpriteRenderer _core;
        private ProjectileMode _mode;
        private PlayerShotKind _shotKind;
        private float _damage;
        private float _poiseDamage;
        private float _homingTimer;
        private float _turnRate;
        private float _travelSpeed;
        private bool _lostHomingTarget;
        private bool _isAttached;
        private BossController _attachedBoss;
        private Vector2 _attachedOffset;
        private float _attachedDps;
        private float _attachedPoiseDps;

        public override bool PreserveOnProjectileClear => true;

        protected override void BuildVisuals()
        {
            _glow = PrototypeVisualUtility.EnsureSpriteChild(transform, "Glow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.PlayerCyan.WithAlpha(0.12f), 9);
            _core = PrototypeVisualUtility.EnsureSpriteChild(transform, "Core", PrototypeVisualUtility.CircleSprite, Color.white, 11);
        }

        public void Spawn(Vector2 logicalPosition, PlayerShotDefinition shotDefinition, Vector2? velocityOverride = null)
        {
            shotDefinition ??= Game.PlayerLoadout != null ? Game.PlayerLoadout.PrimaryShot : null;
            _mode = ProjectileMode.PrimaryShot;
            _shotKind = shotDefinition != null ? shotDefinition.ShotKind : PlayerShotKind.Straight;
            _isAttached = false;
            _attachedBoss = null;
            _lostHomingTarget = false;
            gameObject.SetActive(true);
            LogicalPosition = logicalPosition;
            Velocity = velocityOverride ?? Vector2.up * -(shotDefinition != null ? shotDefinition.Speed : Game.Config.autoShot.speed);
            _travelSpeed = Velocity.magnitude;
            Radius = shotDefinition != null ? shotDefinition.Radius : 4f;
            Lifetime = shotDefinition != null ? shotDefinition.Lifetime : 2.1f;
            _damage = shotDefinition != null ? shotDefinition.Damage : Game.Config.autoShot.damage;
            _poiseDamage = shotDefinition != null ? shotDefinition.PoiseDamage : Game.Config.autoShot.poiseDamage;
            _homingTimer = shotDefinition != null && shotDefinition.ShotKind == PlayerShotKind.Chaser ? shotDefinition.ChaserHomingDuration : 0f;
            _turnRate = shotDefinition != null ? shotDefinition.ChaserTurnRate : 0f;
            _attachedDps = 0f;
            _attachedPoiseDps = 0f;
            ApplyVisuals(
                shotDefinition != null ? shotDefinition.ProjectileColor : new Color(0.91f, 0.98f, 1f, 0.9f),
                shotDefinition != null ? shotDefinition.GlowColor : PrototypeVisualUtility.PlayerCyan.WithAlpha(0.1f),
                shotDefinition != null ? shotDefinition.CoreColor : Color.white.WithAlpha(0.92f),
                shotDefinition != null ? shotDefinition.ProjectileScale : new Vector3(0.065f, 0.18f, 1f));
            SyncTransform();
        }

        public void SpawnSkillProjectile(Vector2 logicalPosition, PlayerSkillDefinition skillDefinition, Vector2? velocityOverride = null)
        {
            _mode = ProjectileMode.StickySkill;
            _shotKind = PlayerShotKind.Straight;
            _isAttached = false;
            _attachedBoss = null;
            _lostHomingTarget = true;
            gameObject.SetActive(true);
            LogicalPosition = logicalPosition;
            Velocity = velocityOverride ?? Vector2.up * -skillDefinition.StickyProjectileSpeed;
            _travelSpeed = Velocity.magnitude;
            Radius = skillDefinition.StickyProjectileRadius;
            Lifetime = skillDefinition.StickyProjectileLifetime;
            _damage = 0f;
            _poiseDamage = 0f;
            _homingTimer = 0f;
            _turnRate = 0f;
            _attachedDps = skillDefinition.StickyProjectileDps;
            _attachedPoiseDps = skillDefinition.StickyProjectilePoiseDps;
            ApplyVisuals(
                skillDefinition.StickyProjectileColor,
                skillDefinition.StickyProjectileGlowColor,
                skillDefinition.StickyProjectileCoreColor,
                skillDefinition.StickyProjectileScale);
            SyncTransform();
        }

        protected override void Tick(float dt)
        {
            if (_mode == ProjectileMode.StickySkill && _isAttached)
            {
                TickAttachedSkillProjectile(dt);
                return;
            }

            Lifetime -= dt;
            TickPrimaryHoming(dt);
            LogicalPosition += Velocity * dt;

            if (IsOutOfBounds(-20f, Game.Config.logicalWidth + 20f, -20f, Game.Config.logicalHeight + 20f))
            {
                Game.DespawnProjectile(this);
                return;
            }

            if (Game.Boss == null || !Game.Boss.Active)
            {
                SyncTransform();
                return;
            }

            if (HitsBoss(Game.Boss))
            {
                if (_mode == ProjectileMode.StickySkill)
                {
                    AttachToBoss(Game.Boss);
                    return;
                }

                Game.Boss.ApplyDamage(_damage, _poiseDamage, LogicalPosition);
                Game.DespawnProjectile(this);
                return;
            }

            SyncTransform();
        }

        private void TickPrimaryHoming(float dt)
        {
            if (_mode != ProjectileMode.PrimaryShot || _shotKind != PlayerShotKind.Chaser || _homingTimer <= 0f || _lostHomingTarget)
            {
                return;
            }

            _homingTimer = Mathf.Max(0f, _homingTimer - dt);
            if (Game.Boss == null || !Game.Boss.Active)
            {
                _lostHomingTarget = true;
                return;
            }

            var targetDirection = Game.Boss.ContactCenterLogical - LogicalPosition;
            if (targetDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            targetDirection.Normalize();
            var currentDirection = Velocity.sqrMagnitude > 0.001f ? Velocity.normalized : Vector2.up * -1f;
            var rotatedDirection = Vector3.RotateTowards(
                new Vector3(currentDirection.x, currentDirection.y, 0f),
                new Vector3(targetDirection.x, targetDirection.y, 0f),
                _turnRate * Mathf.Deg2Rad * dt,
                0f);
            Velocity = new Vector2(rotatedDirection.x, rotatedDirection.y).normalized * Mathf.Max(_travelSpeed, 0.001f);
        }

        private void TickAttachedSkillProjectile(float dt)
        {
            if (_attachedBoss == null || !_attachedBoss.Active)
            {
                Game.DespawnProjectile(this);
                return;
            }

            LogicalPosition = _attachedBoss.LogicalPosition + _attachedOffset;
            _attachedBoss.ApplyDamage(_attachedDps * dt, _attachedPoiseDps * dt, LogicalPosition);
            SyncTransform();
        }

        private bool HitsBoss(BossController bossController)
        {
            var bossPosition = bossController.ContactCenterLogical;
            var bossHalfExtents = bossController.ContactHalfExtents;
            return Mathf.Abs(LogicalPosition.x - bossPosition.x) <= bossHalfExtents.x + Radius &&
                   Mathf.Abs(LogicalPosition.y - bossPosition.y) <= bossHalfExtents.y + Radius;
        }

        private void AttachToBoss(BossController bossController)
        {
            _attachedBoss = bossController;
            _attachedOffset = LogicalPosition - bossController.LogicalPosition;
            _isAttached = true;
            Velocity = Vector2.zero;
            SyncTransform();
        }

        private void ApplyVisuals(Color projectileColor, Color glowColor, Color coreColor, Vector3 scale)
        {
            Renderer.color = projectileColor;
            transform.localScale = scale;
            transform.localRotation = Quaternion.identity;
            if (_glow != null)
            {
                _glow.color = glowColor;
            }

            if (_core != null)
            {
                _core.color = coreColor;
            }
        }
    }
}
