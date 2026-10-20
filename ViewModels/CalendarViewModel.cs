using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Views;
using System.Collections.ObjectModel;

namespace HabitTracker.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly CalendarService _calendarService;
    private readonly HabitService _habitService;

    /// <summary>
    /// Кэш привычек и дней на текущий отображаемый месяц (чтобы не дергать БД при каждом клике)
    /// </summary>
    private List<Habit> _cachedHabits = new();
    private List<CalendarDay> _cachedDays = new();

    /// <summary>Текущий отображаемый месяц.</summary>
    [ObservableProperty]
    private DateTime _currentMonth = DateTime.Today;

    /// <summary>Заголовок с названием месяца и годом (например, "Октябрь 2026").</summary>
    [ObservableProperty]
    private string _monthTitle = string.Empty;

    /// <summary>Сетка дней для календаря (включая пустые ячейки для отступов).</summary>
    public ObservableCollection<CalendarDayVM> Days { get; } = new();

    /// <summary>Названия дней недели для шапки календаря (Пн, Вт, ...).</summary>
    public List<string> DayNames { get; } = new() { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

    public CalendarViewModel(CalendarService calendarService, HabitService habitService)
    {
        _calendarService = calendarService;
        _habitService = habitService;
    }
    [RelayCommand]
    private async Task OpenDayDetailsAsync(CalendarDayVM dayVm)
    {
        if (dayVm == null || dayVm.IsEmpty) return;

        // Передаем копию списка, чтобы изменения в попапе не ломали календарь до нажатия "Сохранить"
        var habitsCopy = dayVm.AllHabits.Select(h => new HabitDayVM(h.Habit, h.CompletedCount)).ToList();

        var vm = new DayDetailsViewModel(dayVm.Date, habitsCopy);
        var popup = new DayDetailsPopup(vm);

        // Подписываемся на сохранение из попапа
        vm.OnProgressSaved += async (s, e) =>
        {
            foreach (var habitVm in vm.Habits)
            {
                var original = dayVm.AllHabits.FirstOrDefault(h => h.Habit.Id == habitVm.Habit.Id);
                if (original != null)
                {
                    // Вычисляем, что реально хотим сохранить
                    int newValue = habitVm.Habit.IsMeasurable
                        ? habitVm.CompletedCount
                        : (habitVm.IsCompleted ? 1 : 0);

                    int oldValue = original.Habit.IsMeasurable
                        ? original.CompletedCount
                        : (original.IsCompleted ? 1 : 0);

                    // Если изменилось - сохраняем
                    if (newValue != oldValue)
                    {
                        await SaveHabitProgressAsync(dayVm, habitVm.Habit, newValue);
                    }
                }
            }
        };

        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
    }
    [RelayCommand]
    public async Task LoadMonthAsync()
    {
        // 1. Заголовок
        var culture = new System.Globalization.CultureInfo("ru-RU");
        MonthTitle = CurrentMonth.ToString("MMMM yyyy", culture);
        MonthTitle = char.ToUpper(MonthTitle[0]) + MonthTitle.Substring(1);

        // 2. Вычисляем диапазон дат (с захватом хвостов прошлого и следующего месяца для "серых" дней)
        var startOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Загружаем дни с отступом в неделю назад и вперед
        var dbDays = await _calendarService.GetDaysInRangeAsync(startOfMonth.AddDays(-7), endOfMonth.AddDays(7));
        _cachedDays = dbDays;

        // 3. Загружаем все активные привычки
        _cachedHabits = await _habitService.GetActiveHabitsAsync();

        // 4. Генерируем сетку
        await GenerateGridAsync(startOfMonth);
    }

    private async Task GenerateGridAsync(DateTime startOfMonth)
    {
        Days.Clear();

        // Вычисляем, на какой день недели выпадает 1-е число (чтобы добавить пустые ячейки)
        // В .NET DayOfWeek: Воскресенье = 0, Понедельник = 1. Нам нужно Пн = 0.
        int offset = ((int)startOfMonth.DayOfWeek + 6) % 7;

        // Находим дату, с которой начнем рисовать сетку (может быть в прошлом месяце)
        var gridStartDate = startOfMonth.AddDays(-offset);

        // Генерируем ровно 42 ячейки (6 недель) — стандарт для календарей
        for (int i = 0; i < 42; i++)
        {
            var date = gridStartDate.AddDays(i);
            bool isCurrentMonth = date.Month == startOfMonth.Month;

            var dayVm = new CalendarDayVM(date, isCurrentMonth);

            // Заполняем привычками на этот день
            await PopulateDayHabitsAsync(dayVm);

            Days.Add(dayVm);
        }
    }

    /// <summary>
    /// Заполняет конкретный день списком привычек на основе кэша.
    /// </summary>
    private async Task PopulateDayHabitsAsync(CalendarDayVM dayVm)
    {
        if (dayVm.IsEmpty) return;

        // Получаем привычки, запланированные на этот день
        var scheduledHabits = await _habitService.GetHabitsForDateAsync(dayVm.Date, _cachedHabits, _cachedDays);

        // Ищем, что уже выполнено в БД
        var dbDay = _cachedDays.FirstOrDefault(d => d.Date.Date == dayVm.Date.Date);

        dayVm.AllHabits.Clear();
        foreach (var habit in scheduledHabits)
        {
            int completedCount = 0;
            if (dbDay != null && dbDay.Completions.TryGetValue(habit.Id, out int count))
            {
                completedCount = count;
            }

            var habitDayVm = new HabitDayVM(habit, completedCount);

            // Подписываемся на изменение чекбокса (для клика снаружи)
            habitDayVm.PropertyChanged += async (s, e) => await OnHabitCheckboxChanged(habitDayVm, dayVm);

            dayVm.AllHabits.Add(habitDayVm);
        }

        // Сортируем и выбираем первые 3 для отображения
        dayVm.RecalculateView();
    }

    /// <summary>
    /// Срабатывает, когда пользователь кликает по чекбоксу прямо в сетке календаря.
    /// </summary>
    private async Task OnHabitCheckboxChanged(HabitDayVM habitVm, CalendarDayVM dayVm)
    {
        if (habitVm.Habit.IsMeasurable)
        {
            // Откатываем чекбокс (чтобы не мигал)
            var dbDay = _cachedDays.FirstOrDefault(d => d.Date.Date == dayVm.Date.Date);
            int oldCount = 0;
            if (dbDay != null && dbDay.Completions.TryGetValue(habitVm.Habit.Id, out int count))
                oldCount = count;

            MainThread.BeginInvokeOnMainThread(() => habitVm.CompletedCountText = oldCount.ToString());

            // Открываем карточку дня, чтобы пользователь ввел число
            await OpenDayDetailsAsync(dayVm);
            return;
        }

        int newCount = habitVm.IsCompleted ? 1 : 0;
        await SaveHabitProgressAsync(dayVm, habitVm.Habit, newCount);
    }

    /// <summary>
    /// Сохраняет прогресс в БД и пересчитывает затронутые дни.
    /// </summary>
    private async Task SaveHabitProgressAsync(CalendarDayVM dayVm, Habit habit, int count)
    {
        // 1. Сохраняем в БД
        await _calendarService.MarkHabitAsync(habit, dayVm.Date, count);

        // 2. Обновляем локальный кэш дня, чтобы не делать лишний запрос
        var dbDay = _cachedDays.FirstOrDefault(d => d.Date.Date == dayVm.Date.Date);
        if (dbDay == null)
        {
            dbDay = new CalendarDay { Date = dayVm.Date.Date };
            _cachedDays.Add(dbDay);
        }
        dbDay.Completions[habit.Id] = count;

        // 3. Пересчитываем текущий день
        dayVm.RecalculateView();

        // 4. Пересчитываем ПОСЛЕДУЮЩИЕ дни (для Weekly-привычек)
        foreach (var day in Days.Where(d => d.Date >= dayVm.Date && d.IsCurrentMonth))
        {
            await PopulateDayHabitsAsync(day);
        }
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        await LoadMonthAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        await LoadMonthAsync();
    }
}