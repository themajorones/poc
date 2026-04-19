using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class ParryFeedbackController : MonoBehaviour
    {
        private GameController _game;
        private float _hitStopUntil;
        private float _hitStopScale = 1f;

        public void Initialize(GameController game)
        {
            _game = game;
            _hitStopUntil = 0f;
            _hitStopScale = 1f;
        }

        public void PlayParrySuccess(Vector2 bulletPosition, Vector2 playerPosition)
        {
            var parryEffect = _game.PlayerLoadout != null ? _game.PlayerLoadout.ParryEffect : null;
            AudioManager.Instance?.PlaySfx(parryEffect != null ? parryEffect.SuccessCueId : AudioCue.ParrySuccess.ToString());
            _game.ScreenShakeController.Bump(parryEffect != null ? parryEffect.ShakeIntensity : 0.08f, parryEffect != null ? parryEffect.ShakeDuration : 0.08f);
            RequestHitStop(parryEffect != null ? parryEffect.HitStopDuration : 0.02f, parryEffect != null ? parryEffect.HitStopScale : 0.05f);
            _game.VfxPoolController.SpawnRing(playerPosition, parryEffect != null ? parryEffect.PlayerRingColor : PrototypeVisualUtility.ParryPurple.WithAlpha(0.5f), parryEffect != null ? parryEffect.PlayerRingRadius : 0.14f, parryEffect != null ? parryEffect.PlayerRingThickness : 0.1f, parryEffect != null ? parryEffect.PlayerRingDuration : 0.32f);
            _game.VfxPoolController.SpawnRing(bulletPosition, parryEffect != null ? parryEffect.BulletRingColor : PrototypeVisualUtility.CounterGold.WithAlpha(0.5f), parryEffect != null ? parryEffect.BulletRingRadius : 0.16f, parryEffect != null ? parryEffect.BulletRingThickness : 0.08f, parryEffect != null ? parryEffect.BulletRingDuration : 0.34f);
            _game.VfxPoolController.SpawnBurst(bulletPosition, parryEffect != null ? parryEffect.BulletBurstColor : new Color(1f, 0.82f, 0.45f, 1f), parryEffect != null ? parryEffect.BulletBurstCount : 10, parryEffect != null ? parryEffect.BulletBurstRadius : 0.24f);
            _game.VfxPoolController.SpawnBurst(playerPosition, parryEffect != null ? parryEffect.PlayerBurstColor : new Color(0.83f, 0.6f, 1f, 1f), parryEffect != null ? parryEffect.PlayerBurstCount : 4, parryEffect != null ? parryEffect.PlayerBurstRadius : 0.16f);
        }

        public void PlayCounterHit(Vector2 logicalPosition)
        {
            var counterDefinition = _game.PlayerLoadout != null ? _game.PlayerLoadout.CounterShot : null;
            AudioManager.Instance?.PlaySfx(counterDefinition != null ? counterDefinition.HitCueId : AudioCue.CounterHit.ToString());
            _game.ScreenShakeController.Bump(0.05f, 0.06f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, new Color(1f, 0.7f, 0.32f, 1f), 8, 0.22f);
        }

        public void PlayBossBreak(Vector2 logicalPosition)
        {
            AudioManager.Instance?.PlaySfx(AudioCue.BossBreak);
            _game.ScreenShakeController.Bump(0.16f, 0.16f);
            RequestHitStop(0.03f, 0.05f);
            _game.VfxPoolController.SpawnRing(logicalPosition, PrototypeVisualUtility.WeakGold.WithAlpha(0.65f), 0.28f, 0.28f, 0.9f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, new Color(1f, 0.84f, 0.48f, 1f), 30, 0.7f);
        }

        public void PlayPlayerHit(Vector2 logicalPosition)
        {
            AudioManager.Instance?.PlaySfx(AudioCue.PlayerHit);
            _game.ScreenShakeController.Bump(0.09f, 0.12f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, new Color(1f, 0.48f, 0.54f, 1f), 12, 0.35f);
        }

        public void PlaySkillCast(Vector2 logicalPosition)
        {
            var skillDefinition = _game.PlayerLoadout != null ? _game.PlayerLoadout.Skill : null;
            AudioManager.Instance?.PlaySfx(skillDefinition != null ? skillDefinition.CastCueId : AudioCue.SkillCast.ToString());
            _game.ScreenShakeController.Bump(skillDefinition != null ? skillDefinition.CastShakeIntensity : 0.1f, skillDefinition != null ? skillDefinition.CastShakeDuration : 0.12f);
            RequestHitStop(skillDefinition != null ? skillDefinition.CastHitStopDuration : 0.02f, skillDefinition != null ? skillDefinition.CastHitStopScale : 0.04f);
            if (skillDefinition != null && skillDefinition.SkillKind == PlayerSkillKind.Laser)
            {
                _game.VfxPoolController.SpawnBurst(logicalPosition, skillDefinition.LaserCastBurstColor, skillDefinition.LaserCastBurstCount, skillDefinition.LaserCastBurstRadius);
                _game.VfxPoolController.SpawnBurst(logicalPosition, skillDefinition.LaserCastBurstSecondaryColor, skillDefinition.LaserCastBurstSecondaryCount, skillDefinition.LaserCastBurstSecondaryRadius);
                return;
            }

            _game.VfxPoolController.SpawnRing(logicalPosition, skillDefinition != null ? skillDefinition.CastRingColor : new Color(0.98f, 0.8f, 0.4f, 0.72f), skillDefinition != null ? skillDefinition.CastRingRadius : 0.75f, skillDefinition != null ? skillDefinition.CastRingThickness : 0.12f, skillDefinition != null ? skillDefinition.CastRingDuration : 0.92f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, skillDefinition != null ? skillDefinition.CastBurstColor : new Color(1f, 0.84f, 0.42f, 0.92f), skillDefinition != null ? skillDefinition.CastBurstCount : 8, skillDefinition != null ? skillDefinition.CastBurstRadius : 0.34f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, skillDefinition != null ? skillDefinition.CastBurstSecondaryColor : new Color(1f, 0.95f, 0.74f, 0.86f), skillDefinition != null ? skillDefinition.CastBurstSecondaryCount : 4, skillDefinition != null ? skillDefinition.CastBurstSecondaryRadius : 0.22f);
        }

        public void PlayRecoveryPickup(Vector2 logicalPosition)
        {
            _game.VfxPoolController.SpawnRing(logicalPosition, PrototypeVisualUtility.HealMint.WithAlpha(0.52f), 0.22f, 0.14f, 0.5f);
            _game.VfxPoolController.SpawnBurst(logicalPosition, new Color(0.61f, 0.95f, 0.78f, 1f), 12, 0.3f);
        }

        private void RequestHitStop(float duration, float minimumScale)
        {
            if (!isActiveAndEnabled || duration <= 0f)
            {
                return;
            }

            _hitStopUntil = Mathf.Max(_hitStopUntil, Time.unscaledTime + duration);
            _hitStopScale = Mathf.Min(_hitStopScale, minimumScale);
            Time.timeScale = _hitStopScale;
        }

        private void Update()
        {
            if (_hitStopUntil <= 0f)
            {
                return;
            }

            if (Time.unscaledTime < _hitStopUntil)
            {
                Time.timeScale = _hitStopScale;
                return;
            }

            _hitStopUntil = 0f;
            _hitStopScale = 1f;
            Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            _hitStopUntil = 0f;
            _hitStopScale = 1f;
            Time.timeScale = 1f;
        }
    }
}
