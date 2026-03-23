using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Audio;
using Game.Core;
using Game.Economy;
using Game.Levels;
using Game.Save;
using Game.UI;
using UnityEngine;

namespace Game.Board
{
    public sealed class BoardController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private BoardView view;
        [SerializeField] private BoardTouchInput input;
        [SerializeField] private BoardAudio audioFX;
        [SerializeField] private Camera uiCamera;

        [Header("DB")]
        [SerializeField] private LevelDatabase levelDb;

        [Header("UI")]
        [SerializeField] private GoalBarUI goalBar;   // ← TopBar'daki GoalBarUI

        [Header("Timing")]
        [SerializeField] private float swapAnimTime = 0.08f;
        [SerializeField] private float dropAnimTime = 0.06f;

        private BoardModel _model;
        private readonly List<int> _matches = new(128);

        private LevelData _level;
        private LevelSession _session;
        public LevelSession Session => _session;
        public LevelData Level => _level;

        private ISaveService _save;
        private ICurrencyService _currency;
        private ProgressService _progress;

        private BoardSwapAnimator _swapAnim;
        private BoardResolver _resolver;

        private bool _busy;

        private void Awake()
        {
            _save = Services.Save;
            _currency = Services.Currency;
            _progress = new ProgressService(_save, _currency);

            if (!view) view = GetComponentInChildren<BoardView>(true);
            if (!input) input = GetComponentInChildren<BoardTouchInput>(true);
            if (!audioFX) audioFX = GetComponentInChildren<BoardAudio>(true);

            DOTween.SetTweensCapacity(700, 50);
        }

        private void OnEnable()
        {
            if (input) input.SwapRequested += OnSwapRequested;

            // Goal tamamlanınca ses çal
            EventBus.GoalCompleted += OnGoalCompleted;
        }

        private void OnDisable()
        {
            if (input) input.SwapRequested -= OnSwapRequested;

            EventBus.GoalCompleted -= OnGoalCompleted;
        }

        private void Start()
        {
            int lvl = Mathf.Clamp(_save.Data.currentLevel, 1, 20);
            _level = levelDb.Get(lvl);

            _model = new BoardModel(_level.width, _level.height);
            _session = new LevelSession(_level, _currency);

            _swapAnim = new BoardSwapAnimator(view);
            _resolver = new BoardResolver(view, audioFX);

            EventBus.RaiseLevelChanged(lvl);

            // Goal slotlarını oluştur
            goalBar?.BuildSlots(_session);

            BoardInitializer.Init(_model, view, _level, _matches);

            EventBus.RaiseMovesChanged(_session.MovesLeft);
            EventBus.RaiseScoreChanged(_session.Score);
            EventBus.RaiseComboChanged(_session.ComboStreak);
        }

        // ── Input ─────────────────────────────────────────────────────────────

        private void OnSwapRequested(int x1, int y1, int x2, int y2)
        {
            if (_busy) return;
            if (!_session.CanMakeMove) return;

            if ((uint)x1 >= (uint)_model.w || (uint)y1 >= (uint)_model.h) return;
            if ((uint)x2 >= (uint)_model.w || (uint)y2 >= (uint)_model.h) return;

            StartCoroutine(TrySwapRoutine(x1, y1, x2, y2));
        }

        // ── Goal ses ─────────────────────────────────────────────────────────

        private void OnGoalCompleted(TileType _) => audioFX?.PlayGoalComplete();

        // ── Swap routine ──────────────────────────────────────────────────────

        private IEnumerator TrySwapRoutine(int x1, int y1, int x2, int y2)
        {
            if (_busy) yield break;
            _busy = true;

            _session.ConsumeMove();
            _session.ResetCombo();
            EventBus.RaiseMovesChanged(_session.MovesLeft);

            var uiA = view.GetTileUI(x1, y1);
            var uiB = view.GetTileUI(x2, y2);

            if (uiA == null || uiB == null) { _busy = false; yield break; }

            audioFX?.PlaySwap();

            yield return _swapAnim.Play(uiA, uiB, x1, y1, x2, y2, swapAnimTime);

            view.SwapTileEntries(x1, y1, x2, y2);
            SwapTypesModelOnly(x1, y1, x2, y2);
            view.RefreshTile(uiA, x2, y2, _model.types[x2, y2]);
            view.RefreshTile(uiB, x1, y1, _model.types[x1, y1]);

            if (MatchFinder.FindAllMatches(_model, _matches) <= 0)
            {
                yield return _swapAnim.Play(uiA, uiB, x2, y2, x1, y1, swapAnimTime);

                view.SwapTileEntries(x1, y1, x2, y2);
                SwapTypesModelOnly(x1, y1, x2, y2);
                view.RefreshTile(uiA, x1, y1, _model.types[x1, y1]);
                view.RefreshTile(uiB, x2, y2, _model.types[x2, y2]);

                _busy = false;

                if (_session.IsLose())
                    EventBus.RaiseToast("Out of moves! You can buy moves");

                yield break;
            }

            yield return _resolver.Resolve(_model, _level, _session, _matches, dropAnimTime);

            EventBus.RaiseScoreChanged(_session.Score);
            EventBus.RaiseComboChanged(_session.ComboStreak);

            if (_session.IsWin())
            {
                int lvl = _save.Data.currentLevel;
                int reward = _session.CalculateWinGoldReward(lvl);
                _progress.CompleteLevel(reward);
                EventBus.RaiseToast($"+{reward} Gold");

                int next = Mathf.Clamp(_save.Data.currentLevel, 1, 20);
                _level = levelDb.Get(next);
                _model = new BoardModel(_level.width, _level.height);
                _session = new LevelSession(_level, _currency);

                EventBus.RaiseLevelChanged(next);

                // Yeni levelin goal slotlarını oluştur
                goalBar?.BuildSlots(_session);

                BoardInitializer.Init(_model, view, _level, _matches);

                EventBus.RaiseMovesChanged(_session.MovesLeft);
                EventBus.RaiseScoreChanged(_session.Score);
                EventBus.RaiseComboChanged(_session.ComboStreak);

                _busy = false;
                yield break;
            }

            if (_session.IsLose())
                EventBus.RaiseToast("Out of moves! You can buy moves");

            _busy = false;
        }

        private void SwapTypesModelOnly(int x1, int y1, int x2, int y2)
        {
            var t = _model.types[x1, y1];
            _model.types[x1, y1] = _model.types[x2, y2];
            _model.types[x2, y2] = t;
        }
    }
}