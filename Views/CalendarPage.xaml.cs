using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;

    // Внедряем ViewModel через конструктор
    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Вызываем загрузку при появлении страницы
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Загружаем календарь при каждом открытии страницы
        if (_viewModel.LoadMonthCommand.CanExecute(null))
        {
            await _viewModel.LoadMonthCommand.ExecuteAsync(null);
        }
    }

    private void OnCheckboxTapped(object sender, TappedEventArgs e)
    {
        // Пустой метод, чтобы клик по чекбоксу не проваливался в Border и не открывал попап
    }
}