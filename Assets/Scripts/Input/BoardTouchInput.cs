using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Game.Core;

namespace Game.Board
{
    public sealed class BoardTouchInput : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private BoardView view;
        [SerializeField] private Camera uiCamera; // Canvas Overlay ise null

        [Header("Tuning")]
        [SerializeField] private float swipeThresholdPx = 18f;

        public event Action<int, int, int, int> SwapRequested;

        private bool _pressed;
        private int _pressX, _pressY;
        private Vector2 _pressPos;

        private void OnEnable()
        {
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerMove += OnFingerMove;
            Touch.onFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerMove -= OnFingerMove;
            Touch.onFingerUp -= OnFingerUp;

            _pressed = false;
        }

        private void OnFingerDown(Finger finger)
        {
            if (InputGate.Blocked) return;

            var pos = finger.screenPosition;
            
            // Eğer BoardView üzerinden bir kamera tanımlandıysa onu kullan (Camera modu için zorunlu), aksi halde eskisini fallback olarak kullan.
            Camera camToUse = (view != null && view.CanvasCam != null) ? view.CanvasCam : uiCamera;
            
            if (!view || !view.ScreenToCell(pos, camToUse, out var x, out var y)) return;

            _pressed = true;
            _pressPos = pos;
            _pressX = x;
            _pressY = y;
        }

        private void OnFingerMove(Finger finger)
        {
            if (!_pressed) return;

            if (InputGate.Blocked) { _pressed = false; return; }

            var delta = finger.screenPosition - _pressPos;
            if (delta.sqrMagnitude < swipeThresholdPx * swipeThresholdPx) return;

            int dx = 0, dy = 0;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) dx = delta.x > 0 ? 1 : -1;
            else dy = delta.y > 0 ? 1 : -1;

            int x2 = _pressX + dx;
            int y2 = _pressY + dy;

            _pressed = false;

            // controller sınır kontrolünü yapsın 
            SwapRequested?.Invoke(_pressX, _pressY, x2, y2);
        }

        private void OnFingerUp(Finger finger) => _pressed = false;
    }
}