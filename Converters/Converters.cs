using System.Globalization;
using HabitTracker.Models;

namespace HabitTracker.Converters;

public class HabitCategoryToRussianConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HabitCategory category)
        {
            return category switch
            {
                HabitCategory.Health => "Здоровье",
                HabitCategory.Fitness => "Спорт",
                HabitCategory.Nutrition => "Питание",
                HabitCategory.Sleep => "Сон",
                HabitCategory.Mindfulness => "Осознанность",
                HabitCategory.Learning => "Обучение",
                HabitCategory.Work => "Работа",
                HabitCategory.Creativity => "Творчество",
                HabitCategory.Finance => "Финансы",
                HabitCategory.Social => "Общение",
                HabitCategory.Chores => "Быт",
                HabitCategory.Other => "Другое",
                _ => category.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}