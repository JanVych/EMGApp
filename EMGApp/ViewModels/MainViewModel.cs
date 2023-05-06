using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using System.Diagnostics;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using EMGApp.Events;
using EMGApp.Contracts.ViewModels;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Defaults;
using EMGApp.Models;

namespace EMGApp.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConnectionService _connectionService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;
    public ISeries[] Series1 { get; set; } =
    {
        new LineSeries<ObservablePoint>
        {
            Values = new ObservablePoint[]{new ObservablePoint(1,1), new ObservablePoint(2,2), new ObservablePoint(3,3)},
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            IsHoverable = false
        }
    };
    public ISeries[] Series2
    {
        get; set;
    } =
    {
        new LineSeries<double>
        {
            Values = new double[]{1,2,3},
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            IsHoverable = false

        },
        new LineSeries<ObservablePoint>
        {
            Values = new ObservablePoint[] {new ObservablePoint(1,1),},
            Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            IsHoverable = false
        }
    };
    public IEnumerable<ICartesianAxis> XAxes1 { get; set; } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> XAxes2 { get; set; } = new Axis[] { new Axis { Name = "measurements", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> YAxes1 { get; set; } = new Axis[] { new Axis { Name = "µV", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> YAxes2 { get; set; } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };

    [ObservableProperty]
    private string? bufferInMilliseconds;

    [ObservableProperty]
    private string? sampleRate;

    [ObservableProperty]
    private string? deviceName;

    [ObservableProperty]
    private string? windowSize;

    [ObservableProperty]
    private string? dataLenght;

    [ObservableProperty]
    private Patient? currentPatient;

    [ObservableProperty]
    private double slope;

    [ObservableProperty]
    private double shift;

    [ObservableProperty]
    private int muscleSelectedIndex = 0;

    [ObservableProperty]
    private bool side = false;

    public MainViewModel(IConnectionService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _connectionService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;
        BufferInMilliseconds = _connectionService.CurrentMeasurement.BufferMilliseconds.ToString();
        SampleRate = _connectionService.CurrentMeasurement.SampleRate.ToString();
        WindowSize = _connectionService.CurrentMeasurement.WindowSize.ToString();
        DataLenght = _connectionService.CurrentMeasurement.MaxDataLength.ToString();
        DeviceName = _connectionService.GetListOfDevices()[_connectionService.CurrentMeasurement.DeviceNumber];

        CurrentPatient = _dataService.Patients.FirstOrDefault(p => p.PatientId == _dataService.CurrentPatientId);

        _connectionService.SelectMeasuredMuscle(MuscleSelectedIndex, Side ? 1 : 0);
    }
    public void OnNavigatedTo(object parameter) => _connectionService.DataAvailable += DataAvailable;
    public void OnNavigatedFrom() => _connectionService.DataAvailable -= DataAvailable;

    private void DataAvailable(object? sender, DataAvaiableArgs args)
    {
        ObservablePoint[] chartData = new ObservablePoint[args.Size];
        double fconst = (double)_connectionService.CurrentMeasurement.SampleRate / (double)_connectionService.CurrentMeasurement.WindowSize;

        if (args.Data != null)
        {
            for (var i = 0; i < args.Size; i++)
            {
                chartData[i] = (new ObservablePoint(i * fconst, args.Data[i]));
            }
            Debug.WriteLine("points: " + chartData.Length.ToString());
        }

        Series1[0].Values = chartData;
        Series2[0].Values = _connectionService.CurrentMeasurement.MeasurementsData[_connectionService.CMIndex].MaxValues.ToArray();

    }
    [RelayCommand]
    private void ChangeSettings() => _navigationService.NavigateTo(typeof(SetupViewModel).FullName!);
    [RelayCommand]
    private void StartButton() => _connectionService.StartRecording();
    [RelayCommand]
    private void SaveButton() {}
    [RelayCommand]
    private void StopButton()
    {
        _connectionService?.StopRecording();
        MeasurementData data = _connectionService.CurrentMeasurement.MeasurementsData[_connectionService.CMIndex];
        Slope = data.Slope;
        Shift = data.Shift;
        ObservablePoint pointA = new ObservablePoint(0, data.Shift);
        ObservablePoint pointB = new ObservablePoint(data.MaxValues.Count - 1, data.Shift + data.Slope * data.MaxValues.Count);
        Series2[1].Values = new ObservablePoint[] {pointA, pointB};
    }

    [RelayCommand]
    private void MuscleSelectionChanged() => _connectionService.SelectMeasuredMuscle(MuscleSelectedIndex,Side ? 1 : 0);
}
