using System;
using System.Collections.Generic;

namespace TileJamUI.Core
{
    /// <summary>
    /// Service Locator tĩnh, thay thế cho DI framework (Zenject/VContainer).
    /// Đăng ký 1 instance cho mỗi Type (interface hoặc class) và lấy lại ở bất kỳ đâu.
    ///
    /// Cách dùng:
    ///   ServiceLocator.Register<ICoinService>(new CoinService());
    ///   var coin = ServiceLocator.Get<ICoinService>();
    ///
    /// LƯU Ý: Clear() phải được gọi khi SceneFlowController load lại Gameplay Scene
    /// từ đầu (ví dụ restart app) để tránh giữ tham chiếu cũ (stale reference).
    /// Trong flow Home <-> Gameplay bình thường KHÔNG cần Clear() vì không reload scene.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException(
                $"[ServiceLocator] Chưa đăng ký service cho type: {type.Name}. " +
                $"Kiểm tra lại SceneFlowController.InitializeServices() hoặc thứ tự Awake().");
        }

        public static bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }

            service = default;
            return false;
        }

        public static bool IsRegistered<T>() => _services.ContainsKey(typeof(T));

        /// <summary>Chỉ gọi khi thật sự reset toàn bộ app state (ví dụ về lại Splash).</summary>
        public static void Clear() => _services.Clear();
    }
}
