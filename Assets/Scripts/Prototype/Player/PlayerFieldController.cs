using System.Collections.Generic;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerFieldController : MonoBehaviour
    {
        private abstract class RingFieldState
        {
            private float _lastRenderedRadius = float.MinValue;
            protected readonly GameController Game;
            protected readonly PlayerFieldRingVisual Visual;
            protected readonly Vector2 CenterLogical;
            protected readonly float Thickness;
            protected readonly Color BaseColor;
            protected readonly RingVisualStyle Style;

            protected float Age;
            protected float CurrentRadius;

            public bool Finished { get; protected set; }
            public Vector2 Center => CenterLogical;

            protected RingFieldState(GameController game, PlayerFieldRingVisual visual, Vector2 centerLogical, float thickness, Color color, RingVisualStyle style)
            {
                Game = game;
                Visual = visual;
                CenterLogical = centerLogical;
                Thickness = thickness;
                BaseColor = color;
                Style = style;
            }

            public void Tick(float dt)
            {
                if (Finished)
                {
                    return;
                }

                Age += dt;
                Step(dt);
                Render(GetCurrentAlpha());
            }

            public bool IntersectsCircle(Vector2 logicalPosition, float radius)
            {
                var delta = logicalPosition - CenterLogical;
                var distance = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y);
                return Mathf.Abs(distance - CurrentRadius) <= Thickness * 0.5f + radius;
            }

            public bool IntersectsBounds(Vector2 center, Vector2 halfExtents)
            {
                var min = center - halfExtents;
                var max = center + halfExtents;
                var closest = new Vector2(
                    Mathf.Clamp(CenterLogical.x, min.x, max.x),
                    Mathf.Clamp(CenterLogical.y, min.y, max.y));
                var minDistance = Vector2.Distance(CenterLogical, closest);
                var farthest = 0f;
                farthest = Mathf.Max(farthest, Vector2.Distance(CenterLogical, new Vector2(min.x, min.y)));
                farthest = Mathf.Max(farthest, Vector2.Distance(CenterLogical, new Vector2(min.x, max.y)));
                farthest = Mathf.Max(farthest, Vector2.Distance(CenterLogical, new Vector2(max.x, min.y)));
                farthest = Mathf.Max(farthest, Vector2.Distance(CenterLogical, new Vector2(max.x, max.y)));

                var halfBand = Thickness * 0.5f;
                return minDistance <= CurrentRadius + halfBand && farthest >= CurrentRadius - halfBand;
            }

            public bool IntersectsRingBand(Vector2 otherCenter, float otherRadius, float otherThickness)
            {
                var centerDistance = Vector2.Distance(CenterLogical, otherCenter);
                var combinedBand = Thickness * 0.5f + otherThickness * 0.5f;
                return centerDistance <= CurrentRadius + otherRadius + combinedBand &&
                       centerDistance >= Mathf.Abs(CurrentRadius - otherRadius) - combinedBand;
            }

            public void Dispose()
            {
                if (Visual != null)
                {
                    Visual.Clear();
                    Game.PoolController.Despawn(Visual.transform);
                }
            }

            protected abstract void Step(float dt);
            protected abstract float GetCurrentAlpha();
            protected abstract float GetNormalizedAge();

            protected virtual RingVisualState GetVisualState()
            {
                return RingVisualState.Active;
            }

            protected void Render(float alpha)
            {
                if (Mathf.Abs(CurrentRadius - _lastRenderedRadius) < 4f && !Finished)
                {
                    return;
                }

                _lastRenderedRadius = CurrentRadius;
                var color = BaseColor.WithAlpha(BaseColor.a * Mathf.Clamp01(alpha));
                Visual?.Render(Game.References, CenterLogical, CurrentRadius, Thickness, color, Style, GetVisualState(), GetNormalizedAge());
            }
        }

        private sealed class DefensiveRingState : RingFieldState
        {
            private readonly float _startRadius;
            private readonly float _finalRadius;
            private readonly float _growDuration;
            private readonly float _holdDuration;
            private readonly float _fadeDuration;

            public DefensiveRingState(GameController game, PlayerFieldRingVisual visual, Vector2 centerLogical, PlayerCounterShotDefinition definition)
                : base(game, visual, centerLogical, definition.DefensiveRingThickness, definition.DefensiveRingColor, RingVisualStyle.DefensiveFieldResolved)
            {
                _startRadius = definition.DefensiveRingStartRadius;
                _finalRadius = definition.DefensiveRingFinalRadius;
                _growDuration = Mathf.Max(0.01f, definition.DefensiveRingGrowDuration);
                _holdDuration = Mathf.Max(0f, definition.DefensiveRingHoldDuration);
                _fadeDuration = Mathf.Max(0.01f, definition.DefensiveRingFadeDuration);
                CurrentRadius = _startRadius;
            }

            protected override void Step(float dt)
            {
                if (Age <= _growDuration)
                {
                    CurrentRadius = Mathf.Lerp(_startRadius, _finalRadius, Mathf.SmoothStep(0f, 1f, Age / _growDuration));
                    return;
                }

                CurrentRadius = _finalRadius;
                if (Age >= _growDuration + _holdDuration + _fadeDuration)
                {
                    Finished = true;
                }
            }

            protected override float GetCurrentAlpha()
            {
                if (Age <= _growDuration + _holdDuration)
                {
                    return 1f;
                }

                return 1f - Mathf.Clamp01((Age - _growDuration - _holdDuration) / _fadeDuration);
            }

            protected override float GetNormalizedAge()
            {
                return Mathf.Clamp01(Age / (_growDuration + _holdDuration + _fadeDuration));
            }

            protected override RingVisualState GetVisualState()
            {
                return Age < _growDuration * 0.2f ? RingVisualState.Telegraph : RingVisualState.Active;
            }
        }

        private sealed class GlobalWaveState : RingFieldState
        {
            private readonly float _startRadius;
            private readonly float _finalRadius;
            private readonly float _duration;

            public GlobalWaveState(GameController game, PlayerFieldRingVisual visual, Vector2 centerLogical, PlayerSkillDefinition definition)
                : base(game, visual, centerLogical, definition.GlobalRingThickness, definition.GlobalRingColor, RingVisualStyle.GlobalWaveResolved)
            {
                _startRadius = definition.GlobalRingStartRadius;
                var authoredFinalRadius = definition.GlobalRingFinalRadius;
                var authoredDuration = Mathf.Max(0.01f, definition.Duration);
                var authoredSpeed = Mathf.Max(0.01f, (authoredFinalRadius - _startRadius) / authoredDuration);
                var screenCoverRadius = CalculateScreenCoverRadius(game, centerLogical, definition.GlobalRingThickness);
                _finalRadius = Mathf.Max(authoredFinalRadius, screenCoverRadius);
                _duration = Mathf.Max(authoredDuration, (_finalRadius - _startRadius) / authoredSpeed);
                CurrentRadius = _startRadius;
            }

            protected override void Step(float dt)
            {
                var t = Mathf.Clamp01(Age / _duration);
                CurrentRadius = Mathf.Lerp(_startRadius, _finalRadius, Mathf.SmoothStep(0f, 1f, t));
                if (Age >= _duration)
                {
                    Finished = true;
                }
            }

            protected override float GetCurrentAlpha()
            {
                return 1f - Mathf.Clamp01(Age / _duration);
            }

            protected override float GetNormalizedAge()
            {
                return Mathf.Clamp01(Age / _duration);
            }

            protected override RingVisualState GetVisualState()
            {
                return Age < _duration * 0.12f ? RingVisualState.Telegraph : RingVisualState.Active;
            }

            // Let the wave fully clear the viewport even when cast from the lowest player position.
            private static float CalculateScreenCoverRadius(GameController game, Vector2 centerLogical, float thickness)
            {
                if (game?.Config == null)
                {
                    return 0f;
                }

                var width = game.Config.logicalWidth;
                var height = game.Config.logicalHeight;
                var maxDistance = 0f;
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(centerLogical, Vector2.zero));
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(centerLogical, new Vector2(width, 0f)));
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(centerLogical, new Vector2(0f, height)));
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(centerLogical, new Vector2(width, height)));
                return maxDistance + thickness;
            }
        }

        private readonly List<DefensiveRingState> _defensiveRings = new();
        private readonly List<GlobalWaveState> _globalWaves = new();
        private GameController _game;

        public void Initialize(GameController game)
        {
            _game = game;
        }

        public void ResetState()
        {
            ClearList(_defensiveRings);
            ClearList(_globalWaves);
        }

        public void Tick(float dt)
        {
            TickList(_defensiveRings, dt);
            TickList(_globalWaves, dt);
        }

        public void SpawnDefensiveRing(Vector2 logicalPosition, PlayerCounterShotDefinition definition)
        {
            if (_game == null || definition == null || definition.ShotKind != PlayerCounterShotKind.DefensiveRing)
            {
                return;
            }

            TrimOldest(_defensiveRings, definition.MaxActiveDefensiveRings);

            var visual = _game.PoolController.SpawnRingVisual(_game.References.LogicalToWorld(logicalPosition));
            if (visual == null)
            {
                return;
            }

            _defensiveRings.Add(new DefensiveRingState(_game, visual, logicalPosition, definition));
        }

        public void SpawnGlobalRing(Vector2 logicalPosition, PlayerSkillDefinition definition)
        {
            if (_game == null || definition == null || definition.SkillKind != PlayerSkillKind.ExpandingGlobalRing)
            {
                return;
            }

            TrimOldest(_globalWaves, definition.MaxActiveGlobalRings);

            var visual = _game.PoolController.SpawnRingVisual(_game.References.LogicalToWorld(logicalPosition));
            if (visual == null)
            {
                return;
            }

            _globalWaves.Add(new GlobalWaveState(_game, visual, logicalPosition, definition));
        }

        public bool TryResolveBossProjectile(BossProjectile projectile)
        {
            if (projectile == null || (_globalWaves.Count == 0 && _defensiveRings.Count == 0))
            {
                return false;
            }

            var position = projectile.Position;
            var radius = projectile.CollisionRadius;

            for (var i = 0; i < _globalWaves.Count; i++)
            {
                if (!_globalWaves[i].IntersectsCircle(position, radius))
                {
                    continue;
                }

                if (projectile.Kind == BossProjectile.ProjectileKind.Parry)
                {
                    if (!_game.Player.CanParryTarget(projectile))
                    {
                        return false;
                    }

                    _game.Player.OnSuccessfulParry(projectile);
                    if (projectile.PersistsOnParry)
                    {
                        projectile.MarkParried();
                        return false;
                    }
                }

                _game.DespawnProjectile(projectile);
                return true;
            }

            if (projectile.Kind == BossProjectile.ProjectileKind.Parry)
            {
                return false;
            }

            for (var i = 0; i < _defensiveRings.Count; i++)
            {
                if (_defensiveRings[i].IntersectsCircle(position, radius))
                {
                    _game.DespawnProjectile(projectile);
                    return true;
                }
            }

            return false;
        }

        public bool TryResolveShockwave(BossShockwaveRing ring, Vector2 hitPoint)
        {
            if (ring == null)
            {
                return false;
            }

            for (var i = 0; i < _globalWaves.Count; i++)
            {
                if (!_globalWaves[i].IntersectsRingBand(ring.CenterLogical, ring.Radius, ring.Thickness))
                {
                    continue;
                }

                var direction = (_globalWaves[i].Center - ring.CenterLogical).sqrMagnitude > 0.001f
                    ? (_globalWaves[i].Center - ring.CenterLogical).normalized
                    : Vector2.up;
                var actualHitPoint = ring.CenterLogical + direction * ring.Radius;
                _game.Player.OnSuccessfulWaveParry(actualHitPoint);
                ring.ConsumeBySkillParry();
                return true;
            }

            return false;
        }

        public bool TryResolveParryCharge(Vector2 centerLogical, Vector2 halfExtents, Vector2 hitPoint)
        {
            for (var i = 0; i < _globalWaves.Count; i++)
            {
                if (!_globalWaves[i].IntersectsBounds(centerLogical, halfExtents))
                {
                    continue;
                }

                _game.Player.OnSuccessfulWaveParry(hitPoint);
                return true;
            }

            return false;
        }

        private static void TickList<T>(List<T> list, float dt) where T : RingFieldState
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                list[i].Tick(dt);
                if (!list[i].Finished)
                {
                    continue;
                }

                list[i].Dispose();
                list.RemoveAt(i);
            }
        }

        private static void ClearList<T>(List<T> list) where T : RingFieldState
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                list[i].Dispose();
            }

            list.Clear();
        }

        private static void TrimOldest<T>(List<T> list, int maxCount) where T : RingFieldState
        {
            while (list.Count >= maxCount)
            {
                list[0].Dispose();
                list.RemoveAt(0);
            }
        }
    }
}
