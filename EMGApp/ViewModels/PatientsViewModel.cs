using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Models;

namespace EMGApp.ViewModels;

public partial class PatientsViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private int selectedPatientIndex;

    [ObservableProperty]
    private int selectedMeasurmentIndex;

    public List<Patient> Patients  
    { 
        get; set; 
    }
    public List<MeasurementGroup> Measurements;


    public PatientsViewModel(IDataService dataService)
    {
        _dataService = dataService;
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
        _dataService.RemovePatient(Patients[SelectedPatientIndex]);
        Patients = _dataService.Patients;
        PatientSelectionChanged();
    }

}
