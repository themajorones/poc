using System.Collections.Generic;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerView view;
        [SerializeField] private PlayerFirePointRig firePointRig;
        [SerializeField] private PlayerAuthoring authoring;

        private GameController _game;
        private Vector2 _logicalPosition;
        private Vector2 _targetPosition;
        private Vector2 _lastPosition;
        private Vector2 _burstFrom;
        private Vector2 _burstTo;
        private float _invuln;
        private float _parryTimer;
        private float _parryGraceTimer;
        private float _burstHoldTimer;
        private float _burstTimer;
        private float _afterimageTimer;
        private float _hitFlash;
        private readonly Dictionary<int, float> _parryTargetCooldowns = new();
        private readonly List<int> _expiredParryTargets = new();

        public Vector2 LogicalPosition => _logicalPosition;
        public Vector2 PreviousLogicalPosition => _lastPosition;
        public Vector2 Velocity { get; private set; }
        public int CurrentHp { get; private set; }
        public bool IsParrying => _burstHoldTimer > 0f || _burstTimer > 0f || _parryGraceTimer > 0f;
        public bool IsBursting => _burstHoldTimer > 0f || _burstTimer > 0f;
        public bool IsInvulnerable => _invuln > 0f || _game.SkillController.GrantsActiveInvulnerability || _game.SkillController.PostSkillInvuln > 0f;
        public bool SkillActive => _game.SkillController.IsCasting;
        public bool ShowLaserVisual => _game.SkillController.UsesLaserVisual;
        public PlayerFirePointRig FirePointRig => firePointRig;
        public float HitboxRadius => authoring != null ? authoring.GetHitboxRadius(_game.Config) : _game.Config.player.hitboxRadius;
        public float ParryRadius => authoring != null ? authoring.GetParryRadius(_game.Config) : _game.Config.player.parryOuterRadius;
        public float HitFlash => _hitFlash;

        public void Initialize(GameController game)
        {
            _game = game;
            if (view == null)
            {
                view = GetComponent<PlayerView>();
            }

            if (firePointRig == null)
            {
                firePointRig = GetComponent<PlayerFirePointRig>();
            }

            if (authoring == null)
            {
                authoring = GetComponent<PlayerAuthoring>();
            }

            view.Build();
            ResetState();
        }

        public void ResetState()
        {
            var startPosition = new Vector2(_game.Config.logicalWidth * 0.5f, _game.Config.logicalHeight * 0.79f);
            _logicalPosition = startPosition;
            _targetPosition = startPosition;
            _lastPosition = startPosition;
            _burstFrom = startPosition;
            _burstTo = startPosition;
            _invuln = 0f;
            _parryTimer = 0f;
            _parryGraceTimer = 0f;
            _burstHoldTimer = 0f;
            _burstTimer = 0f;
            _afterimageTimer = 0f;
            _hitFlash = 0f;
            _parryTargetCooldowns.Clear();
            _expiredParryTargets.Clear();
            CurrentHp = _game.CurrentPlayerMaxHp;
            SyncTransform();
        }

        public void LockToCurrentPosition()
        {
            _targetPosition = _logicalPosition;
        }

        public void TriggerParryBurst()
        {
            var parry = _game.Config.parry;
            _parryTimer = parry.parryWindow;
            _burstHoldTimer = parry.burstHold;
            _burstTimer = parry.burstDuration;
            _burstFrom = _logicalPosition;
            _burstTo = _game.PlayerInput.TargetLogicalPosition;
            _game.VfxPoolController.SpawnAfterimage(view.BodyRenderer, _logicalPosition, new Color(0.7f, 0.56f, 1f, 0.75f), 0.14f, 1f);
        }

        public void TakeDamage(Vector2 logicalHitPosition)
        {
            if (IsInvulnerable || _game.State != GameController.RunState.Playing)
            {
                return;
            }

            CurrentHp = Mathf.Max(0, CurrentHp - 1);
            _invuln = _game.Config.player.hitInvuln;
            _hitFlash = 0.18f;
            _game.ParryFeedbackController.PlayPlayerHit(logicalHitPosition);
            _game.HudController.RefreshState();

            if (CurrentHp <= 0)
            {
                _game.TriggerLose();
            }
        }

        public void HealOne()
        {
            CurrentHp = Mathf.Min(_game.CurrentPlayerMaxHp, CurrentHp + 1);
            _game.HudController.RefreshState();
        }

        public void OnSuccessfulParry(BossProjectile projectile)
        {
            _game.ParryFeedbackController.PlayParrySuccess(projectile.Position, _logicalPosition);
            _game.TriggerParrySpecial(projectile.Position);
            _game.RageSystem.Add(_game.Config.parry.rageGain);
            _invuln = Mathf.Max(_invuln, _game.Config.parry.successInvuln);
            RegisterParryTarget(projectile);
            _game.VfxPoolController.SpawnAfterimage(view.BodyRenderer, _logicalPosition, new Color(0.83f, 0.6f, 1f, 0.9f), 0.12f, 1.1f);
            _game.NotifyParrySucceeded(projectile);
        }

        public void OnSuccessfulWaveParry(Vector2 logicalHitPosition)
        {
            _game.ParryFeedbackController.PlayParrySuccess(logicalHitPosition, _logicalPosition);
            _game.TriggerParrySpecial(logicalHitPosition);
            _game.RageSystem.Add(_game.Config.parry.rageGain);
            _invuln = Mathf.Max(_invuln, _game.Config.parry.successInvuln);
            _game.VfxPoolController.SpawnAfterimage(view.BodyRenderer, _logicalPosition, new Color(0.83f, 0.6f, 1f, 0.82f), 0.12f, 1.05f);
        }

        public bool CanParryTarget(Object target)
        {
            if (target == null)
            {
                return true;
            }

            var id = target.GetInstanceID();
            return !_parryTargetCooldowns.TryGetValue(id, out var untilTime) || untilTime <= Time.time;
        }

        public void RegisterParryTarget(Object target)
        {
            if (target == null)
            {
                return;
            }

            _parryTargetCooldowns[target.GetInstanceID()] = Time.time + _game.Config.parry.sameTargetRepeatCooldown;
        }

        private void Update()
        {
            if (_game == null || _game.State != GameController.RunState.Playing)
            {
                return;
            }

            var dt = Time.deltaTime;
            _invuln = Mathf.Max(0f, _invuln - dt);
            _hitFlash = Mathf.Max(0f, _hitFlash - dt);
            _parryTimer = Mathf.Max(0f, _parryTimer - dt);
            _parryGraceTimer = Mathf.Max(0f, _parryGraceTimer - dt);
            _afterimageTimer = Mathf.Max(0f, _afterimageTimer - dt);
            _lastPosition = _logicalPosition;
            CleanupParryTargetCooldowns();

            var wasHoldingBurst = _burstHoldTimer > 0f;
            _burstHoldTimer = Mathf.Max(0f, _burstHoldTimer - dt);

            if (wasHoldingBurst)
            {
                _burstTo = _game.PlayerInput.TargetLogicalPosition;
                _logicalPosition = _burstFrom;
                if (_burstHoldTimer <= 0f)
                {
                    _game.VfxPoolController.SpawnAfterimage(view.BodyRenderer, _logicalPosition, new Color(0.83f, 0.68f, 1f, 0.85f), 0.08f, 1.05f);
                }
            }
            else if (_burstTimer > 0f)
            {
                // Keep A fixed, but pull the active dash toward the live pointer target.
                _burstTo = _game.PlayerInput.TargetLogicalPosition;
                var burstTimeRemaining = Mathf.Max(0.0001f, _burstTimer);
                var burstStep = Mathf.Min(1f, dt / burstTimeRemaining);
                _logicalPosition = Vector2.Lerp(_logicalPosition, _burstTo, burstStep);
                _burstTimer = Mathf.Max(0f, _burstTimer - dt);
                if (_afterimageTimer <= 0f)
                {
                    _afterimageTimer = _game.Config.vfx.afterimageInterval;
                    _game.VfxPoolController.SpawnAfterimage(view.BodyRenderer, _logicalPosition + new Vector2(0f, 10f), new Color(0.65f, 0.55f, 1f, 0.45f), 0.09f, 1f);
                }
            }
            else if (!_game.SkillController.LocksMovement)
            {
                _targetPosition = _game.PlayerInput.TargetLogicalPosition;
                _logicalPosition = Vector2.Lerp(_logicalPosition, _targetPosition, _game.Config.player.moveLerp);
            }

            _logicalPosition = _game.References.ClampLogicalPosition(_logicalPosition, 20f, 16f, 24f);
            Velocity = (_logicalPosition - _lastPosition) / Mathf.Max(0.0001f, dt);

            if (_burstTimer > 0f && Velocity.y < -_game.Config.parry.activeMinUpSpeed)
            {
                _parryGraceTimer = _game.Config.parry.postGrace;
            }

            SyncTransform();
            view.ApplyState(this);
        }

        private void CleanupParryTargetCooldowns()
        {
            if (_parryTargetCooldowns.Count == 0)
            {
                return;
            }

            _expiredParryTargets.Clear();
            foreach (var pair in _parryTargetCooldowns)
            {
                if (pair.Value <= Time.time)
                {
                    _expiredParryTargets.Add(pair.Key);
                }
            }

            for (var i = 0; i < _expiredParryTargets.Count; i++)
            {
                _parryTargetCooldowns.Remove(_expiredParryTargets[i]);
            }
        }

        private void SyncTransform()
        {
            transform.position = _game.References.LogicalToWorld(_logicalPosition);
        }
    }
}
