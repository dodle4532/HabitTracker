using CommunityToolkit.Maui.Views;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class DayDetailsPopup : Popup
{
    public DayDetailsPopup(DayDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Закрываем окно при сохранении
        vm.OnProgressSaved += (s, e) => Close(true);
    }
}