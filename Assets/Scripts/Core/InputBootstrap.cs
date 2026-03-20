using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Input
{
    public sealed class InputBootstrap : MonoBehaviour
    {
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();

#if UNITY_EDITOR
            TouchSimulation.Enable(); // Editor mouse ile dokunuş simülasyonu
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }
    }
}