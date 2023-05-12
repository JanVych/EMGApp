using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Contracts.ViewModels;
using EMGApp.Models;
using EMGApp.Services;

namespace EMGApp.ViewModels;

public partial class SetupViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMeasurementService _connectionService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int bufferInMilliseconds = 100;

    [ObservableProperty]
    private int sampleRate = 1000;

    [ObservableProperty]
    private int windowSizeSlider = 10;

    [ObservableProperty]
    private int windowSize = 1024;

    [ObservableProperty]
    private int measurementTime = 120_000;

    [ObservableProperty]
    private int selectedDeviceIndex = 0;

    [ObservableProperty]
    private string[]? devices;

    [ObservableProperty]
    private int selectedPatientIndex;

    public List<Patient> Patients { get; set; }

    public ICommand ConnectButtonCommand
    {
        get;
    }
    public SetupViewModel(IMeasurementService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _connectionService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;
        Patients = _dataService.Patients;
        ConnectButtonCommand = new RelayCommand(ConnectButtonClick);
        DevicesSelectionChanged();
    }
    partial void OnWindowSizeSliderChanged(int value) => WindowSize = (int)Math.Pow(2, value);
    public void OnNavigatedFrom() { }
    public void OnNavigatedTo(object parameter) { }

    private void DevicesSelectionChanged() => Devices = _connectionService.GetListOfDevices();
    private void ConnectButtonClick()
    {
        if (Patients.Count <= SelectedPatientIndex) { return; }
        _connectionService.StopRecording();
        _connectionService.CreateConnection(0, SampleRate, BufferInMilliseconds, WindowSize, true, MeasurementTime, SelectedDeviceIndex);
        _dataService.CurrentPatientId = Patients[SelectedPatientIndex].PatientId;
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
    }
}
