using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
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
    private readonly IMeasurementService _measurementService;
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

    public MainViewModel(IMeasurementService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _measurementService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;
        BufferInMilliseconds = _measurementService.CurrentMeasurement.BufferMilliseconds.ToString();
        SampleRate = _measurementService.CurrentMeasurement.SampleRate.ToString();
        WindowSize = _measurementService.CurrentMeasurement.WindowSize.ToString();
        DataLenght = _measurementService.CurrentMeasurement.MaxDataLength.ToString();
        DeviceName = _measurementService.GetListOfDevices()[_measurementService.CurrentMeasurement.DeviceNumber];

        CurrentPatient = _dataService.Patients.FirstOrDefault(p => p.PatientId == _dataService.CurrentPatientId);

        _measurementService.SelectMeasuredMuscle(MuscleSelectedIndex, Side ? 1 : 0);
    }
    public void OnNavigatedTo(object parameter) => _measurementService.DataAvailable += DataAvailable;
    public void OnNavigatedFrom() => _measurementService.DataAvailable -= DataAvailable;

    private void DataAvailable(object? sender, DataAvaiableArgs args)
    {
        ObservablePoint[] chartData = new ObservablePoint[args.Size];
        double fconst = (double)_measurementService.CurrentMeasurement.SampleRate / (double)_measurementService.CurrentMeasurement.WindowSize;

        if (args.Data != null)
        {
            for (var i = 0; i < args.Size; i++)
            {
                chartData[i] = (new ObservablePoint(i * fconst, args.Data[i]));
            }
            Debug.WriteLine("points: " + chartData.Length.ToString());
        }

        Series1[0].Values = chartData;
        Series2[0].Values = _measurementService.CurrentMeasurement.MeasurementsData[_measurementService.CMIndex].MaxValues.ToArray();

    }
    [RelayCommand]
    private void ChangeSettings() => _navigationService.NavigateTo(typeof(SetupViewModel).FullName!);
    [RelayCommand]
    private void StartButton() => _measurementService.StartRecording();
    [RelayCommand]
    private void SaveButton() => _dataService.AddMeasurement(_measurementService.CurrentMeasurement);
    [RelayCommand]
    private void StopButton()
    {
        _measurementService.StopRecording();
        var data = _measurementService.CurrentMeasurement.MeasurementsData[_measurementService.CMIndex];
        Slope = data.Slope;
        Shift = data.StartFrequency;
        var pointA = new ObservablePoint(0, data.StartFrequency);
        var pointB = new ObservablePoint(data.MaxValues.Count - 1, data.StartFrequency + data.Slope * data.MaxValues.Count);
        Series2[1].Values = new ObservablePoint[] {pointA, pointB};
    }

    [RelayCommand]
    private void MuscleSelectionChanged() => _measurementService.SelectMeasuredMuscle(MuscleSelectedIndex,Side ? 1 : 0);
}
