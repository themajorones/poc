using DG.Tweening;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossController : MonoBehaviour
    {
        public enum BossPhase
        {
            Rest,
            Telegraph,
            Attack,
            Break
        }

        [SerializeField] private BossView view;
        [SerializeField] private BossFirePointRig firePointRig;
        [SerializeField] private BossAuthoring authoring;

        private GameController _game;
        private string _bossName;
        private int _bossIndex;
        private float _phaseTimer;
        private int _moveIndex;
        private float _damageFlash;
        private float _anchorX;
        private Vector2 _logicalPosition;
        private Vector2 _previousLogicalPosition;
        private bool _scriptedMotionActive;
        private Vector2 _scriptedLogicalPosition;
        private bool _forcedRecoverActive;
        private Vector2 _forcedRecoverStart;
        private Vector2 _forcedRecoverTarget;
        private float _forcedRecoverDuration;
        private float _forcedRecoverTimer;
        private float _laneVelocity;
        private float _verticalVelocity;
        private float _bobBlend;

        public bool Active { get; private set; }
        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }
        public float CurrentPoise { get; private set; }
        public float MaxPoise { get; private set; }
        public string BossName => _bossName ?? string.Empty;
        public int BossIndex => _bossIndex;
        public Vector2 LogicalPosition => _logicalPosition;
        public Vector2 PreviousLogicalPosition => _previousLogicalPosition;
        public BossPhase Phase { get; set; }
        public float BreakTimer { get; private set; }
        public string CurrentMove { get; set; }
        public int MoveIndex => _moveIndex;
        public float PhaseTimer { get => _phaseTimer; set => _phaseTimer = value; }
        public float AnchorX { get => _anchorX; set => _anchorX = value; }
        public float DamageFlash => _damageFlash;
        public bool IsRecoveringPoise { get; private set; }
        public BossFirePointRig FirePointRig => firePointRig;
        public BossAuthoring Authoring => authoring;
        public Vector2 ContactHalfExtents => authoring != null ? authoring.GetContactHalfExtents(_game.Config) : new Vector2(_game.Config.boss.contactRadiusX, _game.Config.boss.contactRadiusY);
        public Vector2 ContactCenterLogical
        {
            get
            {
                if (authoring != null && authoring.BodyRenderer != null)
                {
                    return _game.References.WorldToLogical(authoring.BodyRenderer.bounds.center);
                }

                return _logicalPosition;
            }
        }
        public Vector2 PreviousContactCenterLogical
        {
            get
            {
                var contactOffset = ContactCenterLogical - _logicalPosition;
                return _previousLogicalPosition + contactOffset;
            }
        }
        public bool CanDealContactDamage => Active && BreakTimer <= 0f && Phase != BossPhase.Break && CurrentPoise > 0.01f;

        public void Initialize(GameController game)
        {
            _game = game;
            if (view == null)
            {
                view = GetComponent<BossView>();
            }

            if (firePointRig == null)
            {
                firePointRig = GetComponent<BossFirePointRig>();
            }

            if (authoring == null)
            {
                authoring = GetComponent<BossAuthoring>();
            }

            view.Build(game);
            Active = false;
            gameObject.SetActive(false);
        }

        public void ApplyDefinition(int bossIndex, string bossName, float hp, float poise, bool immediate)
        {
            _bossName = bossName;
            _bossIndex = bossIndex;
            Active = true;
            _scriptedMotionActive = false;
            _forcedRecoverActive = false;
            _laneVelocity = 0f;
            _verticalVelocity = 0f;
            _bobBlend = 1f;
            CurrentHp = MaxHp = hp;
            CurrentPoise = MaxPoise = poise;
            _logicalPosition = new Vector2(_game.Config.logicalWidth * 0.5f, _game.Config.boss.bossY);
            _previousLogicalPosition = _logicalPosition;
            _anchorX = _logicalPosition.x;
            _moveIndex = 0;
            Phase = BossPhase.Rest;
            _phaseTimer = immediate ? 0.9f : 1f;
            CurrentMove = null;
            BreakTimer = 0f;
            _damageFlash = 0f;
            IsRecoveringPoise = false;
            gameObject.SetActive(true);
            SyncTransform();
        }

        public void Deactivate()
        {
            Active = false;
            _scriptedMotionActive = false;
            _forcedRecoverActive = false;
            _laneVelocity = 0f;
            _verticalVelocity = 0f;
            CurrentMove = null;
            BreakTimer = 0f;
            IsRecoveringPoise = false;
            gameObject.SetActive(false);
        }

        public void ApplyDamage(float hpDamage, float poiseDamage, Vector2 hitLogicalPosition)
        {
            if (!Active)
            {
                return;
            }

            CurrentHp = Mathf.Max(0f, CurrentHp - hpDamage);
            _damageFlash = 0.12f;

            if (BreakTimer <= 0f)
            {
                CurrentPoise = Mathf.Max(0f, CurrentPoise - poiseDamage);
                if (CurrentPoise <= 0f)
                {
                    StartBreak();
                }
            }

            _game.VfxPoolController.SpawnBurst(hitLogicalPosition, new Color(1f, 0.96f, 0.74f, 1f), 5, 0.18f);
            _game.HudController.RefreshState();

            if (CurrentHp <= 0f)
            {
                CurrentHp = 0f;
                _game.BossRushController.OnBossDefeated();
            }
        }

        public void StartBreak()
        {
            _scriptedMotionActive = false;
            _bobBlend = 0f;
            CurrentPoise = 0f;
            BreakTimer = _game.Config.boss.breakDuration;
            Phase = BossPhase.Break;
            _phaseTimer = 0f;
            CurrentMove = null;
            IsRecoveringPoise = true;
            _game.ClearProjectiles(true);
            _game.ParryFeedbackController.PlayBossBreak(_logicalPosition);
            _game.OverlayController.ShowBanner("BREAK", "Weak zone exposed");
            _game.HudController.RefreshState();
        }

        public void BeginForcedRecover(Vector2 targetLogicalPosition, float duration)
        {
            _forcedRecoverActive = true;
            _forcedRecoverStart = _logicalPosition;
            _forcedRecoverTarget = targetLogicalPosition;
            _forcedRecoverDuration = Mathf.Max(0.01f, duration);
            _forcedRecoverTimer = 0f;
            _bobBlend = 0f;
        }

        public void CheckContactDamage()
        {
            if (!CanDealContactDamage)
            {
                return;
            }

            if (string.Equals(CurrentMove, "parryCharge", System.StringComparison.Ordinal))
            {
                return;
            }

            var player = _game.Player;
            var contactHalfExtents = authoring != null ? authoring.GetContactHalfExtents(_game.Config) : new Vector2(_game.Config.boss.contactRadiusX, _game.Config.boss.contactRadiusY);
            var contactCenter = ContactCenterLogical;
            if (IntersectsPlayerSweep(PreviousContactCenterLogical, contactCenter, contactHalfExtents, player.HitboxRadius * 0.65f))
            {
                player.TakeDamage(player.LogicalPosition);
            }
        }

        public Vector2 GetWeakZoneLogicalPosition()
        {
            if (firePointRig != null && firePointRig.WeakZoneAnchor != null)
            {
                return _game.References.WorldToLogical(firePointRig.WeakZoneAnchor.position);
            }

            if (authoring != null && authoring.WeakZoneAnchor != null)
            {
                return _game.References.WorldToLogical(authoring.WeakZoneAnchor.position);
            }

            if (authoring != null && authoring.WeakZoneRenderer != null)
            {
                return _game.References.WorldToLogical(authoring.WeakZoneRenderer.transform.position);
            }

            return _logicalPosition + new Vector2(0f, 58f);
        }

        public void AdvanceMoveIndex()
        {
            _moveIndex += 1;
        }

        public void SetLogicalPosition(Vector2 logicalPosition)
        {
            _previousLogicalPosition = _logicalPosition;
            _logicalPosition = logicalPosition;
            SyncTransform();
        }

        public void SetScriptedMotion(Vector2 logicalPosition)
        {
            _scriptedMotionActive = true;
            _scriptedLogicalPosition = logicalPosition;
            _previousLogicalPosition = _logicalPosition;
            _logicalPosition = logicalPosition;
            _bobBlend = 0f;
            SyncTransform();
            if (view != null)
            {
                view.ApplyState(this);
            }
        }

        public void ClearScriptedMotion()
        {
            _scriptedMotionActive = false;
            _bobBlend = 0f;
        }

        private void Update()
        {
            if (_game == null || !_game.IsPlaying || !Active)
            {
                return;
            }

            var dt = Time.deltaTime;
            _damageFlash = Mathf.Max(0f, _damageFlash - dt);
            _previousLogicalPosition = _logicalPosition;

            if (_forcedRecoverActive)
            {
                _forcedRecoverTimer += dt;
                var recoverT = Mathf.Clamp01(_forcedRecoverTimer / _forcedRecoverDuration);
                var easedRecover = DOVirtual.EasedValue(0f, 1f, recoverT, Ease.OutSine);
                _logicalPosition = Vector2.Lerp(_forcedRecoverStart, _forcedRecoverTarget, easedRecover);

                if (BreakTimer > 0f)
                {
                    BreakTimer = Mathf.Max(0f, BreakTimer - dt);
                    CurrentPoise = Mathf.MoveTowards(CurrentPoise, MaxPoise, MaxPoise / Mathf.Max(0.01f, _game.Config.boss.breakDuration) * dt);
                    IsRecoveringPoise = CurrentPoise < MaxPoise - 0.01f;

                    if (BreakTimer <= 0f)
                    {
                        CurrentPoise = MaxPoise;
                        Phase = BossPhase.Rest;
                        _phaseTimer = _game.Config.boss.restTime;
                        CurrentMove = null;
                        IsRecoveringPoise = false;
                        _game.BossPatternController.ChooseNextAnchor();
                    }
                }

                if (recoverT >= 1f)
                {
                    _forcedRecoverActive = false;
                    _bobBlend = 0f;
                }

                SyncTransform();
                view.ApplyState(this);
                return;
            }

            if (_scriptedMotionActive)
            {
                _logicalPosition = _scriptedLogicalPosition;
                SyncTransform();
                view.ApplyState(this);
                return;
            }

            if (BreakTimer > 0f)
            {
                BreakTimer = Mathf.Max(0f, BreakTimer - dt);
                _logicalPosition.x = Mathf.SmoothDamp(_logicalPosition.x, _game.Config.logicalWidth * 0.5f, ref _laneVelocity, 0.22f, Mathf.Infinity, dt);
                _logicalPosition.y = Mathf.SmoothDamp(_logicalPosition.y, _game.Config.boss.bossY, ref _verticalVelocity, 0.18f, Mathf.Infinity, dt);
                CurrentPoise = Mathf.MoveTowards(CurrentPoise, MaxPoise, MaxPoise / Mathf.Max(0.01f, _game.Config.boss.breakDuration) * dt);
                IsRecoveringPoise = CurrentPoise < MaxPoise - 0.01f;

                if (BreakTimer <= 0f)
                {
                    CurrentPoise = MaxPoise;
                    Phase = BossPhase.Rest;
                    _phaseTimer = _game.Config.boss.restTime;
                    CurrentMove = null;
                    IsRecoveringPoise = false;
                    _game.BossPatternController.ChooseNextAnchor();
                }
            }
            else
            {
                var mobilityPreset = _game.BossRushController.GetMobilityPreset(_bossIndex);
                var laneSmoothTime = mobilityPreset switch
                {
                    BossMobilityPreset.NoMove => 999f,
                    BossMobilityPreset.Slow => 0.38f,
                    BossMobilityPreset.Normal => 0.28f,
                    BossMobilityPreset.Fast => 0.2f,
                    BossMobilityPreset.Fastest => 0.14f,
                    _ => 0.28f
                };
                _logicalPosition.x = mobilityPreset == BossMobilityPreset.NoMove
                    ? Mathf.SmoothDamp(_logicalPosition.x, _game.Config.logicalWidth * 0.5f, ref _laneVelocity, 0.28f, Mathf.Infinity, dt)
                    : Mathf.SmoothDamp(_logicalPosition.x, _anchorX, ref _laneVelocity, laneSmoothTime, Mathf.Infinity, dt);
                IsRecoveringPoise = false;
            }

            _bobBlend = Mathf.MoveTowards(_bobBlend, 1f, dt * 2.6f);
            var targetY = _game.Config.boss.bossY + Mathf.Sin(Time.time * 1.5f + _bossIndex * 0.7f) * 8f * _bobBlend;
            _logicalPosition.y = Mathf.SmoothDamp(_logicalPosition.y, targetY, ref _verticalVelocity, 0.18f, Mathf.Infinity, dt);
            var weakZone = GetWeakZoneLogicalPosition();
            var weakZoneRadius = authoring != null ? authoring.GetWeakZoneRadius(_game.Config) : _game.Config.boss.weakZoneRadius;
            if (BreakTimer > 0f &&
                (_game.Player.LogicalPosition - weakZone).sqrMagnitude <= Mathf.Pow(weakZoneRadius + _game.Player.HitboxRadius, 2f))
            {
                _game.RageSystem.Add(_game.Config.boss.weakZoneRagePerSecond * dt);
            }

            SyncTransform();
            view.ApplyState(this);
        }

        private void SyncTransform()
        {
            transform.position = _game.References.LogicalToWorld(_logicalPosition);
        }

        public bool IntersectsPlayerSweep(Vector2 previousCenter, Vector2 currentCenter, Vector2 halfExtents, float playerRadius)
        {
            var player = _game != null ? _game.Player : null;
            if (player == null)
            {
                return false;
            }

            var segmentStart = player.PreviousLogicalPosition;
            var segmentEnd = player.LogicalPosition;
            var min = Vector2.Min(previousCenter, currentCenter) - halfExtents - Vector2.one * playerRadius;
            var max = Vector2.Max(previousCenter, currentCenter) + halfExtents + Vector2.one * playerRadius;

            if (segmentStart.x >= min.x && segmentStart.x <= max.x &&
                segmentStart.y >= min.y && segmentStart.y <= max.y)
            {
                return true;
            }

            var delta = segmentEnd - segmentStart;
            var tMin = 0f;
            var tMax = 1f;

            if (!ClipAxis(segmentStart.x, delta.x, min.x, max.x, ref tMin, ref tMax))
            {
                return false;
            }

            if (!ClipAxis(segmentStart.y, delta.y, min.y, max.y, ref tMin, ref tMax))
            {
                return false;
            }

            return tMax >= tMin;
        }

        private static bool ClipAxis(float start, float delta, float min, float max, ref float tMin, ref float tMax)
        {
            if (Mathf.Abs(delta) <= 0.0001f)
            {
                return start >= min && start <= max;
            }

            var invDelta = 1f / delta;
            var t1 = (min - start) * invDelta;
            var t2 = (max - start) * invDelta;
            if (t1 > t2)
            {
                (t1, t2) = (t2, t1);
            }

            tMin = Mathf.Max(tMin, t1);
            tMax = Mathf.Min(tMax, t2);
            return tMax >= tMin;
        }
    }
}
