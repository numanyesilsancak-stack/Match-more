
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Inputt
{
    public sealed class GameplayInput : System.IDisposable
    {
        private readonly GameplayInputActions _actions;
        public Vector2 PointerPosition { get; private set; }
        public bool PressedThisFrame { get; private set; }
        public bool ReleasedThisFrame { get; private set; }

        public GameplayInput()
        {
            _actions = new GameplayInputActions();
            _actions.Enable();

            _actions.Gameplay.PointerPosition.performed += OnPos;
            _actions.Gameplay.PointerPress.started += OnPress;
            _actions.Gameplay.PointerPress.canceled += OnRelease;
        }

        private void OnPos(InputAction.CallbackContext ctx) => PointerPosition = ctx.ReadValue<Vector2>();

        private void OnPress(InputAction.CallbackContext ctx) => PressedThisFrame = true;

        private void OnRelease(InputAction.CallbackContext ctx) => ReleasedThisFrame = true;

        public void LateTick()
        {
            PressedThisFrame = false;
            ReleasedThisFrame = false;
        }

        public void Dispose()
        {
            _actions.Gameplay.PointerPosition.performed -= OnPos;
            _actions.Gameplay.PointerPress.started -= OnPress;
            _actions.Gameplay.PointerPress.canceled -= OnRelease;
            _actions.Disable();
        }
    }
}
