using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HabitTracker.ViewModels;

public partial class HabitPageVM : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    private int count;
    public string Text => "Кол-во нажатий - " + Count;
    [RelayCommand]
    private void Add()
    {
        Count++;
    }
}
