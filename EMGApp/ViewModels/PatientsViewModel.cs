using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using EMGApp.Services;

namespace EMGApp.ViewModels;

public partial class PatientsViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int selectedPatientIndex;

    [ObservableProperty]
    private int selectedMeasurmentIndex;

    [ObservableProperty]
    public List<Patient> patients;

    [ObservableProperty]
    public List<MeasurementGroup> measurements;



    public PatientsViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
        Patients = _dataService.Patients;
        Measurements = new List<MeasurementGroup>();
        PatientSelectionChanged();
        
    }
    [RelayCommand]
    private void PatientSelectionChanged()
    {
        Measurements = _dataService.Measurements.FindAll(item => item.PatientId == Patients[SelectedPatientIndex].PatientId);
    }

    [RelayCommand]
    private void DeletePatientButton()
    {
        if (SelectedPatientIndex >= 0)
        {
            _dataService.RemovePatient(Patients[SelectedPatientIndex]);
            SelectedPatientIndex--;
            Patients = _dataService.Patients;
            PatientSelectionChanged();
        }
    }
    [RelayCommand]
    private void AddPatientButton()
    {
        _navigationService.NavigateTo(typeof(AddViewModel).FullName!);
    }
    [RelayCommand]
    private void MeasurementDetailButton()
    {
        if ((SelectedPatientIndex >= 0 && SelectedMeasurmentIndex < Measurements.Count))
        {
            _dataService.ObservedMeasuremntId = Measurements[SelectedMeasurmentIndex].MeasurementId;
            _navigationService.NavigateTo(typeof(MeasurementsViewModel).FullName!);
        }
    }
    [RelayCommand]
    private void EditPatientButton()
    {
    }

}
