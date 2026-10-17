using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

public class AppDBContext : DbContext
{
    public DbSet<Habit> Habits { get; set; }
    public DbSet<CalendarDay> CalendarDays { get; set; }
    public DbSet<User> Users { get; set; }

    // 1. Добавляем конструктор, который принимает настройки
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

    // 2. Оставляем пустой конструктор для MAUI (если где-то создается через new)
    public AppDBContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Если настройки уже переданы (например, из тестов) - ничего не делаем
        if (optionsBuilder.IsConfigured) return;

        // Иначе - стандартный путь для MAUI
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "habittracker.db3");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}