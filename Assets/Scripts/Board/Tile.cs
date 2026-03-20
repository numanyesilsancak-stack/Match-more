using UnityEngine;

namespace Game.Board
{
    public enum TileType {RootRed, RootBlue, RootGreen, RootYellow, RootPurple, RootOrange}

    public sealed class TileView : MonoBehaviour
    {
        public TileType type;
        public int x, y;

        // Görsel sprite ile bağla
        
    }
}
