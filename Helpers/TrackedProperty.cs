using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.Helpers;

/// <summary>
/// Обертка над свойством, которая хранит флаг "трогали ли поле" (IsDirty).
/// </summary>
public class TrackedProperty<T> : INotifyPropertyChanged
{
    private T _value;
    private bool _isDirty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _isDirty = true;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Показывать ли ошибку (true, если поле изменяли ИЛИ была нажата кнопка "Добавить").</summary>
    public bool IsDirty
    {
        get => _isDirty;
        set { _isDirty = value; OnPropertyChanged(); }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}