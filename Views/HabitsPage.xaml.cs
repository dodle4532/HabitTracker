using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class HabitsPage : ContentPage
{
    public HabitsPage(HabitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Устанавливаем ViewModel как контекст для привязок
    }
}