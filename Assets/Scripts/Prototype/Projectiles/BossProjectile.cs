using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossProjectile : ProjectileBase
    {
        public enum ProjectileKind
        {
            Normal,
            Parry
        }

        private ProjectileKind _kind;
        private float _spin;
        private float _homingSpeed;
        private float _homingTurnRate;
        private float _postParryGraceTimer;
        private string _sourceMove;
        private bool _homesTowardPlayer;
        private bool _persistsOnParry;
        private SpriteRenderer _normalGlow;
        private SpriteRenderer _normalCore;
        private SpriteRenderer _parryGlow;
        private SpriteRenderer _parryCore;
        private SpriteRenderer _parryRing;

        public ProjectileKind Kind => _kind;
        public string SourceMove => _sourceMove;
        public bool PersistsOnParry => _persistsOnParry;

        private static bool LineCircleHit(Vector2 start, Vector2 end, Vector2 center, float radius)
        {
            var delta = end - start;
            var lengthSq = delta.sqrMagnitude;
            if (lengthSq <= 0.0001f)
            {
                return (center - start).sqrMagnitude <= radius * radius;
            }

            var t = Mathf.Clamp01(Vector2.Dot(center - start, delta) / lengthSq);
            var point = start + delta * t;
            return (center - point).sqrMagnitude <= radius * radius;
        }

        protected override void BuildVisuals()
        {
            _normalGlow = PrototypeVisualUtility.EnsureSpriteChild(transform, "NormalGlow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.EnemyBlue.WithAlpha(0.12f), 9);
            _normalCore = PrototypeVisualUtility.EnsureSpriteChild(transform, "NormalCore", PrototypeVisualUtility.CircleSprite, Color.white.WithAlpha(0.82f), 11);
            _parryGlow = PrototypeVisualUtility.EnsureSpriteChild(transform, "ParryGlow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.ParryPurple.WithAlpha(0.14f), 9);
            _parryCore = PrototypeVisualUtility.EnsureSpriteChild(transform, "ParryCore", PrototypeVisualUtility.CircleSprite, Color.white.WithAlpha(0.9f), 11);
            _parryRing = PrototypeVisualUtility.EnsureSpriteChild(transform, "ParryRing", PrototypeVisualUtility.CircleSprite, Color.white.WithAlpha(0.12f), 8);
        }

        public void Spawn(
            Vector2 logicalPosition,
            Vector2 velocity,
            float radius,
            ProjectileKind kind,
            string sourceMove,
            float spin,
            float lifetime,
            bool homesTowardPlayer = false,
            float homingTurnRate = 0f,
            bool persistsOnParry = false)
        {
            gameObject.SetActive(true);
            LogicalPosition = logicalPosition;
            Velocity = velocity;
            Radius = radius;
            _kind = kind;
            _sourceMove = sourceMove;
            _spin = spin;
            _homesTowardPlayer = homesTowardPlayer;
            _homingSpeed = velocity.magnitude;
            _homingTurnRate = homingTurnRate;
            _persistsOnParry = persistsOnParry;
            _postParryGraceTimer = 0f;
            Lifetime = lifetime;
            Renderer.color = kind == ProjectileKind.Parry
                ? PrototypeVisualUtility.ParryPurple
                : PrototypeVisualUtility.EnemyBlue;
            var scale = radius / Game.Config.pixelsPerUnit * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);
            transform.localRotation = Quaternion.identity;
            SetVisualState(kind == ProjectileKind.Parry);

            SyncTransform();
        }

        public void MarkParried()
        {
            _postParryGraceTimer = Mathf.Max(_postParryGraceTimer, 0.2f);
        }

        private void SetVisualState(bool parry)
        {
            if (_normalGlow != null)
            {
                _normalGlow.enabled = !parry;
            }

            if (_normalCore != null)
            {
                _normalCore.enabled = !parry;
            }

            if (_parryGlow != null)
            {
                _parryGlow.enabled = parry;
            }

            if (_parryCore != null)
            {
                _parryCore.enabled = parry;
            }

            if (_parryRing != null)
            {
                _parryRing.enabled = parry;
            }
        }

        protected override void Tick(float dt)
        {
            var previousLogicalPosition = LogicalPosition;
            Lifetime -= dt;
            _postParryGraceTimer = Mathf.Max(0f, _postParryGraceTimer - dt);

            if (Mathf.Abs(_spin) > 0.01f)
            {
                var speed = Velocity.magnitude;
                var angle = Mathf.Atan2(Velocity.y, Velocity.x) + _spin * dt * 0.2f;
                Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            }

            if (_homesTowardPlayer && Game.Player != null)
            {
                var targetDirection = (Game.Player.LogicalPosition - LogicalPosition);
                if (targetDirection.sqrMagnitude > 0.001f)
                {
                    targetDirection.Normalize();
                    var currentDirection = Velocity.sqrMagnitude > 0.001f ? Velocity.normalized : targetDirection;
                    var rotatedDirection = Vector3.RotateTowards(
                        new Vector3(currentDirection.x, currentDirection.y, 0f),
                        new Vector3(targetDirection.x, targetDirection.y, 0f),
                        _homingTurnRate * Mathf.Deg2Rad * dt,
                        0f);
                    Velocity = new Vector2(rotatedDirection.x, rotatedDirection.y).normalized * Mathf.Max(_homingSpeed, 0.001f);
                }
            }

            LogicalPosition += Velocity * dt;
            if (_parryRing != null && _kind == ProjectileKind.Parry)
            {
                _parryRing.color = Color.white.WithAlpha(0.08f + Mathf.Sin(Time.time * 18f) * 0.02f);
            }

            if (IsOutOfBounds(-40f, Game.Config.logicalWidth + 40f, -50f, Game.Config.logicalHeight + 40f))
            {
                Game.DespawnProjectile(this);
                return;
            }

            if (Game.PlayerFieldController != null && Game.PlayerFieldController.TryResolveBossProjectile(this))
            {
                return;
            }

            var player = Game.Player;
            var collisionRadius = Radius +
                                  ((_kind == ProjectileKind.Parry && player.IsParrying)
                                      ? player.ParryRadius
                                      : player.HitboxRadius) - 1f;
            var hitCurrent = (player.LogicalPosition - LogicalPosition).sqrMagnitude <= collisionRadius * collisionRadius;
            var hitPlayerSweep = LineCircleHit(player.PreviousLogicalPosition, player.LogicalPosition, LogicalPosition, collisionRadius);
            var hitPlayerSweepPrevBullet = LineCircleHit(player.PreviousLogicalPosition, player.LogicalPosition, previousLogicalPosition, collisionRadius);

            if (hitCurrent || hitPlayerSweep || hitPlayerSweepPrevBullet)
            {
                if (_kind == ProjectileKind.Parry && player.IsParrying && player.CanParryTarget(this))
                {
                    Game.Player.OnSuccessfulParry(this);
                    if (_persistsOnParry)
                    {
                        MarkParried();
                        SyncTransform();
                    }
                    else
                    {
                        Game.DespawnProjectile(this);
                    }
                    return;
                }

                if (_persistsOnParry)
                {
                    if (_postParryGraceTimer > 0f || player.IsInvulnerable)
                    {
                        SyncTransform();
                        return;
                    }

                    player.TakeDamage(LogicalPosition);
                    SyncTransform();
                    return;
                }

                player.TakeDamage(LogicalPosition);
                Game.DespawnProjectile(this);
                return;
            }

            SyncTransform();
        }
    }
}
