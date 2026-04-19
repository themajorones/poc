using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PrototypePresentationController : MonoBehaviour
    {
        [FoldoutGroup("Background")] [SerializeField] private int starCount = 28;
        [FoldoutGroup("Background")] [SerializeField] private Vector2 starSpeedRange = new(0.08f, 0.26f);
        [FoldoutGroup("Background")] [SerializeField] private Vector2 starScaleRange = new(0.015f, 0.045f);
        [FoldoutGroup("Background")] [SerializeField] private float laneAlpha = 0.025f;
        [FoldoutGroup("Background")] [SerializeField] private float bossAuraFollow = 2.4f;
        [FoldoutGroup("Background")] [SerializeField] private SpriteRenderer topGlowRenderer;
        [FoldoutGroup("Background")] [SerializeField] private SpriteRenderer bossAuraRenderer;
        [FoldoutGroup("Background")] [SerializeField] private float topGlowBaseAlpha = 0.04f;
        [FoldoutGroup("Background")] [SerializeField] private float topGlowPulseAlpha = 0.008f;
        [FoldoutGroup("Background")] [SerializeField] private Vector3 topGlowScale = new(8.2f, 4.8f, 1f);
        [FoldoutGroup("Background")] [SerializeField] private float bossAuraBaseAlpha = 0.04f;
        [FoldoutGroup("Background")] [SerializeField] private float bossAuraPulseAlpha = 0.01f;
        [FoldoutGroup("Background")] [SerializeField] private Vector3 bossAuraScale = new(3.1f, 2.2f, 1f);

        private readonly List<Transform> _stars = new();
        private readonly List<float> _starSpeeds = new();
        private GameController _game;
        private Transform _root;
        private SpriteRenderer _backdrop;
        private SpriteRenderer _midGlow;
        private SpriteRenderer _topGlow;
        private SpriteRenderer _bossAura;
        private SpriteRenderer _vignette;

        public void Initialize(GameController game)
        {
            _game = game;
            Build();
            ApplyStaticTheme();
        }

        public void RefreshState(GameController.RunState state)
        {
            if (_bossAura != null)
            {
                _bossAura.color = PrototypeVisualUtility.WeakGold.WithAlpha(state == GameController.RunState.Playing ? bossAuraBaseAlpha : bossAuraBaseAlpha * 0.5f);
            }
        }

        private void Build()
        {
            var parent = _game.References.BoundsRoot != null ? _game.References.BoundsRoot : _game.References.GameplayRoot;
            if (parent == null)
            {
                return;
            }

            _root = parent.Find("PresentationRuntime");
            if (_root == null)
            {
                _root = new GameObject("PresentationRuntime").transform;
                _root.SetParent(parent, false);
            }

            _backdrop = PrototypeVisualUtility.EnsureSpriteChild(_root, "Backdrop", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.BackgroundMid, -80);
            _backdrop.transform.localScale = new Vector3(_game.Config.logicalWidth / _game.Config.pixelsPerUnit, _game.Config.logicalHeight / _game.Config.pixelsPerUnit, 1f);

            _midGlow = PrototypeVisualUtility.EnsureSpriteChild(_root, "MidGlow", PrototypeVisualUtility.CircleSprite, new Color(0.19f, 0.46f, 0.86f, 0.04f), -79);
            _midGlow.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            _midGlow.transform.localScale = new Vector3(7.8f, 9.2f, 1f);

            _topGlow = topGlowRenderer != null
                ? topGlowRenderer
                : PrototypeVisualUtility.EnsureSpriteChild(_root, "TopGlow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.PlayerCyan.WithAlpha(topGlowBaseAlpha), -78);
            _topGlow.transform.localPosition = new Vector3(0f, 3.15f, 0f);
            _topGlow.transform.localScale = topGlowScale;

            _bossAura = bossAuraRenderer != null
                ? bossAuraRenderer
                : PrototypeVisualUtility.EnsureSpriteChild(_root, "BossAura", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.WeakGold.WithAlpha(bossAuraBaseAlpha), -77);
            _bossAura.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            _bossAura.transform.localScale = bossAuraScale;

            _vignette = PrototypeVisualUtility.EnsureSpriteChild(_root, "Vignette", PrototypeVisualUtility.CircleSprite, new Color(0f, 0f, 0f, 0.08f), -76);
            _vignette.transform.localScale = new Vector3(10f, 13f, 1f);

            BuildLaneGuides();
            BuildStars();
        }

        private void ApplyStaticTheme()
        {
            if (_game.References.GameplayCamera == null)
            {
                return;
            }

            _game.References.GameplayCamera.backgroundColor = PrototypeVisualUtility.BackgroundBottom;
        }

        private void BuildLaneGuides()
        {
            for (var i = 1; i < 4; i++)
            {
                var lane = PrototypeVisualUtility.EnsureSpriteChild(_root, $"Lane_{i}", PrototypeVisualUtility.SquareSprite, Color.white.WithAlpha(laneAlpha), -74);
                lane.transform.localScale = new Vector3(0.012f, 7.9f, 1f);
                var logical = new Vector2(_game.Config.logicalWidth / 4f * i, _game.Config.logicalHeight * 0.53f);
                lane.transform.position = _game.References.LogicalToWorld(logical, 0f);
            }
        }

        private void BuildStars()
        {
            _stars.Clear();
            _starSpeeds.Clear();

            var starsRoot = _root.Find("Stars");
            if (starsRoot == null)
            {
                starsRoot = new GameObject("Stars").transform;
                starsRoot.SetParent(_root, false);
            }

            for (var i = starsRoot.childCount - 1; i >= 0; i--)
            {
                _stars.Add(starsRoot.GetChild(i));
            }

            while (_stars.Count < starCount)
            {
                var starRenderer = PrototypeVisualUtility.EnsureSpriteChild(starsRoot, $"Star_{_stars.Count:00}", PrototypeVisualUtility.CircleSprite, Color.white, -75);
                _stars.Add(starRenderer.transform);
            }

            for (var i = 0; i < _stars.Count; i++)
            {
                var t = _stars[i];
                var renderer = t.GetComponent<SpriteRenderer>();
                renderer.color = new Color(0.8f, 0.9f, 1f, Random.Range(0.08f, 0.22f));
                t.localPosition = new Vector3(Random.Range(-2.1f, 2.1f), Random.Range(-4f, 4f), 0f);
                var scale = Random.Range(starScaleRange.x, starScaleRange.y);
                t.localScale = Vector3.one * scale;
                if (_starSpeeds.Count <= i)
                {
                    _starSpeeds.Add(Random.Range(starSpeedRange.x, starSpeedRange.y));
                }
                else
                {
                    _starSpeeds[i] = Random.Range(starSpeedRange.x, starSpeedRange.y);
                }
            }
        }

        private void LateUpdate()
        {
            if (_game == null)
            {
                return;
            }

            var dt = Time.deltaTime;
            for (var i = 0; i < _stars.Count; i++)
            {
                var star = _stars[i];
                var pos = star.localPosition;
                pos.y -= _starSpeeds[i] * dt;
                if (pos.y < -4.7f)
                {
                    pos.y = 4.7f;
                    pos.x = Random.Range(-2.2f, 2.2f);
                }

                star.localPosition = pos;
            }

            var t = Time.time;
            if (_topGlow != null)
            {
                _topGlow.color = PrototypeVisualUtility.PlayerCyan.WithAlpha(topGlowBaseAlpha + Mathf.Sin(t * 0.6f) * topGlowPulseAlpha);
                _topGlow.transform.localScale = new Vector3(topGlowScale.x + Mathf.Sin(t * 0.4f) * 0.16f, topGlowScale.y, topGlowScale.z);
            }

            if (_bossAura != null)
            {
                var targetY = 2.55f;
                var targetX = 0f;
                if (_game.Boss != null && _game.Boss.Active)
                {
                    var bossWorld = _game.References.LogicalToWorld(_game.Boss.LogicalPosition);
                    targetX = bossWorld.x * 0.35f;
                    targetY = Mathf.Clamp(bossWorld.y - 0.05f, 1.6f, 3.15f);
                }

                var targetPos = new Vector3(targetX, targetY, 0f);
                _bossAura.transform.localPosition = Vector3.Lerp(_bossAura.transform.localPosition, targetPos, dt * bossAuraFollow);
                var pulse = bossAuraBaseAlpha + Mathf.Sin(t * 1.7f) * bossAuraPulseAlpha;
                _bossAura.color = (_game.Boss != null && _game.Boss.BreakTimer > 0f
                    ? PrototypeVisualUtility.WeakGold
                    : PrototypeVisualUtility.BossGlow).WithAlpha(pulse);
                _bossAura.transform.localScale = new Vector3(bossAuraScale.x + Mathf.Sin(t * 0.9f) * 0.18f, bossAuraScale.y + Mathf.Sin(t * 0.7f) * 0.12f, bossAuraScale.z);
            }
        }
    }
}
