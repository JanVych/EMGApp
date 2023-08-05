using EMGApp.Contracts.Services;
using EMGApp.Models;
using NAudio.Wave;
using System.Diagnostics;
using EMGApp.Events;
using LiveChartsCore.Defaults;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace EMGApp.Services;
public class MeasurementService : IMeasurementService
{
    private WaveInEvent? Wawe
    {
        get; set;
    }
    //current measurement group
    public MeasurementGroup CurrentMeasurement
    {
        get; set;
    }
    //current measurement data index
    public int CMDataIndex
    {
        get; set;
    } = -1;
    public bool IsRecording
    {
        get; set;
    } = false;
    public EventHandler<DataAvaiableArgs>? DataAvailable
    {
        get; set;
    }

    public MeasurementService()
    {
        App.MainWindow.Closed += (object sender, WindowEventArgs args) => { StopRecording(); };
        CurrentMeasurement = CreateDefaultMeasurement();
    }

    public MeasurementGroup CreateDefaultMeasurement()
    {
        // TO DO - handle device number
        var m = new MeasurementGroup(1000, 100, 1024, true, 60_000, 0, 50, 180, 5, 200, 0);
        CreateConnection(m);
        return m;
    }
    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        var rawData = CurrentMeasurement.MeasurementsData[CMDataIndex];
        for (var i = 0; i < e.Buffer.Length / 2; i++)
        {
            if (rawData.DataIndex >= CurrentMeasurement.DataSize)
            {
                //Buffer is full
                StopRecording();
                return;
            }
            CurrentMeasurement.MeasurementsData[CMDataIndex].Data[rawData.DataIndex] = BitConverter.ToInt16(e.Buffer, i * 2);
            rawData.DataIndex++;
        }

        if (rawData.DataIndex - CurrentMeasurement.WindowLength < 0)
        {
            //Not enough data for window
            DataAvailable?.Invoke(this, new DataAvaiableArgs(rawData.DataIndex, null, null));
            return;
        }

        var data = CalculateFrequencySpecturm(CurrentMeasurement, CMDataIndex);
        var dominantValue = CalculateDominantValue(CurrentMeasurement, CMDataIndex, data);

        var pData = new ObservablePoint[CurrentMeasurement.SpectrumDataSize];
        for (var i = 0; i < CurrentMeasurement.SpectrumDataSize; i++)
        {
            pData[i] = new ObservablePoint(i * CurrentMeasurement.SpectralResolution, data[i]);
        }
        
        DataAvailable?.Invoke(this, new DataAvaiableArgs(rawData.DataIndex, pData, dominantValue));
    }

    public double[] CalculateFrequencySpecturm(MeasurementGroup measurement, int mIndex)
    {
        var mData = measurement.MeasurementsData[mIndex];
        if (mData.DataIndex - measurement.WindowLength < 0)
        {
            return Array.Empty<double>();   
        }
        var startIndex = mData.DataIndex - measurement.WindowLength;
        var rawDataIndex = 0;
        var rawData = new double[measurement.WindowLength];

        for (; startIndex < mData.DataIndex; startIndex++)
        {
            rawData[rawDataIndex] = mData.Data[startIndex];
            rawDataIndex++;
        }

        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(rawData);

        var fftComplex = FftSharp.Transform.FFT(rawData);

        var data = new double[measurement.SpectrumDataSize];
        for (var i = 0; i < measurement.SpectrumDataSize; i++)
        {
            data[i] = Math.Sqrt(fftComplex[i].Real * fftComplex[i].Real + fftComplex[i].Imaginary * fftComplex[i].Imaginary);
        }

        HighPassFilter(measurement, data);
        LowPassFilter(measurement, data);
        NotchFilter(measurement, data);

        return data;
    }

    public ObservablePoint? CalculateDominantValue(MeasurementGroup measurement, int mIndex, double[] data)
    {
        var mData = measurement.MeasurementsData[mIndex];
        var dominantValuesIndex = mData.DominatValuesIndex(measurement.NumberOfSamplesOnWindowShift, measurement.WindowLength);
        double dominantValue;
        if (dominantValuesIndex < 0)
        {
            return null;
        }
        if (measurement.DominantFrequencyCalculationType == 0)
        {
            dominantValue = CalculateMedianValue(measurement, data);
        }
        else 
        {
            dominantValue = CalculateMeanValue(measurement, data);
        }

        mData.DominantValues[dominantValuesIndex] = dominantValue;

        //moving avrage
        var numberOfAvragedSamples = (int)(measurement.MovingAvrageWindowTimeSeconds / measurement.WindowShiftSeconds);
        double sum = 0;
        double positionX;
        if (dominantValuesIndex >= numberOfAvragedSamples - 1)
        {
            for (var i = dominantValuesIndex - numberOfAvragedSamples + 1; i <= dominantValuesIndex; i++)
            {
                sum += mData.DominantValues[i];
            }
            positionX = (dominantValuesIndex + 1) * measurement.WindowShiftSeconds + measurement.WindowLength / (double)measurement.SampleRate;
            return new ObservablePoint(positionX, sum / numberOfAvragedSamples);
        }
        return null;
    //return new ObservablePoint((dominatValuesIndex + 1) * measurement.WindowShiftSeconds, dominantValue);
    }
    public void CalculateSlopeAndShift(MeasurementGroup measurement, int mIndex)
    {
        var mData = measurement.MeasurementsData[mIndex];
        var dominatValuesLength = mData.DominatValuesIndex(measurement.NumberOfSamplesOnWindowShift, measurement.WindowLength);
        var dominantValuesY = new double[dominatValuesLength];
        var timeValuesX = new double[dominatValuesLength];
        for (var i = 0; i < dominatValuesLength; i++)
        {
            dominantValuesY[i] = mData.DominantValues[i];
            timeValuesX[i] = measurement.WindowShiftSeconds * (i + 1);
        }
        var avrageX = timeValuesX.Average();
        var avrageY = dominantValuesY.Average();
        double denominator = 0;
        double numerator = 0;
        for (var i = 0; i < timeValuesX.Length; i++)
        {
            numerator += (timeValuesX[i] - avrageX) * (dominantValuesY[i] - avrageY);

            denominator += Math.Pow((timeValuesX[i] - avrageX), 2);
        }
        var slope = (numerator / denominator);
        mData.StartFrequency = avrageY - slope * avrageX;
        mData.Slope = slope;
    }

    private double CalculateMeanValue(MeasurementGroup measurement, double[] data)
    {
        double sum = 0;
        double sumI = 0;
        for (var i = 0; i < measurement.SpectrumDataSize; i++)
        {
            sum += data[i];
            sumI += data[i] * i;
        }
        var mean = (1.0 / sum) * measurement.SpectralResolution * sumI;
        return mean;
    }
    private double CalculateMedianValue(MeasurementGroup measurement, double[] data)
    {
        double sum = 0;
        double probabilitySum = 0;
        double lowIndex = 0;
        double deltaHigh = 0;
        double deltaLow = 0;
        for (var i = 0; i < measurement.SpectrumDataSize; i++)
        {
            sum += data[i];
        }
        for (var i = 0; i < measurement.SpectrumDataSize; i++)
        {
            probabilitySum += data[i] / sum;
            if (probabilitySum > 0.5 && i > 0) 
            {
                lowIndex = i - 1;
                deltaHigh = probabilitySum - 0.5;
                deltaLow = 0.5 - (probabilitySum - (data[i] / sum));
                break;
            }
        }
        var result = (lowIndex + (deltaLow / (deltaHigh + deltaLow))) * measurement.SpectralResolution;
        return result;
    }
    private void NotchFilter(MeasurementGroup measurement, double[] data)
    {
        var index = (int)Math.Round(measurement.NotchFilter / (double)measurement.SpectralResolution);
        data[index] = 0;
        data[index + 1] = 0;
        data[index - 1] = 0;
    }
    private void LowPassFilter(MeasurementGroup measurement, double[] data)
    {
        var index = Math.Round(measurement.LowPassFilter / (double)measurement.SpectralResolution);
        for (var i = measurement.SpectrumDataSize - 1; i > index; i--)
        {
            data[i] = 0;
        }
    }
    private void HighPassFilter(MeasurementGroup measurement, double[] data)
    {
        var index = Math.Round(measurement.HighPassFilter / (double)measurement.SpectralResolution);
        for (var i = 0; i < index; i++)
        {
            data[i] = 0;
        }
    }

    public void CreateConnection(MeasurementGroup measurement)
    {
        CMDataIndex = -1;
        CurrentMeasurement = measurement;
        Wawe = new()
        {
            DeviceNumber = measurement.DeviceNumber,
            WaveFormat = new WaveFormat(measurement.SampleRate, 16, 1),
            BufferMilliseconds = measurement.WindowShiftMilliseconds
        };
        Wawe.DataAvailable += WaveIn_DataAvailable;
    }
    
    public void SelectOrAddMuscle(int muscleType, int side, int measurementType, int force)
    {
        var index = CurrentMeasurement.MeasurementsData.FindIndex(m => m.MuscleType == muscleType && m.Side == side);
        if (index == -1)
        {
            index = CurrentMeasurement.MeasurementsData.Count;
            CurrentMeasurement.MeasurementsData.Add
                (new MeasurementData(muscleType, side, CurrentMeasurement.DataSize, CurrentMeasurement.DominantValuesSize, measurementType, force));
        }
        if (CMDataIndex >= 0)
        {
            CurrentMeasurement.MeasurementsData[CMDataIndex].IsActive = false;
        }
        CMDataIndex = index;
        CurrentMeasurement.MeasurementsData[CMDataIndex].IsActive = true;
    }

    public void StartRecording()
    {
        if (!IsRecording && Wawe != null && CMDataIndex >= 0)
        {
            Wawe.StartRecording();
            IsRecording = true;
        }
    }

    public void StopRecording()
    {
        if (IsRecording && Wawe != null && CMDataIndex >= 0) 
        {
            Wawe.StopRecording();
            IsRecording = false;
            CalculateSlopeAndShift(CurrentMeasurement, CMDataIndex);
        }
    }
    public ObservableCollection<ObservablePoint> GetAvragedDominantValues(MeasurementGroup measurement, int mDataIndex, int size)
    {
        ObservableCollection<ObservablePoint> dominantValues = new();
        var mData = measurement.MeasurementsData[mDataIndex];
        if (size > measurement.DominantValuesSize) 
        { 
            size = measurement.DominantValuesSize; 
        }

        var numberOfAvragedSamples = (int)(measurement.MovingAvrageWindowTimeSeconds / measurement.WindowShiftSeconds);
        double sum = 0;
        double positionX;
        for (var i = numberOfAvragedSamples - 1; i < size; i++)
        {
            for (var j = i - numberOfAvragedSamples + 1; j <= i; j++)
            {
                sum += mData.DominantValues[j];
            }
            positionX = (i + 1) * measurement.WindowShiftSeconds + measurement.WindowLength / (double)measurement.SampleRate;
            dominantValues.Add(new ObservablePoint(positionX, sum / numberOfAvragedSamples));
            sum = 0;
        }
        return dominantValues;
    }

    public string[] GetListOfDevices()
    {
        var waveInDevices = WaveIn.DeviceCount;
        var devices = new string[waveInDevices];
        for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
        {
            devices[waveInDevice] = WaveIn.GetCapabilities(waveInDevice).ProductName.ToString();
        }
        return devices;
        //var MMDEnumerator = new MMDeviceEnumerator();
        //var endpoints = MMDEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        //var devices = new string[endpoints.Count];
        //for (var device = 0; device < endpoints.Count; device++)
        //{
        //    devices[device] = endpoints[device].FriendlyName;
        //}
        //return devices;
    }
}
