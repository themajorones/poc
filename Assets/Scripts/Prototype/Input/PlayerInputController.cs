using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private InputAdapter adapter;

        private GameController _game;
        private int _activePointerId = int.MinValue;
        private bool _pointerActive;
        private Vector2 _lastPointerPosition;
        private Vector2 _lastPointerScreenPosition;
        private float _lastPointerTime;
        private float _gestureUpDistance;
        private float _gestureTimer;

        public Vector2 TargetLogicalPosition { get; private set; }
        public bool PointerActive => _pointerActive;

        public void Initialize(GameController game)
        {
            _game = game;
            if (adapter == null)
            {
                adapter = gameObject.GetComponent<InputAdapter>();
                if (adapter == null)
                {
                    adapter = gameObject.AddComponent<InputAdapter>();
                }
            }

            adapter.Initialize(game.References);
            ResetState();
        }

        public void ResetState()
        {
            CancelPointerTracking();
            var width = _game != null && _game.Config != null ? _game.Config.logicalWidth : PrototypeCombatConfig.LogicalWidth;
            var height = _game != null && _game.Config != null ? _game.Config.logicalHeight : PrototypeCombatConfig.LogicalHeight;
            TargetLogicalPosition = new Vector2(width * 0.5f, height * 0.79f);
        }

        public void CancelPointerTracking()
        {
            _pointerActive = false;
            _activePointerId = int.MinValue;
            _lastPointerPosition = default;
            _lastPointerScreenPosition = default;
            _lastPointerTime = 0f;
            _gestureUpDistance = 0f;
            _gestureTimer = 0f;
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            if (adapter.RestartPressedThisFrame())
            {
                if (_game.IsTutorialMode && _game.TutorialController != null)
                {
                    _game.TutorialController.HandleRestartShortcut();
                }
                else
                {
                    _game.RestartRun();
                }

                return;
            }

            if (_game.State != GameController.RunState.Playing)
            {
                if (!_game.IsTutorialMode && adapter.StartPressedThisFrame())
                {
                    _game.StartRun();
                }

                return;
            }

            if (adapter.SkillPressedThisFrame())
            {
                _game.SkillController.TryActivateSkill();
            }

            if (!_pointerActive && adapter.TryGetPointerDown(out _activePointerId, out var pointerDown))
            {
                var pointerDownScreen = adapter.TryGetPointerDownScreen(out _, out var screenPosition)
                    ? screenPosition
                    : pointerDown;

                if (adapter.IsScreenPositionOverUi(pointerDownScreen))
                {
                    CancelPointerTracking();
                    return;
                }

                _pointerActive = true;
                _lastPointerPosition = pointerDown;
                _lastPointerScreenPosition = pointerDownScreen;
                _lastPointerTime = Time.unscaledTime;
                _gestureUpDistance = 0f;
                _gestureTimer = 0f;
                BeginRelativeControl();
            }

            if (!_pointerActive)
            {
                return;
            }

            if (!adapter.TryGetPointer(_activePointerId, out var pointerPosition, out var released))
            {
                return;
            }

            var screenPointerPosition = pointerPosition;
            if (adapter.TryGetPointerScreen(_activePointerId, out var currentScreenPointer, out _))
            {
                screenPointerPosition = currentScreenPointer;
            }

            var now = Time.unscaledTime;
            var dt = Mathf.Max(0.001f, now - _lastPointerTime);
            var screenDelta = screenPointerPosition - _lastPointerScreenPosition;
            var logicalUnitsPerScreenY = _game.Config.logicalHeight / Mathf.Max(1f, Screen.height);
            var gestureDeltaY = screenDelta.y * logicalUnitsPerScreenY;
            var verticalSpeed = gestureDeltaY / dt;

            ApplyTargetDelta(pointerPosition - _lastPointerPosition);

            if (dt > _game.Config.parry.travelWindow)
            {
                _gestureUpDistance = 0f;
                _gestureTimer = 0f;
            }

            if (gestureDeltaY > 0f)
            {
                _gestureUpDistance += gestureDeltaY;
                _gestureTimer += dt;
            }
            else if (gestureDeltaY < -2f)
            {
                _gestureUpDistance = Mathf.Max(0f, _gestureUpDistance - (-gestureDeltaY) * 0.85f);
                if (gestureDeltaY < -7f)
                {
                    _gestureTimer = 0f;
                }
            }

            var parry = _game.Config.parry;
            var upwardSpeedBurst = gestureDeltaY > 3f && verticalSpeed > parry.threshold;
            var upwardTravelBurst =
                gestureDeltaY > 2f &&
                _gestureTimer <= parry.travelWindow &&
                _gestureUpDistance >= parry.travelThreshold;

            if ((upwardSpeedBurst || upwardTravelBurst) &&
                !_game.SkillController.IsCasting &&
                !_game.Player.IsBursting)
            {
                _game.Player.TriggerParryBurst();
                _gestureUpDistance = 0f;
                _gestureTimer = 0f;
            }

            _lastPointerPosition = pointerPosition;
            _lastPointerScreenPosition = screenPointerPosition;
            _lastPointerTime = now;

            if (released)
            {
                CancelPointerTracking();
            }
        }

        private void BeginRelativeControl()
        {
            if (_game.Player == null || _game.Player.IsBursting)
            {
                return;
            }

            TargetLogicalPosition = ClampTarget(_game.Player.LogicalPosition);
        }

        private void ApplyTargetDelta(Vector2 logicalDelta)
        {
            TargetLogicalPosition = ClampTarget(TargetLogicalPosition + logicalDelta);
        }

        private Vector2 ClampTarget(Vector2 target)
        {
            return _game.References.ClampLogicalPosition(target, 22f, 16f, 30f);
        }
    }
}
