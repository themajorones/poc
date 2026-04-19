using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class RecoveryCoreController : MonoBehaviour
    {
        [FoldoutGroup("Prefab")] [SerializeField] private GameObject visualPrefab;
        [FoldoutGroup("Renderers")] [SerializeField] private Transform visualRoot;
        [FoldoutGroup("Renderers")] [SerializeField] private SpriteRenderer spriteRenderer;
        [FoldoutGroup("Renderers")] [SerializeField] private SpriteRenderer glowRenderer;
        [FoldoutGroup("Renderers")] [SerializeField] private SpriteRenderer ringRenderer;
        [FoldoutGroup("Tuning")] [SerializeField] private Vector3 coreScale = Vector3.one * 0.24f;
        [FoldoutGroup("Tuning")] [SerializeField] private float glowAlpha = 0.18f;
        [FoldoutGroup("Tuning")] [SerializeField] private float ringAlpha = 0.12f;
        [FoldoutGroup("Tuning")] [SerializeField] private Vector3 glowScale = new(1.9f, 1.9f, 1f);
        [FoldoutGroup("Tuning")] [SerializeField] private Vector3 ringScale = new(2.4f, 2.4f, 1f);

        private GameController _game;
        private Vector2 _logicalPosition;
        private float _timer;
        private float _life;
        private bool _active;
        private bool _homing;
        private Tween _pulseTween;

        public bool Active => _active;

        public void Initialize(GameController game)
        {
            _game = game;
            EnsureVisualInstance();

            if (spriteRenderer == null && visualRoot == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = PrototypeVisualUtility.CircleSprite;
                spriteRenderer.color = PrototypeVisualUtility.HealMint.WithAlpha(0f);
                spriteRenderer.sortingOrder = 16;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = coreScale;
            }

            if (glowRenderer != null)
            {
                glowRenderer.transform.localScale = glowScale;
            }

            if (ringRenderer != null)
            {
                ringRenderer.transform.localScale = ringScale;
            }

            _pulseTween = transform.DOScale(0.34f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            SetVisualActive(false);
        }

        public void ResetState()
        {
            _active = false;
            _homing = false;
            SetVisualActive(false);
        }

        public void Spawn(Vector2 logicalPosition)
        {
            _active = true;
            _homing = false;
            _logicalPosition = logicalPosition;
            _timer = 0f;
            _life = 5.5f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = PrototypeVisualUtility.HealMint;
            }

            if (glowRenderer != null)
            {
                glowRenderer.color = PrototypeVisualUtility.HealMint.WithAlpha(glowAlpha);
            }

            if (ringRenderer != null)
            {
                ringRenderer.color = PrototypeVisualUtility.HealMint.WithAlpha(ringAlpha);
            }

            SetVisualActive(true);
            transform.DOPunchScale(Vector3.one * 0.08f, 0.22f, 1);
            SyncTransform();
        }

        public void Tick(float dt)
        {
            if (!_active)
            {
                return;
            }

            _timer += dt;
            _life -= dt;

            if (!_homing)
            {
                _logicalPosition.y += 90f * dt;
                if (visualRoot != null)
                {
                    visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, Vector3.one * 0.3f, dt * 5f);
                }
                if (_timer > 0.6f)
                {
                    _homing = true;
                }
            }
            else
            {
                var delta = _game.Player.LogicalPosition - _logicalPosition;
                var distance = Mathf.Max(1f, delta.magnitude);
                var speed = Mathf.Lerp(140f, 320f, Mathf.Clamp01(_timer / 1.4f));
                _logicalPosition += delta / distance * speed * dt;
                if (visualRoot != null)
                {
                    visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, Vector3.one * 0.36f, dt * 6f);
                }
            }

            if (ringRenderer != null)
            {
                var pulse = ringScale.x + Mathf.Sin(Time.time * 6f) * 0.12f;
                ringRenderer.transform.localScale = new Vector3(pulse, pulse, ringScale.z);
            }

            var pickupRadius = 14f + _game.Config.player.hitboxRadius + 6f;
            if ((_logicalPosition - _game.Player.LogicalPosition).sqrMagnitude <= pickupRadius * pickupRadius || _life <= 0f)
            {
                Collect();
                return;
            }

            SyncTransform();
        }

        private void Collect()
        {
            _active = false;
            _game.Player.HealOne();
            _game.VfxPoolController.SpawnBurst(_logicalPosition, new Color(0.61f, 0.95f, 0.78f, 1f), 16, 0.45f);
            _game.ParryFeedbackController.PlayRecoveryPickup(_logicalPosition);
            SetVisualActive(false);
            _game.BossRushController.OnRecoveryCoreCollected();
        }

        private void EnsureVisualInstance()
        {
            if (visualRoot == null && visualPrefab != null)
            {
                var existing = transform.Find(visualPrefab.name);
                if (existing != null)
                {
                    visualRoot = existing;
                }
                else
                {
                    var instance = Instantiate(visualPrefab, transform);
                    instance.name = visualPrefab.name;
                    visualRoot = instance.transform;
                }
            }

            if (visualRoot == null)
            {
                return;
            }

            spriteRenderer ??= visualRoot.Find("Core")?.GetComponent<SpriteRenderer>();
            glowRenderer ??= visualRoot.Find("Glow")?.GetComponent<SpriteRenderer>();
            ringRenderer ??= visualRoot.Find("Ring")?.GetComponent<SpriteRenderer>();
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
        }

        private void SetVisualActive(bool active)
        {
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(active);
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = active;
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = active;
            }

            if (ringRenderer != null)
            {
                ringRenderer.enabled = active;
            }
        }

        private void SyncTransform()
        {
            transform.position = _game.References.LogicalToWorld(_logicalPosition);
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }
    }
}
