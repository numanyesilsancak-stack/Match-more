using UnityEngine;

namespace Game.Core
{
    public sealed class OrientationLock : MonoBehaviour
    {
        private void Awake()
        {
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = true;
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}
