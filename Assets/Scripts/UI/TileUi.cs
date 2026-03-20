using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Board
{
    public sealed class TileUI : MonoBehaviour
    {
        

        [SerializeField] private Image image;
        public RectTransform Rt { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }
        public TileType Type { get; private set; }

        private void Awake()
        {
            Rt = (RectTransform)transform;
        }

        public void Set(int x, int y, TileType type, Sprite sprite)
        {
            X = x; Y = y; Type = type;
            image.sprite = sprite;
        }
        private void OnDisable()
        {
            transform.DOKill();
            Rt.DOKill();
            transform.localScale = Vector3.one;
        }
    }
}
