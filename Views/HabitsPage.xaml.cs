namespace HabitTracker.Views;
using HabitTracker.ViewModels;

public partial class HabitsPage : ContentPage
{
    public HabitsPage()
    {
        InitializeComponent();
        this.BindingContext = new HabitPageVM();
    }
}