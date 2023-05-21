using EMGApp.Contracts.Services;
using EMGApp.Models;
using NAudio.Wave;
using System.Diagnostics;
using EMGApp.Events;
using NAudio.CoreAudioApi;

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
    //current measurement group index
    public int CMIndex
    {
        get; set;
    } = 0;
    public bool IsRecording
    {
        get; set;
    }
    public EventHandler<DataAvaiableArgs>? DataAvailable
    {
        get; set;
    }

    public MeasurementService()
    {
        CurrentMeasurement = new MeasurementGroup(1000, 100, 1024, true, 1_048_576, 0, 0, 0, 0, 0, 0, 0);
        CurrentMeasurement.MeasurementsData.Add(new MeasurementData(0, 0, CurrentMeasurement.DataSize,CurrentMeasurement.DominantValuesSize));
        IsRecording = false;

    }
    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        var rawData = CurrentMeasurement.MeasurementsData[CMIndex];
        for (var i = 0; i < e.Buffer.Length / 2; i++)
        {
            if (rawData.DataIndex >= CurrentMeasurement.DataSize)
            {
                Debug.WriteLine("Buffer is full");
                StopRecording();
                return;
            }
            CurrentMeasurement.MeasurementsData[CMIndex].Data[rawData.DataIndex] = BitConverter.ToInt16(e.Buffer, i * 2);
            rawData.DataIndex++;
        }

        if (rawData.DataIndex - CurrentMeasurement.WindowLength < 0)
        {
            Debug.WriteLine($"waveIn index:{rawData.DataIndex}");
            Debug.WriteLine("Not enough data for window");
            return;
        }
        Debug.WriteLine($"waveIn index:{rawData.DataIndex}");

        var data = CalculateFrequencySpecturm(CurrentMeasurement, CMIndex);
        var dominantValues = CalculateDominantValues(CurrentMeasurement, CMIndex, data);

        DataAvailable?.Invoke(this, new DataAvaiableArgs(data, CurrentMeasurement.FrequencyDataSize, dominantValues));
    }

    public double[] CalculateFrequencySpecturm(MeasurementGroup measurement, int mIndex)
    {
        var mData = measurement.MeasurementsData[mIndex];
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

        var data = new double[measurement.FrequencyDataSize];
        for (var i = 0; i < measurement.FrequencyDataSize; i++)
        {
            data[i] = Math.Sqrt(fftComplex[i].Real * fftComplex[i].Real + fftComplex[i].Imaginary * fftComplex[i].Imaginary) / 10;
        }

        HighPassFilter(measurement, data, 6);
        LowPassFilter(measurement, data, 130);
        NotchFilter(measurement, data, 50);
        Debug.WriteLine("Data Calculated");

        return data;
    }

    public double[] CalculateDominantValues(MeasurementGroup measurement, int mIndex, double[] data)
    {
        var mData = measurement.MeasurementsData[mIndex];
        var dominatValuesIndex = mData.DominatValuesIndex(measurement.NumberOfSamplesOnWindowShift, measurement.WindowLength);
        mData.DominantValues[dominatValuesIndex] = CalculateMeanValue(data);
        var dominantValues = new double[dominatValuesIndex];
        for (var i = 0; i <= dominatValuesIndex - 1; i++)
        {
            dominantValues[i] = mData.DominantValues[i];
        }
        Debug.WriteLine("Dominat value added on index:" + dominatValuesIndex.ToString());
        return dominantValues;
    }
    public void CalculateSLopeShift(MeasurementGroup measurement, int mIndex)
    {
        var mData = measurement.MeasurementsData[mIndex];
        var dominatValuesLength = mData.DominatValuesIndex(measurement.NumberOfSamplesOnWindowShift, measurement.WindowLength);
        var dominantValues = new double[dominatValuesLength];
        var milsec = new double[dominatValuesLength];
        for (var i = 0; i < dominatValuesLength; i++)
        {
            dominantValues[i] = mData.DominantValues[i];
            milsec[i] = measurement.BufferMilliseconds * i;
        }
        var avrageX = milsec.Average();
        var avrageY = dominantValues.Average();
        double denominator = 0;
        double numerator = 0;
        for (var i = 0; i < milsec.Length; i++)
        {
            numerator += (milsec[i] - avrageX) * (dominantValues[i] - avrageY);

            denominator += Math.Pow((milsec[i] - avrageX), 2);
        }
        var slope = (numerator / denominator);
        mData.StartFrequency = Math.Round(avrageY - slope * avrageX, 4);
        mData.Slope = Math.Round((slope / measurement.BufferMilliseconds) * 1_000_000, 4);

        Debug.WriteLine("startFrequency:" + mData.StartFrequency.ToString());
        Debug.WriteLine("slope:" + mData.Slope.ToString());
        Debug.WriteLine("Size: " + mData.DominantValues.Length.ToString());
    }

    private double CalculateMovingAvrage(double[] data)
    {
        return data.Average();
    }
    private double CalculateMeanValue(double[] data)
    {
        double sum = 0;
        double sumI = 0;
        for (var i = 0; i < CurrentMeasurement.FrequencyDataSize; i++)
        {
            sum += data[i];
            sumI += data[i] * i;
        }
        double mean = (1 / sum) * ((double)CurrentMeasurement.SampleRate / (double)CurrentMeasurement.WindowLength) * sumI;
        return mean;
    }
    private void NotchFilter(MeasurementGroup measurement, double[] data, double frequency)
    {
        var i = (int)Math.Round((double)measurement.WindowLength / (double)measurement.SampleRate * frequency);
        data[i] = 0;
        data[i + 1] = 0;
        data[i - 1] = 0;
    }
    private void LowPassFilter(MeasurementGroup measurement, double[] data, int cornerFrequency)
    {
        var f = cornerFrequency * measurement.WindowLength / measurement.SampleRate;
        for (var i = measurement.FrequencyDataSize - 1; i > f; i--)
        {
            data[i] = 0;
        }
    }
    private void HighPassFilter(MeasurementGroup measurement, double[] data, int cornerFrequency)
    {
        var f = cornerFrequency * measurement.WindowLength / measurement.SampleRate;
        for (var i = 0; i < f; i++)
        {
            data[i] = 0;
        }
    }

    public void CreateConnection(MeasurementGroup measurement)
    {
        CurrentMeasurement = measurement;
        CMIndex = 0;
        Debug.WriteLine($"New wawe, device number: {measurement.DeviceNumber}, sample rate: {measurement.SampleRate}, buffer size: {measurement.BufferMilliseconds} ms, window size: {measurement.WindowLength}");
        Wawe = new()
        {
            DeviceNumber = measurement.DeviceNumber,
            WaveFormat = new WaveFormat(measurement.SampleRate, 16, 1),
            BufferMilliseconds = measurement.BufferMilliseconds
        };
        Wawe.DataAvailable += WaveIn_DataAvailable;
    }
    public void CreateConnection(int measurmentType, int sampleRate, int bufferMilliseconds, int windowSize, bool MeasurmentTimeFixed, int dataSize, int deviceNumber) 
        => CreateConnection(new MeasurementGroup(sampleRate, bufferMilliseconds, windowSize, MeasurmentTimeFixed, dataSize, 0, 0, 0, 0, 0, 0, deviceNumber));
    
    public void SelectMeasuredMuscle(int muscleType, int side)
    {
        var index = CurrentMeasurement.MeasurementsData.FindIndex(m => m.MuscleType == muscleType && m.Side == side);
        if (index == -1)
        {
            index = CurrentMeasurement.MeasurementsData.Count;
            CurrentMeasurement.MeasurementsData.Add(new MeasurementData(muscleType, side, CurrentMeasurement.DataSize, CurrentMeasurement.DominantValuesSize));
        }
        CMIndex = index;
        Debug.WriteLine("new muscle on index:" + index.ToString());
    }

    public void StartRecording()
    {
        if (!IsRecording && Wawe != null)
        {
            Wawe.StartRecording();
            IsRecording = true;
            Debug.WriteLine("Recording start");
        }
    }

    public void StopRecording()
    {
        if (IsRecording && Wawe != null)
        {
            Wawe.StopRecording();
            IsRecording = false;
            Debug.WriteLine("Recording stop");
            CalculateSLopeShift(CurrentMeasurement, CMIndex);
        }
    }

    public string[] GetListOfDevices()
    {
        //var waveInDevices = WaveIn.DeviceCount;
        //var devices = new string[waveInDevices];
        //for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
        //{
        //    devices[waveInDevice] = WaveIn.GetCapabilities(waveInDevice).ProductName.ToString();
        //}
        //return devices;
        var MMDEnumerator = new MMDeviceEnumerator();
        var endpoints = MMDEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        var devices = new string[endpoints.Count];
        for (var device = 0; device < endpoints.Count; device++)
        {
            devices[device] = endpoints[device].FriendlyName;
        }
        return devices;
    }
}
