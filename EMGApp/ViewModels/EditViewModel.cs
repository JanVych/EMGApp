using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Helpers;
using EMGApp.Models;
using Microsoft.UI.Xaml.Controls;

namespace EMGApp.ViewModels;

public partial class EditViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public Patient? EditedPatient
    {
        get; 
    }

    [ObservableProperty]
    private string firstName = string.Empty;
    [ObservableProperty]
    private string lastName = string.Empty;
    [ObservableProperty]
    private string identificationNumber = string.Empty;
    [ObservableProperty]
    private double age = double.NaN;
    [ObservableProperty]
    private int gender = 0;
    [ObservableProperty]
    private double weight = double.NaN;
    [ObservableProperty]
    private double height = double.NaN;
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

    public PatinetPropertyNumberFormater NumberFormater
    {
        get;
    }

    public EditViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
        NumberFormater = new PatinetPropertyNumberFormater();
        if (_dataService.EditedPatient != null)
        {
            EditedPatient = _dataService.EditedPatient;

            FirstName = EditedPatient.FirstName;
            LastName = EditedPatient.LastName;
            IdentificationNumber = EditedPatient.IdentificationNumber;
            Age = EditedPatient.Age;
            Gender = EditedPatient.Gender;
            Weight = EditedPatient.Weight;
            Height = EditedPatient.Height;
            Address = EditedPatient.Address;
            Email = EditedPatient.Email;
            PhoneNumber = EditedPatient.PhoneNumber;
            Description = EditedPatient.Description;
        }
        else
        {
            _navigationService.GoBack();
        }
    }

    [RelayCommand]
    public void ConfirmButton()
    {
        if (EditedPatient != null) 
        {
            if (FirstName == string.Empty || LastName == string.Empty || IdentificationNumber == string.Empty
            || double.IsNaN(Age) || double.IsNaN(Weight) || double.IsNaN(Height))
            {
                PatientInfoBarSeverity = InfoBarSeverity.Warning;
                PatientInfoBarText = "Please fill mandatory fields";
                IsPatientInfoBarOpen = true;
            }
            else
            {
                var p = new Patient(EditedPatient.PatientId, FirstName, LastName, IdentificationNumber, (int)Age, Gender, (int)Weight, (int)Height,
                Address, Email, PhoneNumber, Description);
                _dataService.EditPatient(p);
                _navigationService.GoBack();
            }
        }
    }
    private void ClearAll()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        IdentificationNumber = string.Empty;
        Age = double.NaN;
        Gender = 0;
        Weight = double.NaN;
        Height = double.NaN;
        Address = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Description = string.Empty;
    }
}
