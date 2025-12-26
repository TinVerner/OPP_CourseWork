using System.ComponentModel;
using System.Runtime.CompilerServices;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.SupportClasses;

public class SelectableIngredient : INotifyPropertyChanged
{
    public Ingredient Ingredient { get; }

    public long Id => Ingredient.Id;
    public string Name => Ingredient.Name;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public SelectableIngredient(Ingredient ingredient)
    {
        Ingredient = ingredient;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
