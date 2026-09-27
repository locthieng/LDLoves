using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TileJamUI.Core
{
    /// <summary>
    /// Quản lý việc chuyển Page bằng horizontal scroll (spec mục 3.2, 10.2), dùng DOTween thay coroutine tay.
    ///
    /// Quy tắc bắt buộc theo spec:
    /// - KHÔNG reload Scene, KHÔNG instantiate lại Page khi chuyển.
    /// - HomePage và ShopPage tồn tại đồng thời, ShopPage nằm off-screen khi Home active.
    /// - PageController CHỈ quản lý index/vị trí/animation, KHÔNG chứa business logic Home/Shop.
    ///
    /// Cách bố trí trong Editor:
    ///   PageContainer (RectTransform, gắn script này)
    ///     ├── HomePage   (index 0, x = 0)
    ///     └── ShopPage   (index 1, x = width)
    /// Container cha (viewport) có kích thước = 1 màn hình, dùng RectMask2D để crop phần page ở ngoài.
    /// </summary>
    public class PageController : MonoBehaviour
    {
        [SerializeField] private List<UIPage> pages = new List<UIPage>();

        [SerializeField] private RectTransform viewport;

        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private Ease ease = Ease.InOutCubic;

        private RectTransform _container;
        private int _currentIndex = 1;
        private Tween _scrollTween;

        public PageId CurrentPage => (pages != null && _currentIndex >= 0 && _currentIndex < pages.Count && pages[_currentIndex] != null) ? pages[_currentIndex].PageId : PageId.Home;
        public event Action<PageId> OnPageChanged;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private IEnumerator Start()
        {
            yield return null;
            _container = (RectTransform)transform;

            if (viewport == null)
            {
                if (transform.parent is RectTransform parentRect)
                    viewport = parentRect;
                else
                    viewport = _container;
            }

            pages.RemoveAll(p => p == null);

            if (pages.Count == 0)
            {
                pages.AddRange(GetComponentsInChildren<UIPage>(true));
            }

            if (pages.Count > 0)
            {
                LayoutPages();
                // Mặc định HomePage ở giữa màn hình (spec 3.1), các page còn lại off-screen.
                int defaultIndex = Mathf.Clamp(1, 0, pages.Count - 1);
                SetActivePageImmediate(defaultIndex);
            }
        }

        private void OnDestroy()
        {
            _scrollTween?.Kill();
        }

        /// <summary>Đặt từng page cạnh nhau theo chiều ngang, dùng chiều rộng viewport làm 1 đơn vị.</summary>
        private void LayoutPages()
        {
            if (viewport == null) return;
            float pageWidth = viewport.rect.width;
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] == null) continue;
                var rect = pages[i].RectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = viewport.rect.size;
                rect.anchoredPosition = new Vector2(i * pageWidth, 0f);
            }
        }

        public void GoToPage(PageId id, bool animated = true, Action OnScrollDone = null)
        {
            int index = pages.FindIndex(p => p != null && p.PageId == id);
            if (index < 0)
            {
                Debug.LogError($"[PageController] Không tìm thấy Page: {id}");
                return;
            }
            GoToPage(index, animated, OnScrollDone);
        }

        public void GoToPage(int index, bool animated = true, Action OnScrollDone = null)
        {
            if (pages == null || index < 0 || index >= pages.Count || index == _currentIndex) return;

            if (_currentIndex >= 0 && _currentIndex < pages.Count && pages[_currentIndex] != null)
            {
                pages[_currentIndex].Hide();
            }

            // DOKill trước khi tween mới, tránh 2 tween cùng chỉnh anchoredPosition.x đè lên nhau
            // nếu player bấm chuyển page liên tục trong lúc animation cũ chưa xong.
            _scrollTween?.Kill();

            if (animated)
            {
                float pageWidth = viewport != null ? viewport.rect.width : 0f;
                float targetX = -index * pageWidth;

                _scrollTween = _container
                    .DOAnchorPosX(targetX, transitionDuration)
                    .SetEase(ease)
                    .SetUpdate(true) // không phụ thuộc Time.timeScale (vẫn scroll được khi Pause)
                    .OnComplete(() =>
                    {
                        OnScrollComplete(index);
                        OnScrollDone?.Invoke();
                    });
            }
            else
            {
                SetActivePageImmediate(index);
                OnScrollDone?.Invoke();
            }
        }

        private void OnScrollComplete(int targetIndex)
        {
            _currentIndex = targetIndex;
            if (_currentIndex >= 0 && _currentIndex < pages.Count && pages[_currentIndex] != null)
            {
                pages[_currentIndex].Show();
                OnPageChanged?.Invoke(pages[_currentIndex].PageId);
            }
        }

        private void SetActivePageImmediate(int index)
        {
            if (pages == null || pages.Count == 0) return;
            index = Mathf.Clamp(index, 0, pages.Count - 1);

            float pageWidth = viewport != null ? viewport.rect.width : 0f;
            _container.anchoredPosition = new Vector2(-index * pageWidth, _container.anchoredPosition.y);
            _currentIndex = index;

            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] == null) continue;
                if (i == index) pages[i].Show();
                else pages[i].Hide();
            }

            if (pages[_currentIndex] != null)
            {
                OnPageChanged?.Invoke(pages[_currentIndex].PageId);
            }
        }
    }
}