using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossShockwaveRing : MonoBehaviour
    {
        private GameController _game;
        private PlayerFieldRingVisual _visual;
        private Vector2 _centerLogical;
        private float _radius;
        private float _previousRadius;
        private float _expandSpeed;
        private float _thickness;
        private float _maxRadius;
        private bool _resolvedAgainstPlayer;
        private float _lastRenderedRadius = -1f;

        public bool Finished { get; private set; }
        public Vector2 CenterLogical => _centerLogical;
        public float Radius => _radius;
        public float Thickness => _thickness;

        public void Initialize(GameController game, Vector2 centerLogical, float startRadius, float expandSpeed, float thickness, float maxRadius)
        {
            _game = game;
            _centerLogical = centerLogical;
            _radius = startRadius;
            _previousRadius = startRadius;
            _expandSpeed = expandSpeed;
            _thickness = thickness;
            _maxRadius = maxRadius;
            _resolvedAgainstPlayer = false;
            Finished = false;
            _visual ??= game.PoolController.SpawnRingVisual(game.References.LogicalToWorld(centerLogical));
            if (_visual != null)
            {
                _visual.SetSortingOrder(15);
            }
            RenderRing(force: true);
        }

        public void ManagedTick(float dt)
        {
            if (Finished || _game == null)
            {
                return;
            }

            _previousRadius = _radius;
            _radius += _expandSpeed * dt;
            ResolveAgainstPlayer();
            if (_radius >= _maxRadius)
            {
                Finished = true;
            }

            RenderRing(force: false);
        }

        private void ResolveAgainstPlayer()
        {
            if (_resolvedAgainstPlayer)
            {
                return;
            }

            if (_game.PlayerFieldController != null && _game.PlayerFieldController.TryResolveShockwave(this, Vector2.zero))
            {
                _resolvedAgainstPlayer = true;
                return;
            }

            var player = _game.Player;
            var currentDelta = player.LogicalPosition - _centerLogical;
            var previousDelta = player.PreviousLogicalPosition - _centerLogical;
            var distance = currentDelta.magnitude;
            var previousDistance = previousDelta.magnitude;
            var bandHalfThickness = _thickness * 0.5f + _game.Config.player.hitboxRadius;
            var distanceMin = Mathf.Min(previousDistance, distance);
            var distanceMax = Mathf.Max(previousDistance, distance);
            var ringMin = Mathf.Min(_previousRadius, _radius) - bandHalfThickness;
            var ringMax = Mathf.Max(_previousRadius, _radius) + bandHalfThickness;
            var crossedShockwaveBand = distanceMax >= ringMin && distanceMin <= ringMax;
            if (!crossedShockwaveBand && _radius - bandHalfThickness <= distance)
            {
                return;
            }

            var normal = distance > 0.01f ? currentDelta / distance : Vector2.up;
            var hitPoint = _centerLogical + normal * _radius;
            if (crossedShockwaveBand && player.IsParrying)
            {
                _resolvedAgainstPlayer = true;
                player.OnSuccessfulWaveParry(hitPoint);
                return;
            }

            if (_radius - bandHalfThickness <= distance)
            {
                return;
            }

            _resolvedAgainstPlayer = true;
            _game.Player.TakeDamage(hitPoint);
        }

        public void ConsumeBySkillParry()
        {
            _resolvedAgainstPlayer = true;
            Finished = true;
        }

        private void OnDestroy()
        {
            if (_game != null && _visual != null)
            {
                _visual.Clear();
                _game.PoolController.Despawn(_visual.transform);
            }

            _visual = null;
        }

        private void RenderRing(bool force)
        {
            if (!force && Mathf.Abs(_radius - _lastRenderedRadius) < 4f)
            {
                return;
            }

            _lastRenderedRadius = _radius;
            var normalizedAge = Mathf.InverseLerp(0f, Mathf.Max(1f, _maxRadius), _radius);
            var state = _radius < _thickness * 1.4f ? RingVisualState.Telegraph : RingVisualState.Active;
            _visual?.Render(
                _game.References,
                _centerLogical,
                _radius,
                _thickness,
                new Color(0.84f, 0.62f, 1f, 0.95f),
                RingVisualStyle.BossShockwaveResolved,
                state,
                normalizedAge);
        }
    }
}
