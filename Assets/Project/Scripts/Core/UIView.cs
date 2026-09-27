using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TileJamUI.Core
{
    /// <summary>
    /// Base class cho MỌI thành phần UI hiển thị được: Screen, Page, Popup.
    /// Mục đích: chuẩn hoá cách Show/Hide, tránh mỗi màn hình tự viết logic ẩn/hiện khác nhau.
    /// Dùng DOTween cho fade thay vì coroutine tay.
    ///
    /// Nguyên tắc quan trọng theo spec mục 10 & 13:
    /// "Business logic không đặt trong View/Popup" -> class con của UIView
    /// CHỈ được chứa code liên quan tới hiển thị (animation, wiring button -> callback),
    /// KHÔNG chứa rule về coin/booster/level. Rule đó nằm ở các *Controller/*System riêng.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] protected float fadeDuration = 0.15f;
        [SerializeField] protected CanvasGroup bg;

        protected CanvasGroup CanvasGroup { get; private set; }
        public bool IsVisible { get; protected set; }

        private Tween _fadeTween;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void OnDestroy()
        {
            _fadeTween?.Kill();
        }

        /// <summary>Gọi khi view được tạo lần đầu (Awake), dùng để cache reference, KHÔNG chứa business logic.</summary>
        public virtual void Show(bool animated = true)
        {
            gameObject.SetActive(true);
            IsVisible = true;

            _fadeTween?.Kill();

            if(bg != null)
            {
                bg.blocksRaycasts = true;
            }

            if (animated && fadeDuration > 0f)
            {
                CanvasGroup.alpha = 0f;
                _fadeTween = CanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    SetInteractable(true);
                });

                if (bg != null)
                {
                    bg.DOFade(1f, fadeDuration).SetUpdate(true);
                }
            }
            else
            {
                CanvasGroup.alpha = 1f;
                if (bg != null)
                {
                    bg.alpha = 1f;
                }
                SetInteractable(true);
            }
            OnShown();
        }

        public virtual void Hide(bool animated = true)
        {
            _fadeTween?.Kill();

            SetInteractable(false);

            if (animated && fadeDuration > 0f && isActiveAndEnabled)
            {
                _fadeTween = CanvasGroup.DOFade(0f, fadeDuration)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        IsVisible = false;
                        OnHidden();
                    });

                if (bg != null)
                {
                    bg.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                    {
                        bg.blocksRaycasts = false;
                    });
                }
            }
            else
            {
                CanvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                SetInteractable(false);
                if (bg != null)
                {
                    bg.blocksRaycasts = false;
                }
                OnHidden();
            }
        }

        /// <summary>Bật/tắt tương tác mà không tắt hẳn GameObject (dùng khi có popup con đè lên).</summary>
        public void SetInteractable(bool interactable)
        {
            CanvasGroup.interactable = interactable;
            CanvasGroup.blocksRaycasts = interactable;
        }

        /// <summary>Hook cho class con: xử lý ngay khi Show() được gọi (không đợi fade xong, giống hành vi cũ).</summary>
        protected virtual void OnShown() { }

        /// <summary>Hook cho class con: xử lý sau khi fade-out xong (hoặc ngay lập tức nếu animated=false).</summary>
        protected virtual void OnHidden() { }
    }
}