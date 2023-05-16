using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using LiveChartsCore.Kernel.Sketches;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using CommunityToolkit.Mvvm.Input;
using EMGApp.Contracts.ViewModels;
using EMGApp.Events;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using EMGApp.Services;

namespace EMGApp.ViewModels;

public partial class MeasurementsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMeasurementService _measurementService;
    private readonly IDataService _dataService;
    public MeasurementGroup? ObservedMeasurement
    {
        get; set;  
    }
    [ObservableProperty]
    public int measurementDataIndex;

    [ObservableProperty]
    private double slope;
    [ObservableProperty]
    private double startFrequency;
    [ObservableProperty]
    private int sliderValue;
    [ObservableProperty]
    private int sliderMaximum;
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
    public ISeries[] InputSignalSeries
    {
        get; set;
    } =
    {
        new LineSeries<short>
        {
            Values = new short[]{1 },
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
            Values = null,
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
    public IEnumerable<ICartesianAxis> FrequencySpectrumXAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> FrequencySpectrumYAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "µV", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> InputSignalXAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "samples", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> InputSignalYAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "µV", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominatValuesXAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "measurements", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesYAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };

    public MeasurementsViewModel(IMeasurementService measurementService, IDataService dataService)
    {
        _measurementService = measurementService;
        _dataService = dataService;
        ObservedMeasurement = _dataService.Measurements.Find(m => m.MeasurementId == _dataService.ObservedMeasuremntId);
        if (ObservedMeasurement != null) 
        {
            if (ObservedMeasurement.MeasurementsData.Count == 0) 
            {
                ObservedMeasurement.MeasurementsData = _dataService.GetMeasurementData(ObservedMeasurement);
            }
            if (ObservedMeasurement.MeasurementsData.Count > _dataService.ObservedMeasurementDataIndex)
            {
                MeasurementDataIndex = _dataService.ObservedMeasurementDataIndex;
            }
            SliderReset();
            DominantValuesChartUpdate();
            SliderValueChanged();
        }
    }
    public void OnNavigatedTo(object parameter)
    {
        if (_dataService.ObservedMeasuremntIsRunning)
        {
            _dataService.ObservedMeasuremntRunEvent += RunEventRecived;
        }
    }
    public void OnNavigatedFrom()
    {
        _dataService.ObservedMeasuremntRunEvent -= RunEventRecived;
        PauseButton();
    }

    private void RunEventRecived(object? sender, ObservedMeasuremntRunStepArgs args) => SliderValue = args.Value;
    [RelayCommand]
    private void SliderValueChanged()
    {
        if (ObservedMeasurement != null)
        {
            ObservedMeasurement.MeasurementsData[MeasurementDataIndex].DataIndex = SliderValue;
            InputSignalChartUpdate();
            FrequencySpectrumChartUpdate();
        }
    }
    [RelayCommand]
    private void MeasuremntDataSelectionChanged()
    {
        _dataService.ObservedMeasurementDataIndex = MeasurementDataIndex;
        PauseButton();
        SliderReset();
        DominantValuesChartUpdate();
    }
    public void SliderReset()
    {
        if (ObservedMeasurement != null)
        {
            SliderValue = ObservedMeasurement.MeasurementsData[MeasurementDataIndex].DataIndex;
            SliderMaximum = ObservedMeasurement.DataSize;
        }
    }
    private void DominantValuesChartUpdate()
    {
        if (ObservedMeasurement != null && MeasurementDataIndex >= 0) 
        {
            DominantValuesSeries[0].Values = ObservedMeasurement.MeasurementsData[MeasurementDataIndex].DominantValues;
            Slope = ObservedMeasurement.MeasurementsData[MeasurementDataIndex].Slope;
            StartFrequency = ObservedMeasurement.MeasurementsData[MeasurementDataIndex].StartFrequency;
        }
    }
    private void FrequencySpectrumChartUpdate()
    {
        if (ObservedMeasurement != null && MeasurementDataIndex >= 0)
        {
            if (SliderValue >= ObservedMeasurement.WindowLength)
            {
                var frequencySpectrumData = _measurementService.CalculateFrequencySpecturm(ObservedMeasurement, MeasurementDataIndex);

                var frequencySpectrumPoints = new ObservablePoint[ObservedMeasurement.FrequencyDataSize];
                var fconst = (double)ObservedMeasurement.SampleRate / (double)ObservedMeasurement.WindowLength;

                for (var i = 0; i < ObservedMeasurement.FrequencyDataSize; i++)
                {
                    frequencySpectrumPoints[i] = (new ObservablePoint(i * fconst, frequencySpectrumData[i]));
                }
                FrequencySpectrumSeries[0].Values = frequencySpectrumPoints;
            }
            else
            {
                FrequencySpectrumSeries[0].Values = new ObservablePoint[] {new ObservablePoint(1,1) };
            }
        }
    }
    private void InputSignalChartUpdate()
    {
        Debug.WriteLine("InputSig");
        if (ObservedMeasurement != null && MeasurementDataIndex >= 0)
        {
            var size = ObservedMeasurement.NumberOfSamplesOnWindowShift;
            if (SliderValue >= size)
            {
                var inputSignalData = new short[size];
                Buffer.BlockCopy(ObservedMeasurement.MeasurementsData[MeasurementDataIndex].Data, SliderValue - size, inputSignalData, 0, size * sizeof(short));
                InputSignalSeries[0].Values = inputSignalData;
            }
            else
            {
                InputSignalSeries[0].Values = new short[] {1 };
            }
        }
    }
    [RelayCommand]
    private async void StartButton()
    {
        if (ObservedMeasurement != null && !_dataService.ObservedMeasuremntIsRunning)
        {
            _dataService.ObservedMeasuremntRunEvent += RunEventRecived;
            await _dataService.ObservedMeasuremntRunAsync(ObservedMeasurement, MeasurementDataIndex);
        }
    }
    [RelayCommand]
    private void PauseButton()
    {
        if (_dataService.ObservedMeasuremntIsRunning)
        {
            _dataService.ObservedMeasuremntIsRunning = false;
        }
    }

    [RelayCommand]
    private void ResetButton()
    {
        _dataService.ObservedMeasuremntIsRunning = false;
        SliderValue = 0;
    }

}

