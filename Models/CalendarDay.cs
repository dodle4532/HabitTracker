using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace HabitTracker.Models;

/// <summary>
/// Один день календаря. Хранит ТОЛЬКО факты выполнения —
/// какие привычки и в каком объёме были отмечены в этот день.
/// 
/// ВАЖНО: список «привычек, запланированных на день» здесь НЕ хранится.
/// Он вычисляется из Habit.Frequency при отрисовке (виртуальный календарь).
/// </summary>
public class CalendarDay
{
    /// <summary>Дата дня. Хранится без времени (00:00:00).</summary>
    [Key]
    public DateTime Date { get; set; }

    /// <summary>
    /// Факты выполнения: HabitId → сколько единиц выполнено.
    /// Для неизмеримых привычек значение 0 или 1.
    /// Для измеримых — от 0 до Habit.TargetCount.
    /// </summary>
    [NotMapped]
    public Dictionary<Guid, int> Completions { get; set; } = new();
    public string CompletionsJson
    {
        get => JsonSerializer.Serialize(Completions);
        set => Completions = JsonSerializer.Deserialize<Dictionary<Guid, int>>(value) ?? new();
    }

    /// <summary>
    /// Доля выполнения конкретной привычки в этот день (0.0 – 1.0).
    /// Если привычка не отмечена — 0. Target берётся из Habit.TargetCount.
    /// </summary>
    public double GetCompletionRatio(Habit habit)
    {
        if (habit.TargetCount <= 0) return 0;

        if (!Completions.TryGetValue(habit.Id, out var done))
            return 0;

        return Math.Min(1.0, (double)done / habit.TargetCount);
    }

    /// <summary>Полностью ли выполнена привычка в этот день.</summary>
    public bool IsFullyCompleted(Habit habit) => GetCompletionRatio(habit) >= 1.0;

    /// <summary>Отмечена ли привычка в этот день хотя бы частично.</summary>
    public bool HasAnyProgress(Habit habit) => Completions.ContainsKey(habit.Id) && Completions[habit.Id] > 0;
}