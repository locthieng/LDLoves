using UnityEngine;

namespace TileJamUI.Core
{
    /// <summary>
    /// Định danh Page bên trong HomeScreen (spec mục 3): HomePage và ShopPage.
    /// </summary>
    public enum PageId
    {
        Home = 0,
        Mission = 2,
        Annisary = 3,
    }

    /// <summary>
    /// Base class cho Page. Khác với UIView thường (Screen/Popup ẩn bằng SetActive/CanvasGroup),
    /// Page LUÔN active cùng lúc (spec 3.2: "HomePage và ShopPage tồn tại đồng thời"),
    /// chỉ có vị trí trong PageContainer thay đổi qua PageController.
    ///
    /// Vì vậy Page override lại Show/Hide để KHÔNG tắt GameObject, chỉ bật/tắt tương tác.
    /// Việc di chuyển vị trí page hoàn toàn do PageController quản lý.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIPage : UIView
    {
        public abstract PageId PageId { get; }

        public RectTransform RectTransform => (RectTransform)transform;

        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(true); // Page luôn active, không SetActive(false) như Screen/Popup
        }

        /// <summary>Gọi bởi PageController khi page trở thành page hiện tại (ở giữa màn hình).</summary>
        public override void Show(bool animated = true)
        {
            IsVisible = true;
            SetInteractable(true);
            OnShown();
        }

        /// <summary>Gọi bởi PageController khi page bị scroll ra khỏi màn hình.</summary>
        public override void Hide(bool animated = true)
        {
            IsVisible = false;
            SetInteractable(false);
            OnHidden();
        }
    }
}
