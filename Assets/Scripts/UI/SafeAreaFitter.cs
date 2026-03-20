using UnityEngine;
namespace Game.UI
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafe;
        private void Awake()
        {
            _rt = (RectTransform)transform;
            Apply();
        }
        private void Update()
        {
            if (Screen.safeArea != _lastSafe) Apply();
        }
        private void Apply()
        {
            _lastSafe = Screen.safeArea;
            var min = _lastSafe.position;
            var max = _lastSafe.position + _lastSafe.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}