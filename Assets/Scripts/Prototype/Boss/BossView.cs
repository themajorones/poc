using DG.Tweening;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossView : MonoBehaviour
    {
        [SerializeField] private BossAuthoring authoring;

        private GameController _game;

        private SpriteRenderer _glow;
        private SpriteRenderer _body;
        private SpriteRenderer _core;
        private SpriteRenderer _telegraphAura;
        private SpriteRenderer _weakZone;
        private SpriteRenderer _weakZoneRing;
        private PlayerFieldRingVisual _weakZoneRingVisual;
        private Tween _telegraphTween;
        private Tween _weakZoneTween;
        private Color _bodyBaseColor;
        private Color _telegraphBaseColor;

        public void Build(GameController game)
        {
            _game = game;
            authoring ??= GetComponent<BossAuthoring>();
            authoring?.SyncContactBoundsAnchorToVisual();
            authoring?.SyncWeakZoneAnchorToVisual();

            _glow = PrototypeVisualUtility.EnsureSpriteChild(transform, "Glow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.BossGlow.WithAlpha(0.01f), 17);
            _glow.transform.localScale = new Vector3(1.3f, 1.05f, 1f);

            _body = authoring != null && authoring.BodyRenderer != null
                ? authoring.BodyRenderer
                : CreatePart("Body", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.BossRose, 20);
            if (_body.sprite == null)
            {
                _body.sprite = PrototypeVisualUtility.SquareSprite;
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                _body.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            }

            _core = authoring != null && authoring.CoreRenderer != null
                ? authoring.CoreRenderer
                : CreatePart("Core", PrototypeVisualUtility.CircleSprite, Color.white, 21);
            if (_core.sprite == null)
            {
                _core.sprite = PrototypeVisualUtility.CircleSprite;
                _core.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            }

            _telegraphAura = authoring != null && authoring.TelegraphRenderer != null
                ? authoring.TelegraphRenderer
                : CreatePart("TelegraphAura", PrototypeVisualUtility.CircleSprite, new Color(1f, 0.47f, 0.68f, 0f), 16);
            var telegraphSprite = _game != null && _game.Config != null && _game.Config.bossTelegraphSprite != null
                ? _game.Config.bossTelegraphSprite
                : PrototypeVisualUtility.CircleSprite;
            _telegraphAura.sprite = telegraphSprite;
            if (_telegraphAura.sprite != null)
            {
                _telegraphAura.transform.localScale = new Vector3(1.36f, 1.08f, 1f);
            }
            _telegraphTween = _telegraphAura.transform.DOScale(new Vector3(1.5f, 1.16f, 1f), 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            _weakZone = authoring != null && authoring.WeakZoneRenderer != null
                ? authoring.WeakZoneRenderer
                : CreatePart("WeakZone", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.WeakGold.WithAlpha(0f), 17);
            if (_weakZone.sprite == null)
            {
                _weakZone.sprite = PrototypeVisualUtility.CircleSprite;
                _weakZone.transform.localPosition = new Vector3(0f, -0.58f, 0f);
                _weakZone.transform.localScale = new Vector3(0.46f, 0.46f, 1f);
            }
            _weakZoneRing = PrototypeVisualUtility.EnsureSpriteChild(transform, "WeakZoneRing", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.WeakGold.WithAlpha(0f), 16);
            _weakZoneRing.transform.localPosition = _weakZone.transform.localPosition;
            var ringScale = GetWeakZoneRingScale(1.18f);
            _weakZoneRing.transform.localScale = ringScale;
            _weakZoneTween = _weakZoneRing.transform.DOScale(GetWeakZoneRingScale(1.32f), 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            var weakZoneRingRoot = transform.Find("WeakZoneRingVisual");
            if (weakZoneRingRoot == null)
            {
                weakZoneRingRoot = new GameObject("WeakZoneRingVisual").transform;
                weakZoneRingRoot.SetParent(transform, false);
            }

            _weakZoneRingVisual = weakZoneRingRoot.GetComponent<PlayerFieldRingVisual>();
            if (_weakZoneRingVisual == null)
            {
                _weakZoneRingVisual = weakZoneRingRoot.gameObject.AddComponent<PlayerFieldRingVisual>();
            }

            _weakZoneRingVisual.SetSortingOrder(16);
            _weakZoneRing.enabled = false;
            _bodyBaseColor = _body.color;
            _telegraphBaseColor = _telegraphAura.color;
        }

        public void ApplyState(BossController boss)
        {
            var chargeTint = boss.CurrentMove == "parryCharge" && (boss.Phase == BossController.BossPhase.Telegraph || boss.Phase == BossController.BossPhase.Attack);
            var breakActive = boss.BreakTimer > 0f;
            var bossHue = Color.Lerp(PrototypeVisualUtility.BossRose, PrototypeVisualUtility.BossGlow, boss.BossIndex / 4f * 0.18f);
            _telegraphAura.color = new Color(
                _telegraphBaseColor.r,
                _telegraphBaseColor.g,
                _telegraphBaseColor.b,
                boss.Phase == BossController.BossPhase.Telegraph || chargeTint ? Mathf.Max(0.24f, _telegraphBaseColor.a) : 0f);
            _weakZone.color = PrototypeVisualUtility.WeakGold.WithAlpha(breakActive ? 0.62f : 0f);
            if (breakActive)
            {
                var weakZoneLogical = _game.References.WorldToLogical(_weakZone.transform.position);
                var weakZoneRadius = authoring != null ? authoring.GetWeakZoneRadius(_game.Config) : _game.Config.boss.weakZoneRadius;
                _weakZoneRingVisual?.Render(
                    _game.References,
                    weakZoneLogical,
                    weakZoneRadius,
                    16f,
                    PrototypeVisualUtility.WeakGold.WithAlpha(0.34f),
                    RingVisualStyle.WeakZoneResolved,
                    RingVisualState.Telegraph,
                    0f);
            }
            else
            {
                _weakZoneRingVisual?.Clear();
            }

            _body.color = chargeTint
                ? Color.Lerp(_bodyBaseColor, new Color(0.88f, 0.48f, 1f, _bodyBaseColor.a), 0.72f)
                : breakActive
                ? Color.Lerp(_bodyBaseColor, new Color(1f, 0.77f, 0.43f, _bodyBaseColor.a), 0.5f)
                : Color.Lerp(_bodyBaseColor, bossHue, 0.2f);
            if (boss.DamageFlash > 0f)
            {
                _body.color = Color.Lerp(_body.color, new Color(1f, 0.9f, 0.95f, 1f), boss.DamageFlash / 0.12f);
            }

            _glow.color = (breakActive ? PrototypeVisualUtility.WeakGold : PrototypeVisualUtility.BossGlow).WithAlpha(0.01f);
            _core.color = breakActive ? new Color(1f, 0.98f, 0.86f, 1f) : Color.Lerp(Color.white, new Color(1f, 0.88f, 0.94f, 1f), boss.DamageFlash / 0.12f);
        }

        private SpriteRenderer CreatePart(string name, Sprite sprite, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private Vector3 GetWeakZoneRingScale(float multiplier)
        {
            var baseScale = _weakZone != null ? _weakZone.transform.localScale : Vector3.one * 0.46f;
            return new Vector3(baseScale.x * multiplier, baseScale.y * multiplier, 1f);
        }

        private void OnDestroy()
        {
            _telegraphTween?.Kill();
            _weakZoneTween?.Kill();
        }
    }
}
