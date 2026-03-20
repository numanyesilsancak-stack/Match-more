using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Game.Levels;

namespace Game.Board
{
    public sealed class BoardResolver
    {
        private readonly BoardView _view;
        private readonly BoardAudio _audio;
        private readonly WaitForSeconds _tinyWait = new WaitForSeconds(0.08f);

        public BoardResolver(BoardView view, BoardAudio audio)
        {
            _view = view;
            _audio = audio;
        }

        public IEnumerator Resolve(BoardModel model, LevelData level, LevelSession session, List<int> matches, float dropAnimTime)
        {
            const float clearDur = 0.14f;
            float moveDur = dropAnimTime;
            int cascadeIndex = 0;

            while (MatchFinder.FindAllMatches(model, matches) > 0)
            {
                var matchInfo = MatchMetrics.AnalyzeMatch(model, matches);
                _audio?.PlayMatchSfx(matchInfo.IsQuad ? 4 : matchInfo.MaxLen, cascadeIndex);
                _view.Vfx.PlayMatchVfx(matches, matchInfo.MaxLen, matchInfo.IsQuad, matchInfo.IsComplex);
                cascadeIndex++;

                int cleared = matches.Count;
                session.AddMatchScore(cleared);

                bool[,] clearedMap = new bool[model.w, model.h];
                foreach (var p in matches)
                {
                    MatchKey.Decode(p, out int x, out int y);
                    clearedMap[x, y] = true;
                }

                // clear anim — her tile için goal sayacını güncelle
                var clearSeq = DOTween.Sequence();
                foreach (var p in matches)
                {
                    MatchKey.Decode(p, out int x, out int y);

                    // ← TEK EKLEDİĞİMİZ SATIR
                    session.CollectTile(model.types[x, y]);

                    var ui = _view.PopTileUI(x, y);
                    if (ui == null) continue;

                    ui.transform.DOKill();
                    ui.Rt.DOKill();

                    clearSeq.Join(_view.TweenPopAndShrink(ui, clearDur)
                        .OnComplete(() => _view.ReleaseTileUI(ui)));
                }
                yield return clearSeq.WaitForCompletion();

                // drop + refill
                var dropSeq = DOTween.Sequence();

                for (int x = 0; x < model.w; x++)
                {
                    int writeY = 0;

                    for (int y = 0; y < model.h; y++)
                    {
                        if (clearedMap[x, y]) continue;

                        if (writeY != y)
                            model.types[x, writeY] = model.types[x, y];

                        var ui = _view.PopTileUI(x, y);
                        if (ui != null)
                        {
                            ui.transform.DOKill();
                            ui.Rt.DOKill();

                            _view.PutTileUI(x, writeY, ui);
                            _view.RefreshTile(ui, x, writeY, model.types[x, writeY]);

                            dropSeq.Join(_view.TweenMoveToCell(ui, x, writeY, moveDur));
                        }

                        writeY++;
                    }

                    for (int y = writeY; y < model.h; y++)
                    {
                        var type = (TileType)Random.Range(0, level.tileTypes);
                        model.types[x, y] = type;

                        _view.SetTile(x, y, type);
                        var ui = _view.GetTileUI(x, y);
                        if (ui != null)
                        {
                            ui.transform.DOKill();
                            ui.Rt.DOKill();
                            ui.transform.localScale = Vector3.one;

                            dropSeq.Join(_view.TweenSpawnFromAbove(ui, x, y, moveDur, extraHeight: 2.0f));
                        }
                    }
                }

                yield return dropSeq.WaitForCompletion();
                yield return _tinyWait;
            }
        }
    }
}