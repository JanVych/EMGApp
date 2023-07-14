using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using EMGApp.Services;
using FftSharp;
using Microsoft.UI.Xaml;

namespace EMGApp.ViewModels;

public partial class PatientsViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    private readonly IMeasurementService _measurementService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int selectedPatientIndex;

    [ObservableProperty]
    private int selectedMeasurmentIndex;

    [ObservableProperty]
    public List<Patient> patients;

    [ObservableProperty]
    public List<MeasurementGroup> measurements;

    //Filter Properties
    [ObservableProperty]
    private string selectedFilterItem = "Name";

    [ObservableProperty]
    private string[]? filterComboBoxItems;

    [ObservableProperty]
    private string? filterComboBoxItem;

    [ObservableProperty]
    private Visibility filterComboBoxVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility filterTextBoxkVisibility = Visibility.Visible;



    public PatientsViewModel(IDataService dataService, INavigationService navigationService, IMeasurementService measurementService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
        _measurementService = measurementService;
        Patients = _dataService.Patients;
        Measurements = new List<MeasurementGroup>();
        PatientSelectionChanged();
        
    }
    [RelayCommand]
    private void PatientSelectionChanged()
    {
        if (SelectedPatientIndex >= 0)
        {
            Measurements = _dataService.Measurements.FindAll(item => item.PatientId == Patients[SelectedPatientIndex].PatientId);
        }
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
    private void DeleteMeasurementButton()
    {
        if (SelectedMeasurmentIndex >= 0)
        {
            _dataService.RemoveMeasurement(Measurements[SelectedMeasurmentIndex]);
            SelectedMeasurmentIndex--;
            Measurements = _dataService.Measurements;
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
        if ((SelectedPatientIndex >= 0 && SelectedMeasurmentIndex >= 0))
        {
            _dataService.ObservedMeasurementId = Measurements[SelectedMeasurmentIndex].MeasurementId;
            _dataService.ObservedPatientId = Patients[SelectedPatientIndex].PatientId;
            _navigationService.NavigateTo(typeof(MeasurementsViewModel).FullName!);
        }
    }
    [RelayCommand]
    private void EditPatientButton()
    {
    }

    [RelayCommand]
    private void MeasurementPatientButton()
    {
        if (Patients.Count <= SelectedPatientIndex || SelectedPatientIndex < 0) { return; }
        _measurementService.StopRecording();
        // ?? Devuice number
        _measurementService.CreateConnection(_measurementService.CreateDefaultMeasurement());
        _dataService.CurrentPatientId = Patients[SelectedPatientIndex].PatientId;

        _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
    }

    [RelayCommand]
    private void SelectedFilterChanged()
    {

        if (SelectedFilterItem == "Gender")
        {
            FilterComboBoxItems = Patient.GenderStrings.Select(x => x.Value).ToArray();
            SelectedFilterChangedCB();
        }
        else
        {
            FilterTextBoxkVisibility = Visibility.Visible;
            FilterComboBoxVisibility = Visibility.Collapsed;
            PatientsFilterChanged(string.Empty);
        }
    }
    private void SelectedFilterChangedCB()
    {
        FilterTextBoxkVisibility = Visibility.Collapsed;
        FilterComboBoxVisibility = Visibility.Visible;
        FilterComboBoxItem = FilterComboBoxItems?.FirstOrDefault();
        PatientsFilterChanged(FilterComboBoxItem ?? string.Empty);
    }

    [RelayCommand]
    private void PatientsFilterChanged(object parameter)
    {
        var text = parameter as string;
        var suitableItems = new List<Patient>();
        var splitText = text?.ToLower().Split(" ");

        foreach (var patient in _dataService.Patients)
        {
            var found = splitText?.All((key) =>
            {
                return Patient.GetStringProperty(patient, SelectedFilterItem).ToLower().Contains(key);
            });
            found ??= false;
            if ((bool)found)
            {
                suitableItems.Add(patient);
            }
        }
        Patients = suitableItems;
        if (suitableItems.Count > 0) { SelectedPatientIndex = 0; }
    }
}
