using DG.Tweening;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class TutorialBreakLessonController : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer bossBodyRenderer;
        [SerializeField] private SpriteRenderer bossCoreRenderer;
        [SerializeField] private SpriteRenderer weakZoneRenderer;
        [SerializeField] private SpriteRenderer weakZoneRingRenderer;

        private GameController _game;
        private PlayerFieldRingVisual _weakZoneRingVisual;
        private Tween _weakZoneTween;
        private Tween _ringTween;
        private float _weakZoneRadius;
        private Vector2 _weakZoneLogicalPosition;

        public void Initialize(GameController game)
        {
            _game = game;
            EnsureVisuals();
            Hide();
        }

        public void Show(Vector2 bossLogicalPosition, Vector2 weakZoneLogicalPosition, float weakZoneRadius)
        {
            EnsureVisuals();
            visualRoot.gameObject.SetActive(true);
            weakZoneRenderer.gameObject.SetActive(true);
            visualRoot.position = _game.References.LogicalToWorld(bossLogicalPosition);

            _weakZoneLogicalPosition = weakZoneLogicalPosition;
            _weakZoneRadius = weakZoneRadius;

            if (weakZoneRenderer != null)
            {
                weakZoneRenderer.transform.position = _game.References.LogicalToWorld(weakZoneLogicalPosition);
                var zoneScale = weakZoneRadius / _game.Config.pixelsPerUnit * 2f;
                weakZoneRenderer.transform.localScale = new Vector3(zoneScale, zoneScale, 1f);
                weakZoneRenderer.color = PrototypeVisualUtility.WeakGold.WithAlpha(0.32f);
            }

            _weakZoneRingVisual?.Render(_game.References, weakZoneLogicalPosition, weakZoneRadius, 18f, PrototypeVisualUtility.WeakGold.WithAlpha(0.3f), RingVisualStyle.WeakZoneResolved, RingVisualState.Telegraph, 0f);

            _weakZoneTween?.Kill();
            _ringTween?.Kill();
            if (weakZoneRenderer != null)
            {
                _weakZoneTween = weakZoneRenderer.transform
                    .DOScale(weakZoneRenderer.transform.localScale * 1.06f, 0.45f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            _ringTween = null;
        }

        public void Hide()
        {
            _weakZoneTween?.Kill();
            _ringTween?.Kill();
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(false);
            }

            if (weakZoneRenderer != null)
            {
                weakZoneRenderer.gameObject.SetActive(false);
            }

            _weakZoneRingVisual?.Clear();
        }

        public bool ContainsPlayer(Vector2 playerLogicalPosition, float playerRadius)
        {
            var radius = _weakZoneRadius + playerRadius;
            return (playerLogicalPosition - _weakZoneLogicalPosition).sqrMagnitude <= radius * radius;
        }

        public void Tick(bool playerInside, bool rageFull)
        {
            if (weakZoneRenderer == null)
            {
                return;
            }

            weakZoneRenderer.color = rageFull
                ? PrototypeVisualUtility.CounterGold.WithAlpha(0.44f)
                : playerInside
                    ? PrototypeVisualUtility.WeakGold.WithAlpha(0.42f)
                    : PrototypeVisualUtility.WeakGold.WithAlpha(0.32f);

            var ringColor = rageFull
                ? PrototypeVisualUtility.CounterGold.WithAlpha(0.4f)
                : playerInside
                    ? PrototypeVisualUtility.WeakGold.WithAlpha(0.36f)
                    : PrototypeVisualUtility.WeakGold.WithAlpha(0.28f);
            _weakZoneRingVisual?.Render(
                _game.References,
                _weakZoneLogicalPosition,
                _weakZoneRadius,
                18f,
                ringColor,
                RingVisualStyle.WeakZoneResolved,
                rageFull ? RingVisualState.Active : RingVisualState.Telegraph,
                0f);
        }

        private void OnDestroy()
        {
            _weakZoneTween?.Kill();
            _ringTween?.Kill();
        }

        private void EnsureVisuals()
        {
            if (visualRoot == null)
            {
                var existingRoot = transform.Find("BreakBossVisual");
                if (existingRoot == null)
                {
                    existingRoot = new GameObject("BreakBossVisual").transform;
                    existingRoot.SetParent(transform, false);
                }

                visualRoot = existingRoot;
            }

            if (bossBodyRenderer == null)
            {
                bossBodyRenderer = PrototypeVisualUtility.EnsureSpriteChild(visualRoot, "BossBody", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.BossRose, 15);
            }

            bossBodyRenderer.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
            bossBodyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            if (bossCoreRenderer == null)
            {
                bossCoreRenderer = PrototypeVisualUtility.EnsureSpriteChild(visualRoot, "BossCore", PrototypeVisualUtility.CircleSprite, Color.white, 16);
            }

            bossCoreRenderer.transform.localScale = Vector3.one * 0.18f;

            if (weakZoneRenderer == null)
            {
                weakZoneRenderer = PrototypeVisualUtility.EnsureSpriteChild(transform, "WeakZone", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.WeakGold.WithAlpha(0.32f), 14);
            }

            if (weakZoneRingRenderer == null)
            {
                weakZoneRingRenderer = PrototypeVisualUtility.EnsureSpriteChild(transform, "WeakZoneRing", PrototypeVisualUtility.RingSprite, PrototypeVisualUtility.WeakGold.WithAlpha(0.14f), 13);
            }

            if (_weakZoneRingVisual == null)
            {
                var ringRoot = transform.Find("WeakZoneRingVisual");
                if (ringRoot == null)
                {
                    ringRoot = new GameObject("WeakZoneRingVisual").transform;
                    ringRoot.SetParent(transform, false);
                }

                _weakZoneRingVisual = ringRoot.GetComponent<PlayerFieldRingVisual>();
                if (_weakZoneRingVisual == null)
                {
                    _weakZoneRingVisual = ringRoot.gameObject.AddComponent<PlayerFieldRingVisual>();
                }

                _weakZoneRingVisual.SetSortingOrder(13);
            }

            if (weakZoneRingRenderer != null)
            {
                weakZoneRingRenderer.enabled = false;
            }
        }
    }
}
