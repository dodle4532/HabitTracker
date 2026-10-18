using CommunityToolkit.Maui.Views;
using HabitTracker.Services;
using HabitTracker.ViewModels;
namespace HabitTracker.Views;

public partial class AddHabitPopup : Popup
{
    AddHabitViewModel _viewModel;
	public AddHabitPopup(HabitService habitService)
	{
		InitializeComponent();
        _viewModel = new AddHabitViewModel(habitService);
        BindingContext = _viewModel;

        // Подписываемся на событие из ViewModel
        _viewModel.OnSuccessfullyAdded += (s, e) =>
        {
            // Закрываем окно. true можно использовать как результат "Успешно добавлено"
            Close(true);
        };
    }
}