using CommunityToolkit.Maui;
using HabitTracker.Services;
using HabitTracker.ViewModels;
using HabitTracker.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers; // Важно для EntryHandler

namespace HabitTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                   .UseMauiCommunityToolkit()
                   .ConfigureFonts(fonts =>
                   {
                       fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                       fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                   });

            // Регистрируем DbContext
            builder.Services.AddDbContext<AppDBContext>(options =>
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "habittracker.db3");
                options.UseSqlite($"Data Source={dbPath}");
            });
            builder.Services.AddTransient<HabitsViewModel>();
            builder.Services.AddTransient<HabitsPage>();
            builder.Services.AddTransient<CalendarViewModel>();
            builder.Services.AddTransient<DayDetailsViewModel>();
            // Регистрируем сервисы
            builder.Services.AddSingleton<CalendarService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<HabitService>();

            // Инициализируем БД
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                db.Database.EnsureCreated();
            }

            // --- ВАЖНО: Настройка Entry внутри метода ---

            // Отключаем рамки у Entry на Windows
#if WINDOWS
            EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                // Убираем толщину рамки
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                
                // Убираем нижнюю линию при фокусе (синюю полосу)
                handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
            });
#endif

            // Отключаем подчеркивание на Android
#if ANDROID
            EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                // Убираем стандартный фон (подчеркивание)
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}