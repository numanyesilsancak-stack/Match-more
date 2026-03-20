using System.Collections;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public static SceneLoader I { get; private set; }

        [Header("Loader Prefab")]
        [SerializeField] private LoadingView loadingPrefab; // senin LoadUI prefabındaki LoadingView component

        [Header("Canvas")]
        [SerializeField] private int sortingOrder = 999;
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
        [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

        [Header("Timing")]
        [SerializeField] private float minShowTime = 0.15f;
        [SerializeField] private float destroyDelay = 0.20f; // fadeOut bitmeden destroy olmasın

        private bool _loading;

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Load(string sceneName)
        {
            if (_loading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;

            // 1) Loader UI spawn
            var overlayRoot = new GameObject("~LoadingOverlayRoot");
            DontDestroyOnLoad(overlayRoot);

            var canvasGO = new GameObject("LoadingCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvasGO.transform.SetParent(overlayRoot.transform, false);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = matchWidthOrHeight;

            LoadingView view = null;
            if (loadingPrefab != null)
            {
                view = Instantiate(loadingPrefab, canvasGO.transform, false);
                view.Show();
                view.SetProgress01(0f);
            }
            else
            {
                Debug.LogError("SceneLoader: loadingPrefab boş. Boot sahnesinde SceneLoader Inspector'dan LoadUI prefabını ata!", this);
            }

            // UI'nin bir frame çizilmesi için
            yield return null;

            float t0 = Time.unscaledTime;

            // 2) Async load
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (view) view.SetProgress01(op.progress / 0.9f);
                yield return null;
            }

            if (view) view.SetProgress01(1f);

            // min show time
            float elapsed = Time.unscaledTime - t0;
            if (elapsed < minShowTime)
                yield return new WaitForSecondsRealtime(minShowTime - elapsed);

            // 3) Activate
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            // 4) Hide + destroy overlay
            if (view) view.Hide();

            yield return new WaitForSecondsRealtime(destroyDelay);
            Destroy(overlayRoot);

            _loading = false;
        }
    }
}