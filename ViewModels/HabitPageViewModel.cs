using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Messages;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Views;
using System.Collections.ObjectModel;

namespace HabitTracker.ViewModels;

public partial class HabitsViewModel : ObservableObject
{
    private readonly HabitService _habitService;

    // Коллекция для привязки к CollectionView
    public ObservableCollection<Habit> Habits { get; } = new();

    public HabitsViewModel(HabitService habitService)
    {
        _habitService = habitService;

        // 1. Загружаем привычки при старте
        Task.Run(async () => await LoadHabitsAsync());

        // Подписка на сообщение
        WeakReferenceMessenger.Default.Register<HabitAddedMessage>(this, async (r, m) =>
        {
            // Перезагружаем список при получении сообщения
            await LoadHabitsAsync();
        });
    }

    [RelayCommand]
    public async Task LoadHabitsAsync()
    {
        var activeHabits = await _habitService.GetActiveHabitsAsync();

        // Обновляем коллекцию в UI-потоке, чтобы CollectionView не крашился
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Habits.Clear();
            foreach (var habit in activeHabits)
            {
                Habits.Add(habit);
            }
        });
    }

    [RelayCommand]
    private async Task OpenAddHabitPopupAsync()
    {
        // Используем сервис, который уже в ViewModel
        var popup = new AddHabitPopup(_habitService);
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
    }

    // Команда удаления
    [RelayCommand]
    private async Task DeleteHabitAsync(Habit habit)
    {
        if (habit == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert("Удаление", $"Удалить привычку '{habit.Name}'?", "Да", "Нет");
        if (confirm)
        {
            // В нашем HabitService нет метода удаления, добавим его или используем архивацию
            await _habitService.DeleteHabitAsync(habit);
            await LoadHabitsAsync(); // Обновляем список
        }
    }

    // Команда редактирования (открытие попапа)
    [RelayCommand]
    private async Task EditHabitAsync(Habit habit)
    {
        if (habit == null) return;

        // Передаем привычку в попап, чтобы он знал, что это редактирование
        var popup = new AddHabitPopup(_habitService, habit);
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
    }
}