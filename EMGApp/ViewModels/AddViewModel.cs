using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Models;

namespace EMGApp.ViewModels;

public partial class AddViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    private readonly ILocalSettingsService _localSettingsService;
    [ObservableProperty]
    private int genderIndex = 0;
    [ObservableProperty]
    private int age;
    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private string? surname;
    [ObservableProperty]
    private string? description;
    [ObservableProperty]
    private int weight;
    [ObservableProperty]
    private int height;
    [ObservableProperty]
    private string? condition;
    [ObservableProperty]
    private string? address;

    public ICommand NewButtonCommand
    { get; }
    public AddViewModel(IDataService dataService, ILocalSettingsService localSettingsService)
    {
        _dataService = dataService;
        _localSettingsService = localSettingsService;
        NewButtonCommand = new RelayCommand(NewButtonClick);
    }

    public void NewButtonClick()
    {
        Description ??= string.Empty;
        Condition ??= string.Empty;
        Address ??= string.Empty;
        if(Name == null || Name == string.Empty || Surname == null || Surname == string.Empty) { return; }
        var p = new Patient(null, Name, Surname, Age, GenderIndex, Address, Weight, Height, Condition, Description);
        _dataService.AddPatient(p);
    }
}
