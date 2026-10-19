using CommunityToolkit.Maui.Views;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.ViewModels;
namespace HabitTracker.Views;

public partial class AddHabitPopup : Popup
{
    AddHabitViewModel _viewModel;
	public AddHabitPopup(HabitService habitService, Habit? habitToEdit = null)
	{
		InitializeComponent();
        _viewModel = new AddHabitViewModel(habitService, habitToEdit);
        BindingContext = _viewModel;

        // Подписываемся на событие из ViewModel
        _viewModel.OnSuccessfullyAdded += (s, e) =>
        {
            // Закрываем окно. true можно использовать как результат "Успешно добавлено"
            Close(true);
        };
    }
}