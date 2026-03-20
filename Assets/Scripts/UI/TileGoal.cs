using System;
using UnityEngine;
using Game.Board;

namespace Game.Levels
{
    [Serializable]
    public struct TileGoal
    {
        public TileType tileType;
        public int requiredCount;
        public Sprite icon;
    }
}