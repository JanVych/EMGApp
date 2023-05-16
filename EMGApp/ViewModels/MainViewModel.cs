using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using System.Diagnostics;
using SkiaSharp;
using EMGApp.Events;
using EMGApp.Contracts.ViewModels;
using EMGApp.Models;
using Windows.System;

namespace EMGApp.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly IMeasurementService _measurementService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;
    public ISeries[] FrequencySpectrumSeries
    {
        get; set;
    } =
    {
        new LineSeries<ObservablePoint>
        {
            Values = new ObservablePoint[]{new ObservablePoint(1,1)},
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            IsHoverable = false
        }
    };
    public ISeries[] DominantValuesSeries
    {
        get; set;
    } =
    {
        new LineSeries<double>
        {
            Values = new double[]{1 },
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
            Values = null,
            Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            IsHoverable = false
        }
    };
    public IEnumerable<ICartesianAxis> FrequencySpectrumXAxes { get; set; } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesXAxes { get; set; } = new Axis[] { new Axis { Name = "measurements", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> FrequencySpectrumYAxes { get; set; } = new Axis[] { new Axis { Name = "µV", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesYAxes { get; set; } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };

    [ObservableProperty]
    private string? bufferInMilliseconds;

    [ObservableProperty]
    private string? sampleRate;

    [ObservableProperty]
    private string? deviceName;

    [ObservableProperty]
    private string? windowSize;

    [ObservableProperty]
    private string? dataSize;

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

    [ObservableProperty]
    private int progressBarValue = 0;

    public MainViewModel(IMeasurementService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _measurementService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;
        BufferInMilliseconds = _measurementService.CurrentMeasurement.BufferMilliseconds.ToString();
        SampleRate = _measurementService.CurrentMeasurement.SampleRate.ToString();
        WindowSize = _measurementService.CurrentMeasurement.WindowLength.ToString();
        DataSize = _measurementService.CurrentMeasurement.DataSize.ToString();
        DeviceName = _measurementService.GetListOfDevices()[_measurementService.CurrentMeasurement.DeviceNumber];

        CurrentPatient = _dataService.Patients.FirstOrDefault(p => p.PatientId == _dataService.CurrentPatientId);

        _measurementService.SelectMeasuredMuscle(MuscleSelectedIndex, Side ? 1 : 0);
        UpdateCharts();
    }
    public void OnNavigatedTo(object parameter) => _measurementService.DataAvailable += DataAvailable;
    public void OnNavigatedFrom() => _measurementService.DataAvailable -= DataAvailable;

    private void DataAvailable(object? sender, DataAvaiableArgs args)
    {
        var frequencySpectrumData = new ObservablePoint[args.Size];
        var fconst = (double)_measurementService.CurrentMeasurement.SampleRate / (double)_measurementService.CurrentMeasurement.WindowLength;

        if (args.Data != null)
        {
            for (var i = 0; i < args.Size; i++)
            {
                frequencySpectrumData[i] = (new ObservablePoint(i * fconst, args.Data[i]));
            }
            Debug.WriteLine("points: " + frequencySpectrumData.Length.ToString());
        }

        FrequencySpectrumSeries[0].Values = frequencySpectrumData;

        DominantValuesSeries[0].Values = args.DominantValues;

        _dispatcherQueue.TryEnqueue(() =>
        {
            ProgressBarValue = _measurementService.CurrentMeasurement.MeasurementsData[_measurementService.CMIndex].DataIndex;
        });
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
        UpdateCharts();
    }

    [RelayCommand]
    private void MuscleSelectionChanged()
    {
        _measurementService.StopRecording();
        _measurementService.SelectMeasuredMuscle(MuscleSelectedIndex, Side ? 1 : 0);
        UpdateCharts();
        ProgressBarValue = 0;
    }

    private void UpdateCharts()
    {
        var data = _measurementService.CurrentMeasurement.MeasurementsData[_measurementService.CMIndex];
        Slope = data.Slope;
        Shift = data.StartFrequency;
        var dominantValueIndex = data.DataIndex / _measurementService.CurrentMeasurement.BufferMilliseconds;
        if (Slope != 0)
        {
            var pointA = new ObservablePoint(0, data.StartFrequency);
            var pointB = new ObservablePoint(dominantValueIndex - 1, data.StartFrequency + (data.Slope / 1000_000) * dominantValueIndex);
            DominantValuesSeries[1].Values = new ObservablePoint[] { pointA, pointB };
        }
        else
        {
            DominantValuesSeries[1].Values = null;
        }
        FrequencySpectrumSeries[0].Values = new ObservablePoint[] { new ObservablePoint(1, 1) };
        var dominantValues = _measurementService.CurrentMeasurement.MeasurementsData[_measurementService.CMIndex].DominantValues;
        if (dominantValues.Length != 0)
        {
            DominantValuesSeries[0].Values = dominantValues.ToArray();
        }
        else
        {
            DominantValuesSeries[0].Values = new double[] { 1};
        }
    }
}
