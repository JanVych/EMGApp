using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using Microsoft.UI.Xaml.Controls;

namespace EMGApp.ViewModels;

public partial class AddViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private string firstName = string.Empty;
    [ObservableProperty]
    private string lastName = string.Empty;
    [ObservableProperty]
    private string identificationNumber = string.Empty;
    [ObservableProperty]
    private int age = 0;
    [ObservableProperty]
    private int gender;
    [ObservableProperty]
    private int weight = 0;
    [ObservableProperty]
    private int height = 0;
    [ObservableProperty]
    private string address = string.Empty;
    [ObservableProperty]
    private string email = string.Empty;
    [ObservableProperty]
    private string phoneNumber = string.Empty;
    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool isPatientInfoBarOpen = false;
    [ObservableProperty]
    private string patientInfoBarText = string.Empty;
    [ObservableProperty]
    private InfoBarSeverity patientInfoBarSeverity = InfoBarSeverity.Success;

    public AddViewModel(IDataService dataService, ILocalSettingsService localSettingsService)
    {
        _dataService = dataService;
        _localSettingsService = localSettingsService;
    }

    [RelayCommand]
    public void NewButton()
    {

        if (FirstName == string.Empty || LastName == string.Empty || IdentificationNumber == string.Empty
            || Age == 0 || Weight == 0 || Height == 0)
        {
            PatientInfoBarSeverity = InfoBarSeverity.Warning;
            PatientInfoBarText = "Please fill required fields";
            IsPatientInfoBarOpen = true;
            return; 
        }
        var p = new Patient(null, FirstName, LastName, IdentificationNumber, (int)Age, Gender, (int)Weight, (int)Height, 
            Address, Email, PhoneNumber, Description);
        _dataService.AddPatient(p);
        ClearAll();
        PatientInfoBarSeverity = InfoBarSeverity.Success;
        PatientInfoBarText = "Patient added successfully";
        IsPatientInfoBarOpen = true;
    }
    private void ClearAll()
    {
        FirstName = string.Empty; 
        LastName = string.Empty;
        IdentificationNumber = string.Empty;
        Age = 0;
        Gender = 0;
        Weight = 0;
        Height = 0;
        Address = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Description = string.Empty; 
    }
}
