using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class SkillController : MonoBehaviour
    {
        private GameController _game;
        private float _skillTimer;
        private float _postSkillInvuln;
        private PlayerSkillDefinition _activeSkillDefinition;

        public bool IsCasting => _skillTimer > 0f;
        public float SkillTimer => _skillTimer;
        public float PostSkillInvuln => _postSkillInvuln;
        public PlayerSkillKind ActiveSkillKind => _activeSkillDefinition != null ? _activeSkillDefinition.SkillKind : PlayerSkillKind.Laser;
        public bool LocksMovement => IsCasting && _activeSkillDefinition != null && _activeSkillDefinition.LockMovementWhileCasting;
        public bool BlocksAutoShot => IsCasting && _activeSkillDefinition != null && _activeSkillDefinition.BlocksAutoShotWhileCasting;
        public bool GrantsActiveInvulnerability => IsCasting && _activeSkillDefinition != null && _activeSkillDefinition.GrantsActiveInvulnerabilityWhileCasting;
        public bool UsesLaserVisual => IsCasting && _activeSkillDefinition != null && ActiveSkillKind == PlayerSkillKind.Laser;

        public void Initialize(GameController game)
        {
            _game = game;
        }

        public void ResetState()
        {
            _skillTimer = 0f;
            _postSkillInvuln = 0f;
            _activeSkillDefinition = null;
        }

        public bool TryActivateSkill()
        {
            if (!_game.IsPlaying || !_game.RageSystem.IsFull || IsCasting)
            {
                return false;
            }

            _activeSkillDefinition = _game.PlayerLoadout != null ? _game.PlayerLoadout.Skill : null;
            if (_activeSkillDefinition == null)
            {
                return false;
            }

            _game.RageSystem.ConsumeAll();
            var skillDefinition = _activeSkillDefinition;
            _skillTimer = skillDefinition.Duration;
            _postSkillInvuln = skillDefinition.RecoveryInvuln;
            if (skillDefinition.LockMovementWhileCasting)
            {
                _game.Player.LockToCurrentPosition();
            }

            if (skillDefinition.SkillKind == PlayerSkillKind.ExpandingGlobalRing)
            {
                _game.PlayerFieldController?.SpawnGlobalRing(_game.Player.LogicalPosition, skillDefinition);
            }
            else if (skillDefinition.SkillKind == PlayerSkillKind.StickyProjectile)
            {
                var shotOrigin = _game.Player.LogicalPosition;
                var firePoint = _game.Player.FirePointRig;
                if (firePoint != null && firePoint.PrimaryShotPoint != null)
                {
                    shotOrigin = _game.References.WorldToLogical(firePoint.PrimaryShotPoint.position);
                }

                _game.SpawnPlayerSkillProjectile(shotOrigin, skillDefinition);
            }

            _game.ParryFeedbackController.PlaySkillCast(_game.Player.LogicalPosition);
            _game.NotifySkillActivated();
            return true;
        }

        private void Update()
        {
            if (_game == null || !_game.IsPlaying)
            {
                return;
            }

            if (_skillTimer > 0f)
            {
                _skillTimer = Mathf.Max(0f, _skillTimer - Time.deltaTime);
                if (ActiveSkillKind == PlayerSkillKind.Laser)
                {
                    ApplyLaserDamage(Time.deltaTime);
                }

                if (_skillTimer <= 0f)
                {
                    _activeSkillDefinition = null;
                }
            }
            else
            {
                _postSkillInvuln = Mathf.Max(0f, _postSkillInvuln - Time.deltaTime);
            }
        }

        private void ApplyLaserDamage(float dt)
        {
            if (_game.Boss == null || !_game.Boss.Active || _activeSkillDefinition == null)
            {
                return;
            }

            var skillDefinition = _activeSkillDefinition;
            var laneAllowance = skillDefinition.LaneHitAllowance;
            if (Mathf.Abs(_game.Player.LogicalPosition.x - _game.Boss.LogicalPosition.x) > laneAllowance)
            {
                return;
            }

            _game.Boss.ApplyDamage(
                skillDefinition.LaserDps * dt,
                skillDefinition.LaserPoiseDps * dt,
                _game.Boss.LogicalPosition);
            _game.RageSystem.Add(skillDefinition.RageGainPerSecond * dt);
            _game.ScreenShakeController.Bump(0.035f, 0.06f);
        }
    }
}
