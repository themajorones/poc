using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class GameReferences : MonoBehaviour
    {
        [SerializeField] private PrototypeCombatConfig config;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private Transform gameplayRoot;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform bossRoot;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private Transform pickupRoot;
        [SerializeField] private Transform boundsRoot;

        public PrototypeCombatConfig Config => config;
        public Camera GameplayCamera => gameplayCamera;
        public Canvas UiCanvas => uiCanvas;
        public Transform GameplayRoot => gameplayRoot;
        public Transform PlayerRoot => playerRoot;
        public Transform BossRoot => bossRoot;
        public Transform ProjectileRoot => projectileRoot;
        public Transform VfxRoot => vfxRoot;
        public Transform PickupRoot => pickupRoot;
        public Transform BoundsRoot => boundsRoot;
        public float Width => config.logicalWidth;
        public float Height => config.logicalHeight;
        public float PixelsPerUnit => config.pixelsPerUnit;

        public Vector3 LogicalToWorld(Vector2 logical, float z = 0f)
        {
            var x = (logical.x - Width * 0.5f) / PixelsPerUnit;
            var y = (Height * 0.5f - logical.y) / PixelsPerUnit;
            return new Vector3(x, y, z);
        }

        public Vector2 WorldToLogical(Vector3 world)
        {
            return new Vector2(
                world.x * PixelsPerUnit + Width * 0.5f,
                Height * 0.5f - world.y * PixelsPerUnit);
        }

        public Vector2 ClampLogicalPosition(Vector2 logical, float marginX, float marginTop, float marginBottom)
        {
            logical.x = Mathf.Clamp(logical.x, marginX, Width - marginX);
            logical.y = Mathf.Clamp(logical.y, marginTop, Height - marginBottom);
            return logical;
        }

        public void ApplyCameraFraming()
        {
            ResolveCameraReference();

            if (gameplayCamera == null || config == null)
            {
                return;
            }

            gameplayCamera.orthographic = true;
            gameplayCamera.orthographicSize = config.logicalHeight / (config.pixelsPerUnit * 2f);
        }

        private void ResolveCameraReference()
        {
            if (gameplayCamera != null)
            {
                return;
            }

#if UNITY_2023_1_OR_NEWER
            gameplayCamera = FindFirstObjectByType<Camera>();
#else
            gameplayCamera = FindObjectOfType<Camera>();
#endif
        }
    }
}
