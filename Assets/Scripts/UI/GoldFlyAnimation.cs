using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Game.Core;
using UnityEngine.Pool;

namespace Game.UI
{
    /// <summary>
    /// Gold Coin Fly to UI Animation System
    ///
    /// ALTYAPI:
    ///   CurrencyService.AddGold()  →  EventBus.RaiseGoldChanged(newTotal)
    ///   ToastPopupUI.Show("+X Gold")  →  GoldFlyAnimation.Instance.Play()
    ///
    /// GOLD TEXT MANTIĞI:
    ///    Animasyon yokken  → GoldChanged gelince text anında güncellenir
    ///    Animasyon aktifken → GoldChanged'dan gelen hedef değer saklanır,
    ///    ilk coin ikona değince AnimateCounter() o değere doğru sayar,
    ///    animasyon bitince doğrudan değer artar.
    ///    (böylece text hiçbir zaman animasyonsuz zıplamaz)
    ///
    /// INSPECTOR BAĞLANTILARI:
    ///   coinPrefab       → GoldCoin_Prefab (Image bileşenli, ~80x80px)
    ///   goldIconTarget   → Sağ üstteki altın ikonu RectTransform
    ///   goldCountText    → Altın sayısını gösteren TextMeshProUGUI
    ///   spawnParent      → Ana Canvas RectTransform
    ///   singleCoinSprite → Sprite sheet slice'tan ilk frame (tek düz coin)
    /// </summary>

    public sealed class GoldFlyAnimation : MonoBehaviour
    {
        public static GoldFlyAnimation Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] private RectTransform coinPrefab;
        [SerializeField] private RectTransform goldIconTarget;
        [SerializeField] private TextMeshProUGUI goldCountText;
        [SerializeField] private RectTransform spawnParent;

        [Header("Sprite")]
        [Tooltip("sprite sheetteki ilk frame: tek düz coin seçilecektir(sol üst köşe 1.)")]
        [SerializeField] private Sprite singleCoinSprite;

        [Header("Coin Count")]
        [Range(4, 14)]
        [SerializeField] private int coinCount = 8;

        [Header("Spawn Scatter")]
        [SerializeField] private float spawnRadius = 55f;
        [SerializeField] private float spreadDuration = 0.28f;

        [Header("Fly")]
        [SerializeField] private float firstCoinDelay = 0.12f;
        [SerializeField] private float staggerDelay = 0.07f;
        [SerializeField] private float flyDuration = 0.72f;
        [Tooltip("Bezier sapma kuvveti - büyük = daha kıvrık yol demektir")]
        [SerializeField] private float bezierStrength = 180f;

        [Header("Icon Bounce")]
        [SerializeField] private float bounceSale = 1.22f;
        [SerializeField] private float bounceDuration = 0.12f;

        [Header("Counter")]
        [SerializeField] private float counterDuration = 0.7f;

        [Header("Pool")]
        [Tooltip("Pool başlangıçta kaç coin yaratsın?")]
        [SerializeField] private int poolDefaultCapacity = 10;
        [Tooltip("Pool maksimum kaç coin tutsun?")]
        [SerializeField] private int poolMaxSize = 20;

        // -------RUNTIME-------
        // text de gösterilen değer
        private int _shownGold;
        // GoldChanged dan gelen güncel gerçek değer
        private int _targerGold;
        //Coin animsyonu sürüyor mu??
        private bool _isAnimating;

        private Tweener _counterTween;
        private Vector3 _iconOriginalScale;

        private IObjectPool<RectTransform> _pool;

        // -------UNITY-------
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            _pool = new ObjectPool<RectTransform>(
                createFunc: CreateCoin,
                actionOnGet: OnGetCoin,
                actionOnRelease: OnReleaseCoin,
                actionOnDestroy: OnDestroyCoin,
                collectionCheck: false,        //release de double-release konntrolü(hata oluşursa coinle true yap hatayı bul false yap true da hep kalmasın);
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );
        }
        private void Start()
        {
            if (goldIconTarget != null)
                _iconOriginalScale = goldIconTarget.localScale;
        }

        void OnEnable() => Game.Core.EventBus.GoldChanged += OnGoldChanged;
        void OnDisable() => Game.Core.EventBus.GoldChanged -= OnGoldChanged;

        // ---- POOL CALLBACKS ----
        private RectTransform CreateCoin()
        {
            var coin = Instantiate(coinPrefab, spawnParent);
            coin.gameObject.SetActive(false);
            return coin;
        }
        private void OnGetCoin(RectTransform coin)
        {
            coin.gameObject.SetActive(true);
            coin.localScale = Vector3.zero;

            if (singleCoinSprite != null)
            {
                var img = coin.GetComponent<Image>();
                if (img) img.sprite = singleCoinSprite;
            }
        }

        private void OnReleaseCoin(RectTransform coin)
        {
            // tweenleri durdur, havuza geri koy
            coin.DOKill();
            coin.localScale = Vector3.zero;
            coin.gameObject.SetActive(false);
        }

        private void OnDestroyCoin(RectTransform coin)
        {
            // pool maxsize ı aşınca unity şunu çağırsın
            if (coin != null)
            {
                Destroy(coin.gameObject);
            }
        }

        // EventBus.GoldChanged 
        // CurrencyService.AddGold / TrySpendGold her çağrıldığında gelir
        private void OnGoldChanged(int newTotal)
        {
            _targerGold = newTotal;

            if (!_isAnimating)
            {
                // animasyon yoksa text i direkt güncelle
                _shownGold = newTotal;
                if (goldCountText != null)
                    goldCountText.text = newTotal.ToString();
            }
            // animasyon aktifse _targetGold saklandı
            // animateCounter() ilk coin ikonuna değince onu kullanacak
        }
        // Entry point ----ToastPopUI buraya erişecek ----
        public void Play(int amount, Vector2 spawnUIPos)
        {
            StartCoroutine(RunAnimation(spawnUIPos));
        }
        // -------- Ana animasyon --------
        private IEnumerator RunAnimation(Vector2 spawnCenter)
        {
            _isAnimating = true;

            //-1 pool dan coinleri çek
            var coins = new List<RectTransform>(coinCount);
            for (int i = 0; i < coinCount; i++)
            {
                //(bu satır ilerideki sürümlerde yöntem bellek sorunları yaratabileceğinden kaldırıldı!) var coin = Instantiate(coinPrefab, spawnParent);
                var coin = _pool.Get();
                coin.SetParent(spawnParent, false);
                coin.anchoredPosition = spawnCenter;
                coins.Add(coin);
            }

            //-2 spread yap pop ile dışa yayıl
            var spreadSeq = DOTween.Sequence();
            for (int i = 0; i < coinCount; i++)
            {
                var coin = coins[i];
                float angle = (360 / coinCount) * i + Random.Range(-15f, 15f);
                float radius = spawnRadius * Random.Range(0.75f, 1.25f);
                Vector2 dest = spawnCenter + new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );
                float delay = i * 0.025f;
                spreadSeq.Insert(delay, coin.DOScale(1f, 0.22f).SetEase(Ease.OutBack));
                spreadSeq.Insert(delay, coin.DOAnchorPos(dest, spreadDuration).SetEase(Ease.OutCubic));
            }
            yield return spreadSeq.WaitForCompletion();
            yield return new WaitForSeconds(firstCoinDelay);

            //-3 Fly yani stagger ile icona uçuş
            Vector2 iconPos = GetIconAnchoredPos();
            bool counterFired = false;

            for (int i = 0; i < coinCount; i++)
            {
                var coin = coins[i];
                Vector2 from = coin.anchoredPosition;
                Vector3[] path = BuildLocalPath(from, BezierControl(from, iconPos), iconPos, coin);

                var flySeq = DOTween.Sequence();
                flySeq.Append(
                    coin.DOLocalPath(path, flyDuration, PathType.CatmullRom, PathMode.TopDown2D)
                    .SetEase(Ease.InCubic)
                );
                flySeq.Insert(flyDuration * 0.45f,
                    coin.DOScale(0.25f, flyDuration * 0.55f).SetEase(Ease.InQuad)
                );

                // CApture için local kopya yapmak
                var capturedCoin = coin;
                flySeq.OnComplete(() =>
                {
                    //destroy yerine poola geri dök
                    _pool.Release(capturedCoin);

                    BounceIcon();

                    if (!counterFired)
                    {
                        counterFired = true;
                        AnimateCounter(_targerGold);
                    }
                });


                flySeq.Play();
                yield return new WaitForSeconds(staggerDelay);
            }
            yield return new WaitForSeconds(flyDuration + counterDuration);
            _isAnimating = false;
        }
        // Helper metotlar
        private Vector2 GetIconAnchoredPos()
        {
            if (goldIconTarget == null) return Vector2.zero;
            var corners = new Vector3[4];
            goldIconTarget.GetWorldCorners(corners);
            Vector3 world = (corners[0] + corners[2]) * 0.5f;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                spawnParent, screen, null, out Vector2 local);
            return local;
        }

        private Vector2 BezierControl(Vector2 from, Vector2 to)
        {
            Vector2 mid = (from + to) * 0.5f;
            return mid + new Vector2(
                Random.Range(-bezierStrength, bezierStrength),
                Random.Range(0f, bezierStrength * 0.8f)
            );
        }
        private Vector3[] BuildLocalPath(Vector2 from, Vector2 ctrl, Vector2 to, RectTransform coin)
        {
            Transform p = coin.parent;
            Vector3 ToLocal(Vector2 ap)
            {
                Vector3 world = spawnParent.TransformPoint(new Vector3(ap.x, ap.y, 0f));
                return p.InverseTransformPoint(world);
            }
            return new[] { ToLocal(from), ToLocal(ctrl), ToLocal(to) };
        }
        private void BounceIcon()
        {
            if (goldIconTarget == null) return;

            goldIconTarget.DOKill();
            goldIconTarget.DOScale(_iconOriginalScale * bounceSale, bounceDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                    goldIconTarget.DOScale(_iconOriginalScale, bounceDuration).SetEase(Ease.InOutSine)
                );
        }
        private void AnimateCounter(int toValue)
        {
            if (goldCountText == null) return;

            _counterTween?.Kill();

            int from = _shownGold;
            _shownGold = toValue;

            float val = from;
            _counterTween = DOTween.To(
                () => val,
                x =>
                {
                    val = x;
                    goldCountText.text = Mathf.RoundToInt(x).ToString();
                },
                toValue,
                counterDuration
            ).SetEase(Ease.OutCubic);

            goldCountText.rectTransform.DOKill();
            goldCountText.rectTransform
                .DOScale(1.25f, 0.14f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                    goldCountText.rectTransform.DOScale(1f, 0.14f).SetEase(Ease.InOutSine));
        }
#if UNITY_EDITOR
        [ContextMenu("Animasyon (play mode)")]
        private void EditorTest() => Play(75, new Vector2(0f, -350f));
#endif
    }
}
    