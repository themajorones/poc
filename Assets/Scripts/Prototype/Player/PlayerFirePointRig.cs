using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerFirePointRig : MonoBehaviour
    {
        [SerializeField] private Transform primaryShotPoint;
        [SerializeField] private Transform leftShotPoint;
        [SerializeField] private Transform rightShotPoint;
        [SerializeField] private Transform laserOrigin;

        public Transform PrimaryShotPoint => primaryShotPoint != null ? primaryShotPoint : transform;
        public Transform LeftShotPoint => leftShotPoint != null ? leftShotPoint : PrimaryShotPoint;
        public Transform RightShotPoint => rightShotPoint != null ? rightShotPoint : PrimaryShotPoint;
        public Transform LaserOrigin => laserOrigin != null ? laserOrigin : PrimaryShotPoint;
    }
}
