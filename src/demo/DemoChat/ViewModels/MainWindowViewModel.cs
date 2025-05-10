using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoChat.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Name = string.Empty;
        IsEnabled = false;
    }
    
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    [StringLength(15, MinimumLength = 3, ErrorMessage = "Длина имени должна быть от 3 до 15 символов")]
    public string Name
    {
        get => _name;

        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Name))
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();
            
            Validator.TryValidateObject(this, context, results, true);

            IsEnabled = !results.Any();
        }
        
        base.OnPropertyChanged(e);
    }
}