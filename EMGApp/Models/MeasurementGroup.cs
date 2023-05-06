using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGApp.Models;
public class MeasurementGroup
{
    public int? MeasurementId
    {
        get; set;
    }
    public int? PatientId
    {
        get; set;
    }
    public int MeasurementType
    {
        get; set; 
    }
    public string? DateTime
    {
        get; set;
    }
    public int SampleRate
    {
        get; set;
    }
    public int BufferMilliseconds
    {
        get; set;
    }
    public int WindowSize
    {
        get; set;
    }
    public bool MeasurementTimeFixed
    {
        get; set; 
    }
    public int MaxDataLength
    {
        get; set;
    }

    //not in DB
    public int MaxFrequencyIndex => (int)Math.Round((double)WindowSize / (double)SampleRate * 120);
    public int DataIndex 
    { 
        get; set; 
    } = 0;
    public int DeviceNumber
    {
        get; set;
    } = 0;

    public List<MeasurementData> MeasurementsData
    {
        get; set; 
    } = new List<MeasurementData>();

    public MeasurementGroup(int masurementType, int sampleRate, int bufferMilliseconds, int windowSize, bool measurementTimeFixed , int maxDataLength, int deviceNumber)
    {
        MeasurementType = masurementType;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        MaxDataLength = maxDataLength;
        MeasurementTimeFixed = measurementTimeFixed;
        DeviceNumber = deviceNumber;
    }
    public MeasurementGroup(int measurementId, int patientId, int masurementType, string date_time, int sampleRate, int bufferMilliseconds, int windowSize, bool measurementTimeFixed, int maxDataLength)
    {
        MeasurementId = measurementId;
        PatientId = patientId;
        MeasurementType = masurementType;
        DateTime = date_time;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        MeasurementTimeFixed = measurementTimeFixed;
        MaxDataLength = maxDataLength;
    }
}
