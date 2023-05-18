using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using EMGApp.Contracts.ViewModels;
using EMGApp.Models;

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
    private int measurementTimeInSec = 120;

    [ObservableProperty]
    private int force;

    [ObservableProperty]
    private int dominantFrequencyClaculationTypeIndex = 0;

    [ObservableProperty]
    private int notchFilter;

    [ObservableProperty]
    private int lowPassFilter;

    [ObservableProperty]
    private int highPassFilter;

    [ObservableProperty]
    private bool measurementFixedTime = false;

    [ObservableProperty]
    private int measurementTypeIndex = 0;

    [ObservableProperty]
    private int selectedDeviceIndex = 0;

    [ObservableProperty]
    private string[]? devices;

    [ObservableProperty]
    private int selectedPatientIndex;

    public List<Patient> Patients { get; set; }

    public Dictionary<int, string> MeasurementTypeStrings
    {
        get; set;
    } = MeasurementGroup.MeasuremntTypeStrings;

    public Dictionary<int, string> DominantFrequencyCalculationTypeStrings
    {
        get; set;
    } = MeasurementGroup.DominantFrequencyCalculationTypeStrings;

    public SetupViewModel(IMeasurementService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _connectionService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;
        Patients = _dataService.Patients;
        DevicesSelectionChanged();
    }
    partial void OnWindowSizeSliderChanged(int value) => WindowSize = (int)Math.Pow(2, value);
    public void OnNavigatedFrom() { }
    public void OnNavigatedTo(object parameter) { }

    private void DevicesSelectionChanged() => Devices = _connectionService.GetListOfDevices();

    [RelayCommand]
    private void ConnectButton()
    {
        if (Patients.Count <= SelectedPatientIndex) { return; }
        _connectionService.StopRecording();
        _connectionService.CreateConnection(0, SampleRate, BufferInMilliseconds, WindowSize, true, MeasurementTimeInSec, SelectedDeviceIndex);
        _dataService.CurrentPatientId = Patients[SelectedPatientIndex].PatientId;
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
    }
}
