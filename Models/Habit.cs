using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace HabitTracker.Models;

/// <summary>
/// Привычка пользователя.
/// 
/// ВАЖНО: календарь виртуальный — день не хранит список привычек.
/// Принадлежность привычки к конкретному дню вычисляется из <see cref="Frequency"/>
/// и связанных полей (<see cref="IntervalDays"/>, <see cref="ActiveDaysOfWeek"/>, <see cref="TimesPerWeek"/>).
/// </summary>
public class Habit
{
    #region Идентификация
    // ── Идентификация ──────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public HabitCategory Category { get; set; } = HabitCategory.Other;
    #endregion

    #region Периодичность

    // ── Периодичность ──────────────────────────────────────────────

    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;

    /// <summary>Используется только при <see cref="HabitFrequency.Interval"/>.</summary>
    public int IntervalDays { get; set; } = 1;

    /// <summary>Используется только при <see cref="HabitFrequency.SpecificDays"/>.</summary>
    [NotMapped]
    public List<DayOfWeek> ActiveDaysOfWeek { get; set; } = new();
    public string ActiveDaysOfWeekJson
    {
        get => JsonSerializer.Serialize(ActiveDaysOfWeek);
        set => ActiveDaysOfWeek = JsonSerializer.Deserialize<List<DayOfWeek>>(value) ?? new();
    }

    /// <summary>Используется только при <see cref="HabitFrequency.Weekly"/>.</summary>
    public int TimesPerWeek { get; set; } = 1;

    #endregion

    #region Очки и измеримость

    // ── Очки и измеримость ─────────────────────────────────────────

    /// <summary>
    /// Очки за ПОЛНОЕ выполнение привычки за день.
    /// При частичном выполнении начисляется линейно: (done / target) * Points.
    /// Округление — вниз до целого.
    /// </summary>
    public int Points { get; set; } = 10;

    /// <summary>
    /// Является ли привычка измеримой (например, «8 стаканов воды»).
    /// Если false — <see cref="TargetCount"/> всегда 1 и выполнение булево.
    /// </summary>
    public bool IsMeasurable { get; set; } = false;

    /// <summary>
    /// Сколько единиц нужно выполнить за день для полного зачёта.
    /// Для неизмеримых привычек всегда 1.
    /// 
    /// Порог для зачёта дня в стрик: неполное выполнение ВСЕГДА сбрасывает стрик,
    /// даже если очки начислены (см. CalendarDay.GetCompletionRatio и логику сервиса).
    /// </summary>
    public int TargetCount { get; set; } = 1;

    #endregion

    #region Стрики

    // ── Стрики (храним, не вычисляем) ──────────────────────────────

    /// <summary>Текущий стрик по этой привычке (дней подряд с полным выполнением).</summary>
    public int CurrentStreak { get; set; } = 0;

    /// <summary>Рекордный стрик по этой привычке за всё время.</summary>
    public int LongestStreak { get; set; } = 0;

    #endregion

    #region Метаданные

    // ── Метаданные ─────────────────────────────────────────────────

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Архивные привычки не показываются в активном списке и не участвуют в стриках,
    /// но остаются в истории календаря.
    /// </summary>
    public bool IsArchived { get; set; } = false;

    #endregion
}