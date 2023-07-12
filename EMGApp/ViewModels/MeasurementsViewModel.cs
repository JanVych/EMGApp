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
using System.Collections.ObjectModel;

namespace EMGApp.ViewModels;

public partial class MeasurementsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMeasurementService _measurementService;
    private readonly IDataService _dataService;

    private ObservableCollection<ObservablePoint> _dominantValuesPoints = new();
    private ObservableCollection<ObservablePoint> _regressionLinePoints = new();

    public Patient? ObservedPatient
    {
        get; set; 
    }
    public MeasurementGroup? ObservedMeasurement
    {
        get; set;  
    }
    // Observed measurement data index
    [ObservableProperty]
    public int oMDataIndex;

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
         new LineSeries<ObservablePoint>
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
    } = new Axis[] { new Axis { Name = "s", NamePaint = new SolidColorPaint(SKColors.Gray) } };
    public IEnumerable<ICartesianAxis> DominantValuesYAxes
    {
        get; set;
    } = new Axis[] { new Axis { Name = "Hz", NamePaint = new SolidColorPaint(SKColors.Gray) } };

    public MeasurementsViewModel(IMeasurementService measurementService, IDataService dataService)
    {
        _measurementService = measurementService;
        _dataService = dataService;
        ObservedMeasurement = _dataService.ObservedMeasurement;
        ObservedPatient = _dataService.ObservedPatient;
        if (ObservedMeasurement != null) 
        {
            if (ObservedMeasurement.MeasurementsData.Count == 0) 
            {
                ObservedMeasurement.MeasurementsData = _dataService.GetMeasurementData(ObservedMeasurement);
            }
            if (ObservedMeasurement.MeasurementsData.Count > _dataService.ObservedMeasurementDataIndex)
            {
                OMDataIndex = _dataService.ObservedMeasurementDataIndex;
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
            ObservedMeasurement.MeasurementsData[OMDataIndex].DataIndex = SliderValue;
            InputSignalChartUpdate();
            FrequencySpectrumChartUpdate();
        }
    }
    [RelayCommand]
    private void MeasuremntDataSelectionChanged()
    {
        _dataService.ObservedMeasurementDataIndex = OMDataIndex;
        PauseButton();
        SliderReset();
        DominantValuesChartUpdate();
    }
    public void SliderReset()
    {
        if (ObservedMeasurement != null)
        {
            SliderValue = ObservedMeasurement.MeasurementsData[OMDataIndex].DataIndex;
            SliderMaximum = ObservedMeasurement.DataSize;
        }
    }
    private void DominantValuesChartUpdate()
    {
        if (ObservedMeasurement != null && OMDataIndex >= 0) 
        {
            var m = ObservedMeasurement;
            var data = m.MeasurementsData[OMDataIndex];

            _dominantValuesPoints = _measurementService.GetAvragedDominantValues(m, OMDataIndex, m.DominantValuesSize);
            DominantValuesSeries[0].Values = _dominantValuesPoints;
            DominantValuesSeries[1].Values = _regressionLinePoints;
            Slope = data.Slope;
            StartFrequency = data.StartFrequency;
            if (Slope != 0)
            {
                var startPoint = new ObservablePoint(m.WindowLength / m.SampleRate, data.StartFrequency);
                var lastPointIndex = _dominantValuesPoints.Count * m.WindowShiftSeconds + m.WindowLength / m.SampleRate + m.MovingAvrageWindowTimeSeconds;
                var lastPointValue = data.StartFrequency + data.Slope * lastPointIndex;
                var lastPoint = new ObservablePoint(lastPointIndex, lastPointValue);
                _regressionLinePoints.Clear();
                _regressionLinePoints.Add(startPoint);
                _regressionLinePoints.Add(lastPoint);
            }
        }
    }
    private void FrequencySpectrumChartUpdate()
    {
        if (ObservedMeasurement != null && OMDataIndex >= 0)
        {
            if (SliderValue >= ObservedMeasurement.WindowLength)
            {
                var frequencySpectrumData = _measurementService.CalculateFrequencySpecturm(ObservedMeasurement, OMDataIndex);

                var frequencySpectrumPoints = new ObservablePoint[ObservedMeasurement.FrequencyDataSize];

                for (var i = 0; i < ObservedMeasurement.FrequencyDataSize; i++)
                {
                    frequencySpectrumPoints[i] = (new ObservablePoint(i * ObservedMeasurement.SpectralResolution, frequencySpectrumData[i]));
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
        if (ObservedMeasurement != null && OMDataIndex >= 0)
        {
            var size = ObservedMeasurement.NumberOfSamplesOnWindowShift;
            if (SliderValue >= size)
            {
                var inputSignalData = new short[size];
                Buffer.BlockCopy(ObservedMeasurement.MeasurementsData[OMDataIndex].Data, SliderValue - size, inputSignalData, 0, size * sizeof(short));
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
            await _dataService.ObservedMeasuremntRunAsync(ObservedMeasurement, OMDataIndex);
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

