using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossFirePointRig : MonoBehaviour
    {
        [SerializeField] private Transform centerMuzzle;
        [SerializeField] private Transform leftMuzzle;
        [SerializeField] private Transform rightMuzzle;
        [SerializeField] private Transform topMuzzle;
        [SerializeField] private Transform lowerMuzzle;
        [SerializeField] private Transform weakZoneAnchor;

        public Transform CenterMuzzle => centerMuzzle != null ? centerMuzzle : transform;
        public Transform LeftMuzzle => leftMuzzle != null ? leftMuzzle : CenterMuzzle;
        public Transform RightMuzzle => rightMuzzle != null ? rightMuzzle : CenterMuzzle;
        public Transform TopMuzzle => topMuzzle != null ? topMuzzle : CenterMuzzle;
        public Transform LowerMuzzle => lowerMuzzle != null ? lowerMuzzle : CenterMuzzle;
        public Transform WeakZoneAnchor => weakZoneAnchor != null ? weakZoneAnchor : transform;
    }
}
