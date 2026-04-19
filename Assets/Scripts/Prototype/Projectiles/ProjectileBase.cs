using UnityEngine;

namespace CupHeadClone.Prototype
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        protected GameController Game;
        protected SpriteRenderer Renderer;
        private bool _visualsBuilt;

        protected Vector2 LogicalPosition;
        protected Vector2 Velocity;
        protected float Radius;
        protected float Lifetime;

        public Vector2 Position => LogicalPosition;
        public float CollisionRadius => Radius;
        public bool IsGameplayActive { get; private set; }
        public virtual bool PreserveOnProjectileClear => false;

        public virtual void Initialize(GameController game)
        {
            Game = game;
            EnsureRenderer();
        }

        private void OnSpawned()
        {
            if (Game == null)
            {
#if UNITY_2023_1_OR_NEWER
                Game = FindFirstObjectByType<GameController>();
#else
                Game = FindObjectOfType<GameController>();
#endif
            }

            if (Renderer == null)
            {
                EnsureRenderer();
            }
        }

        public void ActivateForGameplay(GameController game)
        {
            Game = game;
            EnsureRenderer();
            IsGameplayActive = true;
        }

        public void DeactivateForGameplay()
        {
            IsGameplayActive = false;
        }

        protected void SyncTransform()
        {
            transform.position = Game.References.LogicalToWorld(LogicalPosition);
        }

        protected bool IsOutOfBounds(float left, float right, float top, float bottom)
        {
            return LogicalPosition.x < left || LogicalPosition.x > right || LogicalPosition.y < top || LogicalPosition.y > bottom || Lifetime <= 0f;
        }

        protected abstract void Tick(float dt);

        protected virtual void BuildVisuals()
        {
        }

        protected void EnsureRenderer()
        {
            Renderer = gameObject.GetComponent<SpriteRenderer>();
            if (Renderer == null)
            {
                Renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            Renderer.sprite = PrototypeVisualUtility.CircleSprite;
            Renderer.sortingOrder = 10;
            if (_visualsBuilt)
            {
                return;
            }

            _visualsBuilt = true;
            BuildVisuals();
        }

        public void ManagedTick(float dt)
        {
            if (Game == null || !Game.IsPlaying || !IsGameplayActive)
            {
                return;
            }

            Tick(dt);
        }
    }
}
