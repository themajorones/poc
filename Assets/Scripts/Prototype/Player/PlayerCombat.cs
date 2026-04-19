using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        private GameController _game;
        private float _shotTimer;

        public void Initialize(GameController game)
        {
            _game = game;
            ResetState();
        }

        public void ResetState()
        {
            _shotTimer = 0f;
        }

        private void Update()
        {
            if (_game == null || !_game.IsPlaying || _game.SkillController.BlocksAutoShot || !_game.AllowPlayerAutoShot)
            {
                return;
            }

            var shotDefinition = _game.PlayerLoadout != null ? _game.PlayerLoadout.PrimaryShot : null;
            if (shotDefinition == null)
            {
                return;
            }

            _shotTimer -= Time.deltaTime;
            var shotInterval = shotDefinition.Interval;
            while (_shotTimer <= 0f)
            {
                _shotTimer += shotInterval;
                var shotOrigin = _game.Player.LogicalPosition + shotDefinition.SpawnOffset;
                var firePoint = _game.Player.FirePointRig;
                if (firePoint != null && firePoint.PrimaryShotPoint != null)
                {
                    shotOrigin = _game.References.WorldToLogical(firePoint.PrimaryShotPoint.position);
                }

                if (shotDefinition.ShotKind == PlayerShotKind.Spreadshot)
                {
                    SpawnSpreadShot(shotOrigin, shotDefinition);
                }
                else
                {
                    _game.SpawnPlayerProjectile(shotOrigin, null, shotDefinition);
                }

                AudioManager.Instance?.PlaySfx(shotDefinition.FireCueId);
            }
        }

        private void SpawnSpreadShot(Vector2 shotOrigin, PlayerShotDefinition shotDefinition)
        {
            var pelletCount = shotDefinition.SpreadPelletCount;
            var totalAngle = shotDefinition.SpreadAngleDegrees;
            for (var i = 0; i < pelletCount; i++)
            {
                var t = pelletCount == 1 ? 0.5f : i / (float)(pelletCount - 1);
                var angle = -totalAngle * 0.5f + totalAngle * t;
                var radians = angle * Mathf.Deg2Rad;
                var forward = new Vector2(Mathf.Sin(radians), -Mathf.Cos(radians)).normalized;
                _game.SpawnPlayerProjectile(shotOrigin, forward * shotDefinition.Speed, shotDefinition);
            }
        }
    }
}
