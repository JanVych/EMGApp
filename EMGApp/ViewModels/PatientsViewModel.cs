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
    [ObservableProperty]
    private List<MeasurementGroup> measurements;

    public ICommand PatientSelectionChangedCommand
    {
        get;
    }
    public PatientsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Patients = _dataService.Patients;
        Measurements = new List<MeasurementGroup>();
        PatientSelectionChangedCommand = new RelayCommand(PatientSelectionChanged);
        PatientSelectionChanged();
    }
    private void PatientSelectionChanged()
    {
        Measurements.Clear();
        Measurements = _dataService.Measurements.FindAll(item => item.PatientId == Patients[SelectedPatientIndex].PatientId);
    }


}
