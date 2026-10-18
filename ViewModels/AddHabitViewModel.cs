using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Messages;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HabitTracker.ViewModels;

public partial class AddHabitViewModel : ObservableValidator
{
    private readonly HabitService _habitService;

    public event EventHandler? OnSuccessfullyAdded;

    // --- Строковые поля для UI (валидация проверяет их) ---

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(NameError))]
    [Required(ErrorMessage = "Название обязательно")]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(PointsError))]
    [NotifyPropertyChangedFor(nameof(IsPointsInvalid))]
    [CustomValidation(typeof(AddHabitViewModel), nameof(ValidatePoints))]
    private string _pointsText = "10";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TargetCountError))]
    [NotifyPropertyChangedFor(nameof(IsTargetCountInvalid))]
    [CustomValidation(typeof(AddHabitViewModel), nameof(ValidateTargetCount))]
    private string _targetCountText = "1";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(IntervalError))]
    [NotifyPropertyChangedFor(nameof(IsIntervalInvalid))]
    [CustomValidation(typeof(AddHabitViewModel), nameof(ValidateIntervalDays))]
    private string _intervalDaysText = "2";

    // --- Остальные поля ---

    [ObservableProperty]
    private bool _isMeasurable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFrequencyIndex))]
    [NotifyPropertyChangedFor(nameof(IsIntervalVisible))]
    [NotifyPropertyChangedFor(nameof(IsDaysOfWeekVisible))]
    private HabitFrequency _selectedFrequency = HabitFrequency.Daily;

    [ObservableProperty]
    private bool _showErrors;

    // --- Вычисляемые int-свойства для бизнес-логики ---

    public int Points => int.TryParse(PointsText, out var val) ? val : 0;
    public int TargetCount => int.TryParse(TargetCountText, out var val) ? val : 1;
    public int IntervalDays => int.TryParse(IntervalDaysText, out var val) ? val : 0;

    // --- Видимость полей ---

    public bool IsIntervalVisible => SelectedFrequency == HabitFrequency.Interval;
    public bool IsDaysOfWeekVisible => SelectedFrequency == HabitFrequency.SpecificDays;

    // --- Коллекция дней недели ---

    public ObservableCollection<DayOfWeekItem> DaysOfWeek { get; }

    // --- Состояния ошибок для UI ---

    public bool IsNameInvalid => ShowErrors && GetErrors(nameof(Name)).Any();
    public string NameError => GetErrors(nameof(Name)).FirstOrDefault()?.ErrorMessage ?? "";

    public bool IsPointsInvalid => ShowErrors && GetErrors(nameof(PointsText)).Any();
    public string PointsError => GetErrors(nameof(PointsText)).FirstOrDefault()?.ErrorMessage ?? "";

    public bool IsTargetCountInvalid => ShowErrors && GetErrors(nameof(TargetCountText)).Any();
    public string TargetCountError => GetErrors(nameof(TargetCountText)).FirstOrDefault()?.ErrorMessage ?? "";

    public bool IsIntervalInvalid => ShowErrors && GetErrors(nameof(IntervalDaysText)).Any();
    public string IntervalError => GetErrors(nameof(IntervalDaysText)).FirstOrDefault()?.ErrorMessage ?? "";

    // --- Список частот и Picker ---

    public List<string> FrequencyNames { get; } = new()
    {
        "Каждый день",
        "С определенным интервалом",
        "По конкретным дням недели",
        "Несколько раз в неделю"
    };

    public int SelectedFrequencyIndex
    {
        get => (int)SelectedFrequency;
        set
        {
            if (value >= 0 && value != (int)SelectedFrequency)
            {
                SelectedFrequency = (HabitFrequency)value;
                OnPropertyChanged();
            }
        }
    }

    // --- Статические методы валидации ---

    public static ValidationResult? ValidatePoints(string? value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value)) return new ValidationResult("Очки обязательны");
        if (!int.TryParse(value, out var result)) return new ValidationResult("Введите целое число");
        if (result < 1 || result > 10) return new ValidationResult("Очки от 1 до 10");
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateTargetCount(string? value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value)) return new ValidationResult("Цель обязательна");
        if (!int.TryParse(value, out var result)) return new ValidationResult("Введите целое число");
        if (result < 1) return new ValidationResult("Цель должна быть больше 0");
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateIntervalDays(string? value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value)) return new ValidationResult("Интервал обязателен");
        if (!int.TryParse(value, out var result)) return new ValidationResult("Введите целое число");
        if (result < 2 || result > 365) return new ValidationResult("Интервал от 2 до 365 дней");
        return ValidationResult.Success;
    }


    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TimesPerWeekError))]
    [NotifyPropertyChangedFor(nameof(IsTimesPerWeekInvalid))]
    [CustomValidation(typeof(AddHabitViewModel), nameof(ValidateTimesPerWeek))]
    private string _timesPerWeekText = "3";

    public bool IsTimesPerWeekVisible => SelectedFrequency == HabitFrequency.Weekly;

    partial void OnSelectedFrequencyChanged(HabitFrequency value)
    {
        OnPropertyChanged(nameof(IsIntervalVisible));
        OnPropertyChanged(nameof(IsDaysOfWeekVisible));
        OnPropertyChanged(nameof(IsTimesPerWeekVisible)); // <--- Добавить
    }
    public int TimesPerWeek => int.TryParse(TimesPerWeekText, out var val) ? val : 0;

    public bool IsTimesPerWeekInvalid => ShowErrors && GetErrors(nameof(TimesPerWeekText)).Any();
    public string TimesPerWeekError => GetErrors(nameof(TimesPerWeekText)).FirstOrDefault()?.ErrorMessage ?? "";

    public static ValidationResult? ValidateTimesPerWeek(string? value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value)) return new ValidationResult("Количество обязательно");
        if (!int.TryParse(value, out var result)) return new ValidationResult("Введите целое число");
        if (result < 1 || result > 7) return new ValidationResult("От 1 до 7 раз в неделю");
        return ValidationResult.Success;
    }

    partial void OnTimesPerWeekTextChanged(string value)
    {
        OnPropertyChanged(nameof(TimesPerWeek));
        if (ShowErrors) OnPropertyChanged(nameof(IsTimesPerWeekInvalid));
    }
    // --- Команда добавления ---

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ShowErrors = true;
        ValidateAllProperties();

        // Обновляем видимость ошибок для всех полей
        OnPropertyChanged(nameof(IsNameInvalid));
        OnPropertyChanged(nameof(IsPointsInvalid));
        OnPropertyChanged(nameof(IsTargetCountInvalid));
        OnPropertyChanged(nameof(IsIntervalInvalid));

        if (HasErrors)
        {
            SubmitCommand.NotifyCanExecuteChanged();
            return;
        }

        var newHabit = new Habit
        {
            Name = Name.Trim(),
            Points = Points,
            IsMeasurable = IsMeasurable,
            TargetCount = IsMeasurable ? TargetCount : 1,
            TimesPerWeek = TimesPerWeek,
            Frequency = SelectedFrequency,
            IntervalDays = IntervalDays,
            ActiveDaysOfWeek = IsDaysOfWeekVisible
                ? DaysOfWeek.Where(d => d.IsSelected).Select(d => d.Day).ToList()
                : new List<DayOfWeek>()
        };

        await _habitService.AddHabitAsync(newHabit);
        WeakReferenceMessenger.Default.Send(new HabitAddedMessage());
        OnSuccessfullyAdded?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnSuccessfullyAdded?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSubmit()
    {
        if (!ShowErrors) return true;
        return !HasErrors;
    }

    // --- Обработчики изменений ---

    partial void OnNameChanged(string value) { if (ShowErrors) OnPropertyChanged(nameof(IsNameInvalid)); }

    // При изменении строк обновляем вычисляемые int-свойства
    partial void OnPointsTextChanged(string value)
    {
        // Если ввели "01", "02" и т.д., заменяем на нормальное число
        if (!string.IsNullOrEmpty(value) && value.Length > 1 && value.StartsWith("0") && int.TryParse(value, out var parsed))
        {
            PointsText = parsed.ToString(); // Это вызовет метод еще раз, но уже с чистым текстом
            return;
        }
        OnPropertyChanged(nameof(Points));
        if (ShowErrors) OnPropertyChanged(nameof(IsPointsInvalid));
    }

    partial void OnTargetCountTextChanged(string value)
    {
        // Если ввели "01", "02" и т.д., заменяем на нормальное число
        if (!string.IsNullOrEmpty(value) && value.Length > 1 && value.StartsWith("0") && int.TryParse(value, out var parsed))
        {
            TargetCountText = parsed.ToString(); // Это вызовет метод еще раз, но уже с чистым текстом
            return;
        }
        OnPropertyChanged(nameof(TargetCount));
        if (ShowErrors) OnPropertyChanged(nameof(IsTargetCountInvalid));
    }

    partial void OnIntervalDaysTextChanged(string value)
    {
        // Если ввели "01", "02" и т.д., заменяем на нормальное число
        if (!string.IsNullOrEmpty(value) && value.Length > 1 && value.StartsWith("0") && int.TryParse(value, out var parsed))
        {
            IntervalDaysText = parsed.ToString(); // Это вызовет метод еще раз, но уже с чистым текстом
            return;
        }
        OnPropertyChanged(nameof(IntervalDays));
        if (ShowErrors) OnPropertyChanged(nameof(IsIntervalInvalid));
    }

    partial void OnShowErrorsChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNameInvalid));
        OnPropertyChanged(nameof(IsPointsInvalid));
        OnPropertyChanged(nameof(IsTargetCountInvalid));
        OnPropertyChanged(nameof(IsIntervalInvalid));
        SubmitCommand.NotifyCanExecuteChanged();
    }

    // --- Конструктор ---

    public AddHabitViewModel(HabitService habitService)
    {
        _habitService = habitService;

        var orderedDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        DaysOfWeek = new ObservableCollection<DayOfWeekItem>(
            orderedDays.Select(d => new DayOfWeekItem { Day = d, IsSelected = false })
        );

        ErrorsChanged += (s, e) => SubmitCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>
/// Вспомогательный класс для отображения дней недели в UI.
/// </summary>
public partial class DayOfWeekItem : ObservableObject
{
    public DayOfWeek Day { get; set; }

    public string DayName => Day switch
    {
        DayOfWeek.Monday => "Пн",
        DayOfWeek.Tuesday => "Вт",
        DayOfWeek.Wednesday => "Ср",
        DayOfWeek.Thursday => "Чт",
        DayOfWeek.Friday => "Пт",
        DayOfWeek.Saturday => "Сб",
        DayOfWeek.Sunday => "Вс",
        _ => ""
    };

    [ObservableProperty]
    private bool _isSelected;
}