using CommunityToolkit.Maui;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HabitTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkit().ConfigureFonts(fonts =>
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

            // Регистрируем сервис — MAUI сам подставит AppDbContext в конструктор
            builder.Services.AddSingleton<CalendarService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<HabitService>();

            // Инициализируем БД (создаем файл, если его нет)
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                db.Database.EnsureCreated(); // Создает таблицы на основе моделей
            }
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
