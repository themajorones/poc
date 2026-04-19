using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CupHeadClone.Prototype
{
    public sealed class InputAdapter : MonoBehaviour
    {
        private Camera _camera;
        private GameReferences _references;
        private readonly List<RaycastResult> _uiRaycastResults = new();

        public void Initialize(GameReferences references)
        {
            _references = references;
            _camera = references.GameplayCamera;
        }

        public bool TryGetPointerDown(out int pointerId, out Vector2 logicalPosition)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    pointerId = touch.fingerId;
                    logicalPosition = ScreenToLogical(touch.position);
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                pointerId = -1;
                logicalPosition = ScreenToLogical(Input.mousePosition);
                return true;
            }

            pointerId = int.MinValue;
            logicalPosition = default;
            return false;
        }

        public bool TryGetPointerDownScreen(out int pointerId, out Vector2 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    pointerId = touch.fingerId;
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                pointerId = -1;
                screenPosition = Input.mousePosition;
                return true;
            }

            pointerId = int.MinValue;
            screenPosition = default;
            return false;
        }

        public bool TryGetPointer(int activePointerId, out Vector2 logicalPosition, out bool released)
        {
            if (Input.touchCount > 0)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.fingerId != activePointerId)
                    {
                        continue;
                    }

                    logicalPosition = ScreenToLogical(touch.position);
                    released = touch.phase is TouchPhase.Ended or TouchPhase.Canceled;
                    return true;
                }
            }

            if (activePointerId == -1)
            {
                logicalPosition = ScreenToLogical(Input.mousePosition);
                released = Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0);
                return Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);
            }

            logicalPosition = default;
            released = false;
            return false;
        }

        public bool TryGetPointerScreen(int activePointerId, out Vector2 screenPosition, out bool released)
        {
            if (Input.touchCount > 0)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.fingerId != activePointerId)
                    {
                        continue;
                    }

                    screenPosition = touch.position;
                    released = touch.phase is TouchPhase.Ended or TouchPhase.Canceled;
                    return true;
                }
            }

            if (activePointerId == -1)
            {
                screenPosition = Input.mousePosition;
                released = Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0);
                return Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);
            }

            screenPosition = default;
            released = false;
            return false;
        }

        public bool SkillPressedThisFrame()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E);
        }

        public bool RestartPressedThisFrame()
        {
            return Input.GetKeyDown(KeyCode.R);
        }

        public bool StartPressedThisFrame()
        {
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        }

        public bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return pointerId == -1
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        public bool IsScreenPositionOverUi(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            _uiRaycastResults.Clear();
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            EventSystem.current.RaycastAll(pointerData, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
        }

        public bool IsAnyPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (Input.touchCount > 0)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    {
                        return true;
                    }
                }
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private Vector2 ScreenToLogical(Vector2 screenPosition)
        {
            var world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            var logical = _references.WorldToLogical(world);
            return _references.ClampLogicalPosition(logical, 22f, 16f, 12f);
        }
    }
}
