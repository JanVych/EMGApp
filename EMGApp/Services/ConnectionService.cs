using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using NAudio.Wave;
using System.Diagnostics;
using EMGApp.Events;
using NAudio.CoreAudioApi;

namespace EMGApp.Services;
public class ConnectionService : IConnectionService
{
    private readonly IDataService IDataService;
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
    public EventHandler<DataAvaiableArgs>? DataAvailable 
    { 
        get; set; 
    }
    public bool IsRecording 
    { 
        get; set; 
    }

    public ConnectionService(IDataService iDataService)
    {
        IDataService = iDataService;
        CurrentMeasurement = new MeasurementGroup(0, 1000, 100, 1024, true, 1_048_576, 0);
        CurrentMeasurement.MeasurementsData.Add(new MeasurementData(0, 0, CurrentMeasurement.MaxDataLength));
        IsRecording = false;
        
    }
    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        for (var i = 0; i < e.Buffer.Length / 2; i++)
        {
            if (CurrentMeasurement.DataIndex > CurrentMeasurement.MaxDataLength)
            {
                Debug.WriteLine("Buffer is full");
                StopRecording();
                return;
            }
            CurrentMeasurement.MeasurementsData[CMIndex].Data[CurrentMeasurement.DataIndex] = BitConverter.ToInt16(e.Buffer, i * 2);
            CurrentMeasurement.DataIndex++;
        }

        if (CurrentMeasurement.DataIndex - CurrentMeasurement.WindowSize < 0)
        {
            Debug.WriteLine("Not enough data for window");
            return;
        }
        Debug.WriteLine($"waveIn index:{CurrentMeasurement.DataIndex}");

        DataAvaiableArgs args = new DataAvaiableArgs(CalculateData(), CurrentMeasurement.MaxFrequencyIndex);
        DataAvailable?.Invoke(this, args);
    }

    private double[] CalculateData()
    {
        var startIndex = CurrentMeasurement.DataIndex - CurrentMeasurement.WindowSize;
        var rawDataIndex = 0;
        var rawData = new double[CurrentMeasurement.WindowSize];

        for (; startIndex < CurrentMeasurement.DataIndex; startIndex++)
        {
            rawData[rawDataIndex] = CurrentMeasurement.MeasurementsData[CMIndex].Data[startIndex];
            rawDataIndex++;
        }

        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(rawData);

        var fftComplex = FftSharp.Transform.FFT(rawData);

        var data = new double[CurrentMeasurement.MaxFrequencyIndex];
        for (var i = 0; i < CurrentMeasurement.MaxFrequencyIndex; i++)
        {
            data[i] = Math.Sqrt(fftComplex[i].Real * fftComplex[i].Real + fftComplex[i].Imaginary * fftComplex[i].Imaginary) / 10;
        }

        HightPassFilter(data, 6);
        NotchFilter(data,50);

        CurrentMeasurement.MeasurementsData[CMIndex].MaxValues.Add(CalculateMeanValue(data));

        Debug.WriteLine("Data Calculated");
        return data;
    }
    public void CalculateSLopeShift()
    {
        double[] milsec = new double[CurrentMeasurement.MeasurementsData[CMIndex].MaxValues.Count];
        List<double> hz = CurrentMeasurement.MeasurementsData[CMIndex].MaxValues;
        for (int i = 0; i < milsec.Length; i++)
        {
            milsec[i] = CurrentMeasurement.BufferMilliseconds * i;
        }
        double avrageX = milsec.Average();
        double avrageY = hz.Average();
        double denominator = 0;
        double numerator = 0;
        for (int i = 0; i < milsec.Length; i++)
        {
            numerator += (milsec[i] - avrageX) * (hz[i] - avrageY);

            denominator += Math.Pow((milsec[i] - avrageX), 2);
        }
        var slope = (numerator / denominator) ;
        CurrentMeasurement.MeasurementsData[CMIndex].Shift = Math.Round(avrageY - slope * avrageX,4);
        CurrentMeasurement.MeasurementsData[CMIndex].Slope = Math.Round((slope / CurrentMeasurement.BufferMilliseconds) * 1_000_000, 4 );

        Debug.WriteLine("shift:" + CurrentMeasurement.MeasurementsData[CMIndex].Shift.ToString());
        Debug.WriteLine("slope:" + CurrentMeasurement.MeasurementsData[CMIndex].Slope.ToString());
        Debug.WriteLine("Size: " + CurrentMeasurement.MeasurementsData[CMIndex].MaxValues.Count.ToString());
    }

    private double CalculateMovingAvrage(double[] data)
    {
        return data.Average();
    }
    private double CalculateMeanValue(double[] data)
    {
        double sum = 0;
        double sumI = 0;
        for (var i = 0; i < CurrentMeasurement.MaxFrequencyIndex; i++)
        {
            sum += data[i];
            sumI += data[i] * i;
        }
        double mean = (1 / sum) * ((double)CurrentMeasurement.SampleRate / (double)CurrentMeasurement.WindowSize) * sumI;
        return mean;
    }
    private void NotchFilter(double[] data, double frequency)
    {
        int i = (int)Math.Round((double)CurrentMeasurement.WindowSize / (double)CurrentMeasurement.SampleRate * frequency);
        data[i ] = 0;
        data[i + 1] = 0;
        data[i - 1] = 0;
    }
    private void HightPassFilter(double[] data, int cornerFrequency)
    {
        var end = cornerFrequency * CurrentMeasurement.WindowSize / CurrentMeasurement.SampleRate;
        for (var i = 0; i < end; i++)
        {
            data[i] = 0;
        }
    }

    public void CreateConnection(int measurmentType, int sampleRate, int bufferMilliseconds, int windowSize, bool MeasurmentTimeFixed, int maxDataLenght, int deviceNumber)
    {
        CurrentMeasurement = new MeasurementGroup(measurmentType, sampleRate, bufferMilliseconds, windowSize, MeasurmentTimeFixed, maxDataLenght, deviceNumber);
        CMIndex = 0;
        Debug.WriteLine($"New wawe, device number: {deviceNumber}, sample rate: {sampleRate}, buffer size: {bufferMilliseconds} ms, window size: {windowSize}");
        Wawe = new()
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(sampleRate, 16, 1),
            BufferMilliseconds = bufferMilliseconds
        };
        Wawe.DataAvailable += WaveIn_DataAvailable;
        DataAvailable?.Invoke(this, new DataAvaiableArgs(null,0));
    }
    public void SelectMeasuredMuscle(int muscleType, int side)
    {
        var index =  CurrentMeasurement.MeasurementsData.FindIndex(m => m.MusleType == muscleType && m.Side == side);
        if (index == -1) 
        {
            index = CurrentMeasurement.MeasurementsData.Count;
            CurrentMeasurement.MeasurementsData.Add(new MeasurementData(muscleType, side, CurrentMeasurement.MaxDataLength));
        }
        CMIndex = index;
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
            CalculateSLopeShift();
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
