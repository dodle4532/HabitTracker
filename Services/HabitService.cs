using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public class HabitService
{
    private readonly AppDBContext _dbContext;

    public HabitService(AppDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Получает все активные (не архивные) привычки.</summary>
    public async Task<List<Habit>> GetActiveHabitsAsync()
    {
        _dbContext.ChangeTracker.Clear();
        return await _dbContext.Habits
            .Where(h => !h.IsArchived)
            .ToListAsync();
    }

    /// <summary>Добавляет новую привычку.</summary>
    public async Task AddHabitAsync(Habit habit)
    {
        _dbContext.Habits.Add(habit);
        await _dbContext.SaveChangesAsync();
    }
    /// <summary>Удаляет привычку.</summary>
    public async Task DeleteHabitAsync(Habit habit)
    {
        _dbContext.Habits.Remove(habit);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>Обновляет привычку.</summary>
    public async Task UpdateHabitAsync(Habit habit)
    {
        _dbContext.Habits.Update(habit);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>Архивирует привычку (не удаляет из БД, чтобы сохранить историю).</summary>
    public async Task ArchiveHabitAsync(Guid habitId)
    {
        var habit = await _dbContext.Habits.FindAsync(habitId);
        if (habit != null)
        {
            habit.IsArchived = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Проверяет, должна ли привычка выполняться в указанный день.
    /// Основано на виртуальном календаре.
    /// </summary>
    public bool IsScheduledFor(Habit habit, DateTime date)
    {
        // Если привычка создана позже проверяемой даты — она не могла быть выполнена
        if (date.Date < habit.CreatedAt.Date) return false;
        if (habit.IsArchived) return false;

        var dayOfWeek = date.DayOfWeek;

        return habit.Frequency switch
        {
            HabitFrequency.Daily => true,
            HabitFrequency.SpecificDays => habit.ActiveDaysOfWeek.Contains(dayOfWeek),
            HabitFrequency.Interval => (date.Date - habit.CreatedAt.Date).Days % habit.IntervalDays == 0,
            HabitFrequency.Weekly => true, // Для Weekly логика сложнее и считается в сервисе стриков
            _ => false
        };
    }
    /// <summary>
    /// Возвращает список привычек, которые нужно выполнить в указанный день.
    /// Учитывает Weekly логику (не показывать, если цель на неделю выполнена).
    /// </summary>
    public async Task<List<Habit>> GetHabitsForDateAsync(DateTime date, List<Habit> allHabits, List<CalendarDay> monthDays)
    {
        var result = new List<Habit>();
        var dayOfWeek = date.DayOfWeek;

        foreach (var habit in allHabits)
        {
            if (date.Date < habit.CreatedAt.Date || habit.IsArchived) continue;

            bool isScheduled = habit.Frequency switch
            {
                HabitFrequency.Daily => true,
                HabitFrequency.SpecificDays => habit.ActiveDaysOfWeek.Contains(dayOfWeek),
                HabitFrequency.Interval => (date.Date - habit.CreatedAt.Date).Days % habit.IntervalDays == 0,

                // Логика Weekly: считаем выполнения за неделю (Пн-Вс)
                HabitFrequency.Weekly => IsWeeklyHabitPending(habit, date, monthDays),

                _ => false
            };

            if (isScheduled) result.Add(habit);
        }

        return result;
    }

    private bool IsWeeklyHabitPending(Habit habit, DateTime date, List<CalendarDay> monthDays)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = date.AddDays(-1 * diff).Date;
        var endOfWeek = startOfWeek.AddDays(6).Date;

        // Считаем, сколько раз выполнили на этой неделе
        int completions = monthDays
            .Where(d => d.Date >= startOfWeek && d.Date <= endOfWeek)
            .Where(d => d.Completions.ContainsKey(habit.Id))
            .Sum(d => d.Completions[habit.Id] > 0 ? 1 : 0);

        // Если выполнили меньше, чем нужно - привычка активна
        return completions < habit.TimesPerWeek;
    }
}