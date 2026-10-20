using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using System.Collections.ObjectModel;

namespace HabitTracker.ViewModels;

public partial class DayDetailsViewModel : ObservableObject
{
    public DateTime Date { get; }
    public string Title => Date.ToString("dd MMMM yyyy, dddd");

    public ObservableCollection<HabitDayVM> Habits { get; } = new();

    // Событие для закрытия попапа и сообщения календарю, что что-то изменилось
    public event EventHandler? OnProgressSaved;

    public DayDetailsViewModel(DateTime date, List<HabitDayVM> habits)
    {
        Date = date;
        var sorted = habits.OrderBy(h => h.IsCompleted).ToList();
        foreach (var habit in sorted)
        {
            Habits.Add(habit);
            // Подписываемся на ошибки, чтобы обновлять состояние кнопки
            habit.ErrorsChanged += (s, e) => SaveCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        OnProgressSaved?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSave() => !Habits.Any(h => h.HasErrors);
}