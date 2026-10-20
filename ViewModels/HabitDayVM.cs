using CommunityToolkit.Mvvm.ComponentModel;
using HabitTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.ViewModels;

// ВАЖНО: Наследуемся от ObservableValidator, чтобы работали атрибуты и HasErrors
public partial class HabitDayVM : ObservableValidator
{
    public Habit Habit { get; }

    /// <summary>Строковое представление введенного числа для валидации в UI.</summary>
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(HabitDayVM), nameof(ValidateCompletedCount))]
    private string _completedCountText = "0";

    /// <summary>Реальное количество (int) для бизнес-логики.</summary>
    public int CompletedCount => int.TryParse(CompletedCountText, out var val) ? val : 0;

    public int TargetCount => Habit.TargetCount;

    [ObservableProperty]
    private bool _isCompleted;

    /// <summary>
    /// Возвращает текст первой ошибки для поля ввода.
    /// </summary>
    public string ErrorMessage => GetErrors(nameof(CompletedCountText)).FirstOrDefault()?.ErrorMessage ?? string.Empty;

    public string ProgressText => Habit.IsMeasurable
        ? $"{CompletedCountText} / {TargetCount}"
        : string.Empty;

    public HabitDayVM(Habit habit, int completedCount = 0)
    {
        Habit = habit;
        _completedCountText = completedCount.ToString();

        _isCompleted = habit.IsMeasurable
            ? completedCount >= habit.TargetCount
            : completedCount > 0;

        // Сразу запускаем валидацию, чтобы HasErrors знал о текущем состоянии
        ValidateAllProperties();
    }

    // Логика синхронизации CheckBox и Entry
    partial void OnCompletedCountTextChanged(string value)
    {
        // 1. Автоисправление ведущих нулей (например, "01" -> "1")
        if (!string.IsNullOrEmpty(value) && value.Length > 1 && value.StartsWith("0") && int.TryParse(value, out var parsed))
        {
            // Перезаписываем текст, это вызовет метод еще раз, но уже с чистым числом
            CompletedCountText = parsed.ToString();
            return;
        }

        // 2. Обновляем связанные свойства
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ErrorMessage));

        // 3. Синхронизация с чекбоксом
        if (IsCompleted != (CompletedCount >= TargetCount))
        {
            IsCompleted = CompletedCount >= TargetCount;
        }
    }

    partial void OnIsCompletedChanged(bool value)
    {
        if (Habit.IsMeasurable)
        {
            int newCount = value ? TargetCount : 0;
            if (CompletedCountText != newCount.ToString())
            {
                CompletedCountText = newCount.ToString();
            }
        }
    }

    // Валидация
    public static ValidationResult? ValidateCompletedCount(string? value, ValidationContext context)
    {
        if (context.ObjectInstance is not HabitDayVM vm) return ValidationResult.Success;

        if (string.IsNullOrWhiteSpace(value))
            return new ValidationResult("Введите число");

        if (!int.TryParse(value, out var result))
            return new ValidationResult("Только цифры");

        if (result < 0 || result > vm.TargetCount)
            return new ValidationResult($"От 0 до {vm.TargetCount}");

        return ValidationResult.Success;
    }
}