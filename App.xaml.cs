using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

namespace HabitTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        // protected override Window CreateWindow(IActivationState? activationState)
        // {
        //     return new Window(new AppShell());
        // }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Задаем желаемые размеры один раз в переменных
            int windowWidth = 600;
            int windowHeight = 800;

#if MACCATALYST
    // 1. Задаем размеры через ограничения (это работает)
    window.MinimumWidth = windowWidth;
    window.MaximumWidth = windowWidth;
    window.MinimumHeight = windowHeight;
    window.MaximumHeight = windowHeight;

    // 2. Ждем, пока окно создастся, и применяем нативную центровку
    window.HandlerChanged += (s, e) =>
    {
        if (window.Handler?.PlatformView is UIKit.UIWindow nativeWindow)
        {
            // Получаем доступ к WindowScene (нативный объект macOS)
            var windowScene = nativeWindow.WindowScene;
            if (windowScene != null)
            {
                // Получаем размеры экрана (в пикселях)
                var screen = windowScene.Screen;
                var screenBounds = screen.Bounds;

                // Вычисляем позицию для центрирования
                double x = (screenBounds.Width - windowWidth) / 2;
                double y = (screenBounds.Height - windowHeight) / 2;

                // Создаем запрос на изменение геометрии окна
                var geometryPreferences = new UIKit.UIWindowSceneGeometryPreferencesMac();
                // Устанавливаем желаемый фрейм (позиция + размер)
                // Обрати внимание: координаты для Mac Catalyst могут требовать пересчета
                geometryPreferences.SystemFrame = new CoreGraphics.CGRect(x, y, windowWidth, windowHeight);

                // Применяем изменения
                windowScene.RequestGeometryUpdate(geometryPreferences, (error) =>
                {
                    // Сюда попадаем, если что-то пошло не так (например, система запретила)
                    System.Diagnostics.Debug.WriteLine($"Geometry update error: {error?.LocalizedDescription}");
                });
            }
        }
    };
#endif

#if WINDOWS
            // === Для Windows ===
            // В Windows можно задать размер и позицию напрямую
            window.Width = windowWidth;
            window.Height = windowHeight;

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            // Центрируем окно
            window.X = (displayInfo.Width / displayInfo.Density - windowWidth) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - windowHeight) / 2;
            
            // Если нужно запретить изменение размера (как на Mac):
            // window.MinimumWidth = windowWidth;
            // window.MaximumWidth = windowWidth;
            // window.MinimumHeight = windowHeight;
            // window.MaximumHeight = windowHeight;
#endif

            return window;
        }
    }
}