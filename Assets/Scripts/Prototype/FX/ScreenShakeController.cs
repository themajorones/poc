using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class ScreenShakeController : MonoBehaviour
    {
        [SerializeField] private Transform shakeTarget;

        private GameController _game;
        private float _magnitude;
        private float _duration;
        private Vector3 _baseLocalPosition;
        private bool _capturedBasePosition;

        public void Initialize(GameController game)
        {
            _game = game;
            if (shakeTarget == null && game.References != null && game.References.GameplayCamera != null)
            {
                shakeTarget = game.References.GameplayCamera.transform.parent != null
                    ? game.References.GameplayCamera.transform.parent
                    : game.References.GameplayCamera.transform;
            }

            if (shakeTarget != null)
            {
                _baseLocalPosition = shakeTarget.localPosition;
                _capturedBasePosition = true;
            }
        }

        public void Bump(float magnitude, float duration)
        {
            _magnitude = Mathf.Max(_magnitude, magnitude);
            _duration = Mathf.Max(_duration, duration);
        }

        private void LateUpdate()
        {
            if (_game == null || shakeTarget == null)
            {
                return;
            }

            if (!_capturedBasePosition)
            {
                _baseLocalPosition = shakeTarget.localPosition;
                _capturedBasePosition = true;
            }

            if (_duration > 0f)
            {
                _duration = Mathf.Max(0f, _duration - Time.deltaTime);
                var shakeOffset = (Vector3)(Random.insideUnitCircle * _magnitude);
                shakeTarget.localPosition = _baseLocalPosition + shakeOffset;
                _magnitude = Mathf.Lerp(_magnitude, 0f, Time.deltaTime * 12f);
            }
            else
            {
                shakeTarget.localPosition = Vector3.Lerp(shakeTarget.localPosition, _baseLocalPosition, Time.deltaTime * 20f);
            }
        }
    }
}
