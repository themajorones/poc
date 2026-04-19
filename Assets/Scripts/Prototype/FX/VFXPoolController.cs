using System.Collections.Generic;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class VFXPoolController : MonoBehaviour
    {
        private sealed class TimedEffect
        {
            public SpriteRenderer renderer;
            public float life;
            public float age;
            public Vector3 startScale;
            public Vector3 endScale;
            public Color baseColor;
            public Vector3 velocity;
            public float angularVelocity;
            public bool useEaseOut;
        }

        private sealed class TimedRingEffect
        {
            public PlayerFieldRingVisual visual;
            public Vector2 logicalPosition;
            public float life;
            public float age;
            public float startRadius;
            public float endRadius;
            public float thickness;
            public Color baseColor;
            public RingVisualStyle style;
            public RingVisualState state;
        }

        private readonly List<TimedEffect> _active = new();
        private readonly List<TimedRingEffect> _activeRings = new();
        private GameController _game;

        public void Initialize(GameController game)
        {
            _game = game;
        }

        public void ResetState()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].renderer != null)
                {
                    _game.PoolController.Despawn(_active[i].renderer.transform);
                }
            }

            _active.Clear();

            for (var i = _activeRings.Count - 1; i >= 0; i--)
            {
                if (_activeRings[i].visual != null)
                {
                    _activeRings[i].visual.Clear();
                    _game.PoolController.Despawn(_activeRings[i].visual.transform);
                }
            }

            _activeRings.Clear();
        }

        public void SpawnBurst(Vector2 logicalPosition, Color color, int count, float life)
        {
            if (_game == null)
            {
                return;
            }

            var scaledCount = Mathf.Max(1, Mathf.RoundToInt(count * _game.Config.vfx.burstCountScale));
            if (GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
            {
                scaledCount = Mathf.Min(2, scaledCount);
            }

            for (var i = 0; i < scaledCount; i++)
            {
                if (GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
                {
                    return;
                }

                var offset = Random.insideUnitCircle * 0.18f;
                var effect = _game.PoolController.SpawnEffect(
                    PrototypePoolController.EffectTemplateKind.Burst,
                    _game.References.LogicalToWorld(logicalPosition) + new Vector3(offset.x, offset.y, 0f));
                if (effect == null)
                {
                    return;
                }

                effect.transform.position = _game.References.LogicalToWorld(logicalPosition) + new Vector3(offset.x, offset.y, 0f);
                effect.color = color;
                effect.transform.localScale = Vector3.one * Random.Range(0.04f, 0.1f);
                _active.Add(new TimedEffect
                {
                    renderer = effect,
                    life = life,
                    age = 0f,
                    startScale = effect.transform.localScale,
                    endScale = Vector3.one * 0.01f,
                    baseColor = color,
                    velocity = new Vector3(offset.x, offset.y, 0f) * 1.2f,
                    angularVelocity = Random.Range(-90f, 90f)
                });
            }
        }

        // Shared ring API: radius/thickness/life map directly to gameplay-authored data.
        public void SpawnRing(Vector2 logicalPosition, Color color, float startRadius, float thickness, float life)
        {
            SpawnRing(logicalPosition, color, startRadius, thickness, life, RingVisualStyle.TransientImpactResolved, RingVisualState.Impact);
        }

        public void SpawnRing(
            Vector2 logicalPosition,
            Color color,
            float startRadius,
            float thickness,
            float life,
            RingVisualStyle style,
            RingVisualState state)
        {
            if (_game == null || GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
            {
                return;
            }

            var visual = _game.PoolController.SpawnRingVisual(_game.References.LogicalToWorld(logicalPosition));
            if (visual == null)
            {
                return;
            }

            visual.SetSortingOrder(19);
            _activeRings.Add(new TimedRingEffect
            {
                visual = visual,
                logicalPosition = logicalPosition,
                life = Mathf.Max(0.05f, life),
                age = 0f,
                startRadius = Mathf.Max(0.04f, startRadius),
                endRadius = Mathf.Max(startRadius + Mathf.Max(thickness * 1.9f, startRadius * 0.92f), startRadius + 0.06f),
                thickness = Mathf.Max(0.03f, thickness),
                baseColor = color,
                style = style,
                state = state
            });
        }

        public void SpawnAfterimage(Vector2 logicalPosition, Color color, float life, float scale)
        {
            if (_game == null || GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
            {
                return;
            }

            var effect = _game.PoolController.SpawnEffect(
                PrototypePoolController.EffectTemplateKind.Afterimage,
                _game.References.LogicalToWorld(logicalPosition));
            if (effect == null)
            {
                return;
            }

            effect.transform.position = _game.References.LogicalToWorld(logicalPosition);
            effect.color = color;
            effect.transform.localScale = Vector3.one * 0.24f * scale;
            _active.Add(new TimedEffect
            {
                renderer = effect,
                life = life,
                age = 0f,
                startScale = effect.transform.localScale,
                endScale = effect.transform.localScale * 1.2f,
                baseColor = color
            });
        }

        public void SpawnAfterimage(SpriteRenderer sourceRenderer, Vector2 logicalPosition, Color color, float life, float scale)
        {
            if (_game == null || GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
            {
                return;
            }

            var effect = _game.PoolController.SpawnEffect(
                PrototypePoolController.EffectTemplateKind.Afterimage,
                _game.References.LogicalToWorld(logicalPosition));
            if (effect == null)
            {
                return;
            }

            effect.transform.position = _game.References.LogicalToWorld(logicalPosition);
            effect.sprite = sourceRenderer != null && sourceRenderer.sprite != null ? sourceRenderer.sprite : PrototypeVisualUtility.CircleSprite;
            effect.color = color;
            effect.transform.rotation = sourceRenderer != null ? sourceRenderer.transform.rotation : Quaternion.identity;
            effect.transform.localScale = (sourceRenderer != null ? sourceRenderer.transform.lossyScale : Vector3.one * 0.24f) * scale;
            _active.Add(new TimedEffect
            {
                renderer = effect,
                life = life,
                age = 0f,
                startScale = effect.transform.localScale,
                endScale = effect.transform.localScale * 1.08f,
                baseColor = color
            });
        }

        public void SpawnDirectionalGhosts(Vector2 logicalPosition, Color color, int count, float life, Vector2 stepOffset, float scale)
        {
            if (_game == null || GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
            {
                return;
            }

            var worldPosition = _game.References.LogicalToWorld(logicalPosition);
            for (var i = 0; i < count; i++)
            {
                if (GetActiveEffectCount() >= _game.Config.vfx.maxActiveEffects)
                {
                    return;
                }

                var effect = _game.PoolController.SpawnEffect(
                    PrototypePoolController.EffectTemplateKind.Afterimage,
                    worldPosition);
                if (effect == null)
                {
                    return;
                }

                var offset = new Vector3(stepOffset.x * i, stepOffset.y * i, 0f);
                var intensity = 1f - (i * 0.18f);
                var ghostColor = color.WithAlpha(color.a * intensity);
                effect.transform.position = worldPosition + offset;
                effect.sprite = PrototypeVisualUtility.SquareSprite;
                effect.color = ghostColor;
                effect.transform.rotation = Quaternion.identity;
                effect.transform.localScale = new Vector3(0.22f, 0.3f, 1f) * scale * intensity;
                _active.Add(new TimedEffect
                {
                    renderer = effect,
                    life = life,
                    age = 0f,
                    startScale = effect.transform.localScale,
                    endScale = new Vector3(0.04f, 0.12f, 1f) * scale * Mathf.Max(0.5f, intensity),
                    baseColor = ghostColor,
                    velocity = new Vector3(0f, -0.18f - (i * 0.03f), 0f),
                    angularVelocity = 0f
                });
            }
        }

        private void Update()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var item = _active[i];
                item.age += Time.deltaTime;
                var t = Mathf.Clamp01(item.age / item.life);
                var easedT = item.useEaseOut ? 1f - Mathf.Pow(1f - t, 1.9f) : t;
                item.renderer.color = item.baseColor.WithAlpha(item.baseColor.a * (1f - easedT));
                item.renderer.transform.localScale = Vector3.Lerp(item.startScale, item.endScale, easedT);
                item.renderer.transform.position += item.velocity * Time.deltaTime;
                if (!Mathf.Approximately(item.angularVelocity, 0f))
                {
                    item.renderer.transform.Rotate(0f, 0f, item.angularVelocity * Time.deltaTime);
                }

                if (item.age < item.life)
                {
                    continue;
                }

                _game.PoolController.Despawn(item.renderer.transform);
                _active.RemoveAt(i);
            }

            for (var i = _activeRings.Count - 1; i >= 0; i--)
            {
                var item = _activeRings[i];
                item.age += Time.deltaTime;
                var t = Mathf.Clamp01(item.age / item.life);
                var easedT = 1f - Mathf.Pow(1f - t, 1.8f);
                var radius = Mathf.Lerp(item.startRadius, item.endRadius, easedT);
                item.visual.Render(
                    _game.References,
                    item.logicalPosition,
                    radius,
                    item.thickness,
                    item.baseColor.WithAlpha(item.baseColor.a * (1f - t)),
                    item.style,
                    item.state,
                    t);

                if (item.age < item.life)
                {
                    continue;
                }

                item.visual.Clear();
                _game.PoolController.Despawn(item.visual.transform);
                _activeRings.RemoveAt(i);
            }
        }

        private int GetActiveEffectCount()
        {
            return _active.Count + _activeRings.Count;
        }
    }
}
