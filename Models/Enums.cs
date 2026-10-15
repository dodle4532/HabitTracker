namespace HabitTracker.Models;

/// <summary>
/// Категория привычки. Используется для группировки и фильтрации в UI.
/// </summary>
public enum HabitCategory
{
    Health,       // Здоровье
    Fitness,      // Спорт
    Nutrition,    // Питание
    Sleep,        // Сон
    Mindfulness,  // Медитация / осознанность
    Learning,     // Учёба / саморазвитие
    Work,         // Работа / продуктивность
    Creativity,   // Творчество
    Finance,      // Финансы
    Social,       // Общение / отношения
    Chores,       // Быт / домашние дела
    Other         // Другое
}

/// <summary>
/// Периодичность появления привычки в календаре.
/// </summary>
public enum HabitFrequency
{
    /// <summary>Каждый день.</summary>
    Daily,

    /// <summary>Раз в N дней. Число задаётся в <see cref="Habit.IntervalDays"/>.</summary>
    Interval,

    /// <summary>Конкретные дни недели. Список задаётся в <see cref="Habit.ActiveDaysOfWeek"/>.</summary>
    SpecificDays,

    /// <summary>N раз в неделю без привязки к конкретным дням. Число задаётся в <see cref="Habit.TimesPerWeek"/>.</summary>
    Weekly
}