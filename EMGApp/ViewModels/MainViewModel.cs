using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.Services;
using SkiaSharp;
using EMGApp.Events;
using EMGApp.Contracts.ViewModels;
using EMGApp.Models;
using Windows.System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;

namespace EMGApp.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly IMeasurementService _measurementService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    private ObservableCollection<ObservablePoint> _dominantValuesPoints = new();
    private ObservableCollection<ObservablePoint> _regressionLinePoints = new();

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
            IsHoverable = false
        }
    };
    public ISeries[] DominantValuesSeries
    {
        get; set;
    } =
    {
        new LineSeries<ObservablePoint>
        {
            Values = null,
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            IsHoverable = false
            
        },
        new LineSeries<ObservablePoint>
        {
            Values = null,
            Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            IsHoverable = false
        }
    };
    public IEnumerable<ICartesianAxis> FrequencySpectrumXAxes { get; set; } = new Axis[] 
    { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesXAxes { get; set; } = new Axis[] 
    { new Axis { Name = "s", NamePaint = new SolidColorPaint(SKColors.Gray)} };
    public IEnumerable<ICartesianAxis> FrequencySpectrumYAxes { get; set; } = new Axis[] 
    { new Axis { Name = "µV", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesYAxes { get; set; } = new Axis[] 
    { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };

    [ObservableProperty]
    private string? deviceName;

    [ObservableProperty]
    private double slope;

    [ObservableProperty]
    private double startFrequency;

    // new muscle select
    [ObservableProperty]
    private int selectedMuscleType = 0;

    [ObservableProperty]
    private int side = 0;

    [ObservableProperty]
    private int selectedMeasurementType = 0;

    [ObservableProperty]
    private int force = 100;
    public Dictionary<int, string> MuscleTypeStrings
    {
        get; set;
    } = MeasurementData.MuscleTypeStrings;
    public Dictionary<int, string> MeasurementTypeStrings
    {
        get; set;
    } = MeasurementData.MeasuremntTypeStrings;

    // progress bar
    [ObservableProperty]
    private int progressBarValue = 0;

    [ObservableProperty]
    private double progressTimeValue = 0;

    [ObservableProperty]
    private List<MeasurementData>? currentMeasurementData;

    public MeasurementGroup CurrentMeasurement
    {
        get; set;
    }
    public Patient? CurrentPatient
    {
        get; set;
    }

    public MainViewModel(IMeasurementService connectionService, IDataService dataService, INavigationService navigationService)
    {
        _measurementService = connectionService;
        _dataService = dataService;
        _navigationService = navigationService;

        CurrentPatient = _dataService.CurrentPatient;
        CurrentMeasurement = _measurementService.CurrentMeasurement;
        CurrentMeasurementData = CurrentMeasurement.MeasurementsData;
        DeviceName = _measurementService.GetListOfDevices()[CurrentMeasurement.DeviceNumber];

        DominantValuesSeries[0].Values = _dominantValuesPoints;
        DominantValuesSeries[1].Values = _regressionLinePoints;

        if (CurrentMeasurementData.Count != 0)
        {
           UpdateView();
        }
    }
    public void OnNavigatedTo(object parameter) => _measurementService.DataAvailable += DataAvailable;
    public void OnNavigatedFrom() => _measurementService.DataAvailable -= DataAvailable;

    private void DataAvailable(object? sender, DataAvaiableArgs args)
    {
        if (_measurementService.IsRecording)
        {
            FrequencySpectrumSeries[0].Values = args.Data;
            //!! "thread save"
            _dominantValuesPoints.Add(args.DominantValue);

            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateProgressBar();
            });
        }
    }

    [RelayCommand]
    private void ChangeSettings()
    {
        _measurementService.StopRecording();
        _navigationService.NavigateTo(typeof(SetupViewModel).FullName!);
    }
    [RelayCommand]
    private void StartButton()
    {
        _regressionLinePoints.Clear();
        _measurementService.StartRecording();
    }
    [RelayCommand]
    private void SaveButton() 
    {;
        _measurementService.StopRecording();
        UpdateView();
        _dataService.AddMeasurement(_measurementService.CurrentMeasurement);
    }
    [RelayCommand]
    private void StopButton()
    {
        if (_measurementService.IsRecording)
        {
            _measurementService.StopRecording();
            UpdateView();
        }
    }
    [RelayCommand]
    private void ResetButton()
    {
        if (CurrentMeasurementData != null && _measurementService.CMDataIndex >= 0)
        {
            _measurementService.StopRecording();
            var m = CurrentMeasurementData[_measurementService.CMDataIndex];
            CurrentMeasurementData[_measurementService.CMDataIndex] = new 
                MeasurementData(m.MuscleType, m.Side, m.Data.Length, m.DominantValues.Length, m.MeasurementType, m.Force);
            UpdateView();
        }
    }
    [RelayCommand]
    private void AddMuscleButton()
    {
        _measurementService.StopRecording();
        _measurementService.SelectOrAddMuscle(SelectedMuscleType, Side, SelectedMeasurementType, Force);
        CurrentMeasurementData = null;
        CurrentMeasurementData = CurrentMeasurement?.MeasurementsData;
        UpdateView();
    }
    internal void MuscleButton(int muscletype, int side)
    {
        if (CurrentMeasurementData != null)
        {
            _measurementService.StopRecording();
            _measurementService.SelectOrAddMuscle(muscletype, side, SelectedMeasurementType, Force);
            for (var i = 0; i < CurrentMeasurementData.Count; i++)
            {
                if (CurrentMeasurementData[i].MuscleType != muscletype || CurrentMeasurementData[i].Side != side)
                {
                    CurrentMeasurementData[i].IsActive = false;
                }
                else
                {
                    CurrentMeasurementData[i].IsActive = true;
                }
            }
            CurrentMeasurementData = null;
            CurrentMeasurementData = CurrentMeasurement?.MeasurementsData;
            UpdateView();
        }
    }
    private void UpdateProgressBar()
    {
        if (CurrentMeasurementData != null && CurrentMeasurementData.Count > 0) 
        {
            ProgressBarValue = CurrentMeasurementData[_measurementService.CMDataIndex].DataIndex;
            ProgressTimeValue = Math.Round((double)ProgressBarValue / CurrentMeasurement.SampleRate, 2);
            if (ProgressBarValue >= CurrentMeasurement.DataSize)
            {
                UpdateSlopeAndStartFrequency();
                UpdateCharts();
            }
        }
    }
    private void UpdateCharts()
    {
        //bad
        FrequencySpectrumSeries[0].Values = new ObservablePoint[]{new ObservablePoint(0,0)};  
        _dominantValuesPoints.Clear();
        _regressionLinePoints.Clear();
        DominantValuesSeries[0] = new LineSeries<ObservablePoint>
        {
            Values = _dominantValuesPoints,
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            IsHoverable = false

        };
        DominantValuesSeries[1] = new LineSeries<ObservablePoint>
        {
            Values = _regressionLinePoints,
            Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
            Fill = null,
            LineSmoothness = 1,
            GeometrySize = 0,
            IsHoverable = false
        };

        if (CurrentMeasurementData != null && CurrentMeasurementData.Count > 0) 
        {
            var data = CurrentMeasurementData[_measurementService.CMDataIndex];
            var dominantValuesIndex = data.DominatValuesIndex(CurrentMeasurement.NumberOfSamplesOnWindowShift, CurrentMeasurement.WindowLength);
            var windowShiftSeconds = CurrentMeasurement.WindowShiftSeconds;
            for (var i = 0; i <= dominantValuesIndex - 1; i++)
            {
                _dominantValuesPoints.Add(new ObservablePoint((i + 1) * windowShiftSeconds, data.DominantValues[i]));
            }
            if (data.Slope != 0)
            {
                var index = _dominantValuesPoints.Count - 1;
                var startP = new ObservablePoint(0, data.Shift);
                var last = data.Shift + data.Slope * index * CurrentMeasurement.WindowShiftMilliseconds / 1000.0;
                var slopeP = new ObservablePoint(index * CurrentMeasurement.WindowShiftSeconds, last);
                _regressionLinePoints.Add(startP);
                _regressionLinePoints.Add(slopeP);
            }
        }
    }
    private void UpdateSlopeAndStartFrequency()
    {
        if (CurrentMeasurementData != null)
        {
            var data = CurrentMeasurementData[_measurementService.CMDataIndex];
            Slope = Math.Round(data.Slope, 4);
            StartFrequency = Math.Round(data.Shift, 4);  
        }
    }
    private void UpdateView()
    {
        UpdateProgressBar();
        UpdateSlopeAndStartFrequency();
        UpdateCharts();
    }
}
