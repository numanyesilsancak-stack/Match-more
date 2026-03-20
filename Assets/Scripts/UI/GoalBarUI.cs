using Game.Board;
using Game.Levels;
using UnityEngine;
using EventBus = Game.Core.EventBus;   // Unity.VisualScripting.EventBus ile çakışmayı önlet

namespace Game.UI
{
    public sealed class GoalBarUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform slotsContainer;
        [SerializeField] private GoalSlotUI goalSlotPrefab;

        private GoalSlotUI[] _slots;

        private void OnEnable()
        {
            EventBus.LevelChanged += OnLevelChanged;
            EventBus.GoalProgressChanged += OnProgressChanged;
            EventBus.GoalCompleted += OnGoalCompleted;
        }

        private void OnDisable()
        {
            EventBus.LevelChanged -= OnLevelChanged;
            EventBus.GoalProgressChanged -= OnProgressChanged;
            EventBus.GoalCompleted -= OnGoalCompleted;
        }

        public void BuildSlots(LevelSession session)
        {
            ClearSlots();

            var goals = session?.GetGoals();
            if (goals == null || goals.Length == 0) return;

            _slots = new GoalSlotUI[goals.Length];

            for (int i = 0; i < goals.Length; i++)
            {
                var slot = Object.Instantiate(goalSlotPrefab, slotsContainer);
                slot.Setup(goals[i].tileType, goals[i].icon, 0, goals[i].requiredCount);
                _slots[i] = slot;
            }
        }

        private void ClearSlots()
        {
            if (_slots != null)
            {
                foreach (var s in _slots)
                    if (s) Destroy(s.gameObject);
            }
            _slots = null;
        }

        private void OnLevelChanged(int _) => ClearSlots();

        private void OnProgressChanged(TileType type, int current, int required)
        {
            if (_slots == null) return;
            foreach (var s in _slots)
                s?.OnProgressChanged(type, current, required);
        }

        private void OnGoalCompleted(TileType type)
        {
            if (_slots == null) return;
            foreach (var s in _slots)
                s?.OnGoalCompleted(type);
        }
    }
}