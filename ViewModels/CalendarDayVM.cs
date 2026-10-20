using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HabitTracker.ViewModels;

public partial class CalendarDayVM : ObservableObject
{
    public DateTime Date { get; }
    public int DayNumber => Date.Day;

    public bool IsEmpty { get; }
    public bool IsCurrentMonth { get; } // Для серых дней
    public bool IsToday => Date.Date == DateTime.Today;

    /// <summary>
    /// Все привычки, запланированные на этот день
    /// </summary>
    public List<HabitDayVM> AllHabits { get; set; } = new();

    /// <summary>
    /// Те 2-3 штуки, которые видны в ячейке
    /// </summary>
    public ObservableCollection<HabitDayVM> VisibleHabits { get; } = new();

    [ObservableProperty]
    private string _hiddenHabitsText = string.Empty; // "+7 еще"

    [ObservableProperty]
    private bool _hasHiddenHabits;

    public CalendarDayVM(DateTime date, bool isCurrentMonth = true, bool isEmpty = false)
    {
        Date = date;
        IsCurrentMonth = isCurrentMonth;
        IsEmpty = isEmpty;
    }

    public static CalendarDayVM CreateEmpty() => new(DateTime.MinValue, false, true);

    /// <summary>
    /// Пересчитывает, какие привычки показать (сортирует и берет первые 3).
    /// Вызывается при загрузке месяца и при клике на чекбокс.
    /// </summary>
    public void RecalculateView()
    {
        // Сортировка: сначала невыполненные, потом выполненные
        var sorted = AllHabits
            .OrderBy(h => h.IsCompleted)
            .ThenBy(h => h.Habit.Name)
            .ToList();

        VisibleHabits.Clear();
        foreach (var habit in sorted.Take(3))
        {
            VisibleHabits.Add(habit);
        }

        int hiddenCount = sorted.Count - 3;
        HasHiddenHabits = hiddenCount > 0;
        HiddenHabitsText = HasHiddenHabits ? $"+{hiddenCount} еще" : string.Empty;
    }
}