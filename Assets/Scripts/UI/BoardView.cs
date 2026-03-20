using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using DG.Tweening;

namespace Game.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private TileUI tilePrefab;
        [SerializeField] private Sprite[] tileSprites; // index: (int)TileType
        
        [Header("Particles")]
        [SerializeField] private ParticleSystem vfxMatch3;
        [SerializeField] private ParticleSystem vfxMatch4;
        [SerializeField] private ParticleSystem vfxMatch5;
        [SerializeField] private ParticleSystem vfxQuad;
        [SerializeField] private ParticleSystem vfxComplex; // T and L

        [Header("Config")]
        [SerializeField] private int prewarm = 80;

        private Pool<TileUI> _pool;
        private readonly Dictionary<int, TileUI> _tiles = new();

        private float _cellSize;
        private float _boardSize;
        private Vector2 _origin;

        public int W { get; private set; }
        public int H { get; private set; }

        public void Init(int w, int h)
        {
            W = w;
            H = h;

            _pool ??= new Pool<TileUI>(tilePrefab, boardRoot, prewarm);

            // kare alana sığdır
            var rect = boardRoot.rect;
            _boardSize = Mathf.Min(rect.width, rect.height);
            _cellSize = _boardSize / W;

            // pivot center varsayımı
            _origin = new Vector2(-_boardSize * 0.5f + _cellSize * 0.5f,
                                  -_boardSize * 0.5f + _cellSize * 0.5f);

            // root'u kare yapmak 
            boardRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _boardSize);
            boardRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _boardSize);
        }

        public void ClearAll()
        {
            foreach (var ui in _tiles.Values)
                _pool.Release(ui);

            _tiles.Clear();
        }

        public void SetTile(int x, int y, TileType type)
        {
            int key = Key(x, y);

            if (!_tiles.TryGetValue(key, out var ui))
            {
                ui = _pool.Get();
                ui.transform.SetParent(boardRoot, false);
                _tiles.Add(key, ui);
            }

            ui.Set(x, y, type, tileSprites[(int)type]);

            var rt = ui.Rt;
            rt.anchoredPosition = CellToLocal(x, y);

            float tileSize = _cellSize * 0.92f;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tileSize);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tileSize);
        }

        public void RemoveTile(int x, int y)
        {
            int key = Key(x, y);
            if (!_tiles.Remove(key, out var ui)) return;

            _pool.Release(ui);
        }

        public Vector2 CellToLocal(int x, int y)
        {
            // y aşağıdan yukarı
            return _origin + new Vector2(x * _cellSize, y * _cellSize);
        }

        public bool ScreenToCell(Vector2 screenPos, Camera uiCam, out int x, out int y)
        {
            x = y = -1;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRoot, screenPos, uiCam, out var local))
                return false;

            // local pivot center
            float half = _boardSize * 0.5f;
            float lx = local.x + half;
            float ly = local.y + half;

            if (lx < 0f || ly < 0f || lx >= _boardSize || ly >= _boardSize)
                return false;

            x = Mathf.Clamp(Mathf.FloorToInt(lx / _cellSize), 0, W - 1);
            y = Mathf.Clamp(Mathf.FloorToInt(ly / _cellSize), 0, H - 1);
            return true;
        }

        public TileUI GetTileUI(int x, int y)
        {
            _tiles.TryGetValue(Key(x, y), out var ui);
            return ui;
        }

        private int Key(int x, int y) => x + y * W; // 8x8 için güvenli, genel olarak da çakışmasız


        public TileUI PopTileUI(int x, int y)
        {
            int key = Key(x, y);
            if (_tiles.TryGetValue(key, out var ui))
            {
                _tiles.Remove(key);
                return ui;
            }
            return null;
        }


        public void PutTileUI(int x, int y, TileUI ui)
        {
            int key = Key(x, y);
            _tiles[key] = ui;
        }


        public void ReleaseTileUI(TileUI ui)
        {
            _pool.Release(ui);
        }

        public void UpdateTileData(TileUI ui, int x, int y, TileType type)
        {
            // sprite aynı type için güncellensin
            ui.Set(x, y, type, tileSprites[(int)type]);
        }

        public Tween TweenMoveToCell(TileUI ui, int x, int y, float duration)
        {
            return ui.Rt.DOAnchorPos(CellToLocal(x, y), duration)
                .SetEase(Ease.OutQuad);
        }

        public Tween TweenSpawnFromAbove(TileUI ui, int x, int y, float duration, float extraHeight = 2.5f)
        {
            // üstten gelsin
            var target = CellToLocal(x, y);
            ui.Rt.anchoredPosition = target + new Vector2(0f, _cellSize * extraHeight);

            return ui.Rt.DOAnchorPos(target, duration)
                .SetEase(Ease.OutBounce);
        }

        public Tween TweenPopAndShrink(TileUI ui, float duration)
        {
            ui.transform.localScale = Vector3.one;
            // küçük bir pop, sonra shrink
            var seq = DOTween.Sequence();
            seq.Append(ui.transform.DOPunchScale(Vector3.one * 0.15f, duration * 0.45f, vibrato: 6, elasticity: 0.5f));
            seq.Append(ui.transform.DOScale(0f, duration * 0.55f).SetEase(Ease.InBack));
            return seq;
        }
        public void SwapTileEntries(int x1, int y1, int x2, int y2)
        {
            int k1 = Key(x1, y1);
            int k2 = Key(x2, y2);

            _tiles.TryGetValue(k1, out var a);
            _tiles.TryGetValue(k2, out var b);

            if (a != null) _tiles[k2] = a; else _tiles.Remove(k2);
            if (b != null) _tiles[k1] = b; else _tiles.Remove(k1);
        }


        public void RefreshTile(TileUI ui, int x, int y, TileType type)
        {
            // UI içindeki sprite günceller + X/Y/Type set eder
            ui.Set(x, y, type, tileSprites[(int)type]);
        }

        public void PlayMatchVfx(List<int> matches, int matchLen, bool isQuad, bool isComplex)
        {
            ParticleSystem ps = vfxMatch3;

            if (isComplex) ps = vfxComplex;
            else if (isQuad) ps = vfxQuad;
            else if (matchLen >= 5) ps = vfxMatch5;
            else if (matchLen == 4) ps = vfxMatch4;

            if (ps == null) return;

            foreach (var p in matches)
            {
                MatchKey.Decode(p, out int x, out int y);
                var pos = CellToLocal(x, y);
                var instance = Instantiate(ps, boardRoot, false);
                instance.transform.localPosition = pos;
                // instance auto-destroys if configured so in Unity (Main module -> Stop Action: Destroy)
                // usually we just let it play and it'll stop or destroy itself
                instance.Play();
            }
        }

    } 
}

    
