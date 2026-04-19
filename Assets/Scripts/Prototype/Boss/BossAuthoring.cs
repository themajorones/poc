using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossAuthoring : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer coreRenderer;
        [SerializeField] private SpriteRenderer telegraphRenderer;
        [SerializeField] private SpriteRenderer weakZoneRenderer;

        [Header("Gameplay Anchors")]
        [SerializeField] private Transform contactBoundsAnchor;
        [SerializeField] private Transform weakZoneAnchor;

        public SpriteRenderer BodyRenderer => bodyRenderer;
        public SpriteRenderer CoreRenderer => coreRenderer;
        public SpriteRenderer TelegraphRenderer => telegraphRenderer;
        public SpriteRenderer WeakZoneRenderer => weakZoneRenderer;
        public Transform ContactBoundsAnchor => contactBoundsAnchor;
        public Transform WeakZoneAnchor => weakZoneAnchor;

        public void SyncContactBoundsAnchorToVisual()
        {
            if (contactBoundsAnchor == null || bodyRenderer == null)
            {
                return;
            }

            contactBoundsAnchor.localPosition = bodyRenderer.transform.localPosition;
            contactBoundsAnchor.localRotation = bodyRenderer.transform.localRotation;
            contactBoundsAnchor.localScale = bodyRenderer.transform.localScale;
        }

        public void SyncWeakZoneAnchorToVisual()
        {
            if (weakZoneAnchor == null || weakZoneRenderer == null)
            {
                return;
            }

            weakZoneAnchor.localPosition = weakZoneRenderer.transform.localPosition;
            weakZoneAnchor.localRotation = weakZoneRenderer.transform.localRotation;
            weakZoneAnchor.localScale = weakZoneRenderer.transform.localScale;
        }

        public Vector2 GetContactHalfExtents(PrototypeCombatConfig config)
        {
            if (bodyRenderer != null)
            {
                var bodyBounds = bodyRenderer.bounds.extents;
                return new Vector2(
                    Mathf.Max(1f, bodyBounds.x * config.pixelsPerUnit),
                    Mathf.Max(1f, bodyBounds.y * config.pixelsPerUnit));
            }

            if (contactBoundsAnchor != null)
            {
                var scale = contactBoundsAnchor.lossyScale;
                return new Vector2(
                    Mathf.Max(1f, scale.x * config.pixelsPerUnit * 0.5f),
                    Mathf.Max(1f, scale.y * config.pixelsPerUnit * 0.5f));
            }

            return new Vector2(config.boss.contactRadiusX, config.boss.contactRadiusY);
        }

        public float GetWeakZoneRadius(PrototypeCombatConfig config)
        {
            if (weakZoneRenderer != null)
            {
                var weakBounds = weakZoneRenderer.bounds.extents;
                return Mathf.Max(1f, Mathf.Max(weakBounds.x, weakBounds.y) * config.pixelsPerUnit);
            }

            if (weakZoneAnchor != null)
            {
                return Mathf.Max(1f, weakZoneAnchor.lossyScale.x * config.pixelsPerUnit * 0.5f);
            }

            return config.boss.weakZoneRadius;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SyncContactBoundsAnchorToVisual();
            SyncWeakZoneAnchorToVisual();
        }
#endif
    }
}
