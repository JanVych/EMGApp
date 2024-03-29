﻿using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Contracts.ViewModels;
using EMGApp.Models;
using Microsoft.UI.Xaml;

namespace EMGApp.ViewModels;

public partial class SetupViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMeasurementService _measurementService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int windowShiftMilliseconds = 100;

    [ObservableProperty]
    private int sampleRate = 1000;

    [ObservableProperty]
    private int windowSizeSlider = 10;

    [ObservableProperty]
    private int windowSize = 1024;

    [ObservableProperty]
    private double measurementTime = 60;

    [ObservableProperty]
    private double minimumMeasurementTime = 10;

    [ObservableProperty]
    private double maximumMeasurementTime = 300;

    [ObservableProperty]
    private int measurementTimeTypeIndex;

    [ObservableProperty]
    private int dominantFrequencyClaculationTypeIndex = 0;

    [ObservableProperty]
    private int notchFilter = 50;

    [ObservableProperty]
    private int lowPassFilter = 180;

    [ObservableProperty]
    private int highPassFilter = 5;

    [ObservableProperty]
    private int cornerFrequency = 200;

    [ObservableProperty]
    private bool measurementFixedTime = true;

    [ObservableProperty]
    private int selectedDeviceIndex = 0;

    [ObservableProperty]
    private string[]? devices;

    [ObservableProperty]
    private int selectedPatientIndex;

    [ObservableProperty]
    public List<Patient> patients;

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


    //
    public Dictionary<int, string> DominantFrequencyCalculationTypeStrings
    {
        get; set;
    } = MeasurementGroup.DominantFrequencyCalculationTypeStrings;

    public SetupViewModel(IMeasurementService measurementService, IDataService dataService, INavigationService navigationService)
    {
        _measurementService = measurementService;
        _dataService = dataService;
        _navigationService = navigationService;
        Patients = _dataService.Patients;
        DevicesSelectionChanged();
    }
    partial void OnWindowSizeSliderChanged(int value) => WindowSize = (int)Math.Pow(2, value);
    public void OnNavigatedFrom() { }
    public void OnNavigatedTo(object parameter) { }

    private void DevicesSelectionChanged() => Devices = _measurementService.GetListOfDevices();

    [RelayCommand]
    private void ConfirmButton()
    {
        if (Patients.Count <= SelectedPatientIndex || SelectedPatientIndex < 0) { return; }
        if (MeasurementTimeTypeIndex == 1)
        {
            MeasurementTime *= 60;
            MeasurementTimeTypeIndex = 0;
        }
        var measurementDataSize = (int)(SampleRate * MeasurementTime);
        var measurement = new MeasurementGroup(SampleRate, WindowShiftMilliseconds, WindowSize , MeasurementFixedTime, measurementDataSize,
            DominantFrequencyClaculationTypeIndex, NotchFilter, LowPassFilter, HighPassFilter,CornerFrequency, SelectedDeviceIndex);

        _measurementService.StopRecording();
        _measurementService.CreateConnection(measurement);
        _dataService.CurrentPatientId = Patients[SelectedPatientIndex].PatientId;

        _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
    }

    [RelayCommand]
    private void MeasurementTimeTypeChanged()
    {
        Debug.WriteLine(MeasurementTimeTypeIndex);
        if (MeasurementTimeTypeIndex == 0)
        {
            var time = MeasurementTime * 60;
            MinimumMeasurementTime = 10;
            MaximumMeasurementTime = 300;
            MeasurementTime = Math.Round(time);
        }
        else 
        {
            var time = MeasurementTime / 60;
            MinimumMeasurementTime = 0.16;
            MaximumMeasurementTime = 5;
            MeasurementTime = time;
        }
        return;
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
