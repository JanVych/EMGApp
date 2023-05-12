using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGApp.Models;
public class MeasurementGroup
{
    public long? MeasurementId
    {
        get; set;
    }
    public long? PatientId
    {
        get; set;
    }
    public DateTime? DateTime
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
    public bool MeasurementFixedTime
    {
        get; set; 
    }
    public int DataSize
    {
        get; set;
    }
    public int MeasurementType
    {
        get; set;
    }
    public int Force
    {
        get; set;
    }
    public int DominantFrequencyCalculationType
    {
        get; set;
    }
    public int NotchFilter
    {
        get; set;
    }
    public int LowPassFilter
    {
        get; set;
    }
    public int HighPassFilter
    {
        get; set;
    }

    //not in DB
    public int MaxFrequencyIndex => (int)Math.Round((double)WindowSize / (double)SampleRate * 120);
    public int MeasuremntMaxTime => DataSize / SampleRate;
    public int DominantValuesSize => DataSize / BufferMilliseconds - (int)Math.Ceiling((double)WindowSize / (double)BufferMilliseconds) + 1;
    public string? DateTimeString => DateTime.ToString();
    public int DeviceNumber
    {
        get; set;
    } = 0;
    public List<MeasurementData> MeasurementsData
    {
        get; set;
    } = new List<MeasurementData>();

    public MeasurementGroup(int sampleRate, int bufferMilliseconds, int windowSize, bool mTFix , int dataSize, int masurementType, int force, int dFCType, int nFilter, int lPFilter, int hPFilter, int deviceNumber)
    {
        MeasurementType = masurementType;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        DataSize = dataSize;
        MeasurementFixedTime = mTFix;
        DeviceNumber = deviceNumber;
        Force = force;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
    }
    public MeasurementGroup(long measurementId, long patientId, DateTime dateTime, int sampleRate, int bufferMilliseconds, int windowSize, bool mTFix, int dataSize, int masurementType, int force, int dFCType, int nFilter, int lPFilter, int hPFilter)
    {
        MeasurementId = measurementId;
        PatientId = patientId;
        MeasurementType = masurementType;
        DateTime = dateTime;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        MeasurementFixedTime = mTFix;
        DataSize = dataSize;
        Force = force;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
    }
}
