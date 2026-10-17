using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public class CalendarService
{
    private readonly AppDBContext _dbContext;
    private readonly UserService _userService;
    private readonly HabitService _habitService;

    public CalendarService(AppDBContext dbContext, UserService userService, HabitService habitService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _habitService = habitService;
    }

    /// <summary>Получает день из БД (или создает пустой, если его нет).</summary>
    public async Task<CalendarDay> GetOrCreateDayAsync(DateTime date)
    {
        var dayKey = date.Date;
        var day = await _dbContext.CalendarDays.FindAsync(dayKey);

        if (day == null)
        {
            day = new CalendarDay { Date = dayKey };
            _dbContext.CalendarDays.Add(day);
            await _dbContext.SaveChangesAsync();
        }
        return day;
    }



    /// <summary>
    /// Отмечает выполнение привычки на указанную дату.
    /// Возвращает количество начисленных очков.
    /// </summary>
    public async Task<int> MarkHabitAsync(Habit habit, DateTime date, int count)
    {
        // 1. Ограничиваем количество
        count = Math.Clamp(count, 0, habit.TargetCount);

        // 2. Находим или создаем день
        var day = await GetOrCreateDayAsync(date);

        // 3. Записываем факт выполнения
        day.Completions[habit.Id] = count;

        _dbContext.CalendarDays.Update(day);
        await _dbContext.SaveChangesAsync();

        // 4. Считаем очки
        int earnedPoints = 0;
        if (count > 0)
        {
            // Линейное начисление: (выполнено / цель) * очки
            earnedPoints = (int)Math.Floor((double)count / habit.TargetCount * habit.Points);
            await _userService.AddPointsAsync(await _userService.GetOrCreateUserAsync(), earnedPoints);
        }

        // 5. Пересчитываем стрик привычки и общий стрик (вызов метода ниже)
        await RecalculateStreaksAsync(habit, date);

        return earnedPoints;
    }

    /// <summary>
    /// Пересчитывает стрики. Вызывается при каждой отметке.
    /// </summary>
    private async Task RecalculateStreaksAsync(Habit habit, DateTime changedDate)
    {
        // Логика пересчета стриков (сложная, вынесим в отдельный метод позже)
        // Пока просто заглушка, которая обновляет User.CurrentStreak
        var user = await _userService.GetOrCreateUserAsync();

        // ... (здесь будет логика проверки: все ли привычки выполнены за день)
    }
}