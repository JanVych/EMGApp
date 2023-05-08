using System;
using System.Collections.Generic;
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
    public bool MeasurementTimeFixed
    {
        get; set; 
    }
    public int MaxDataLength
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
    public string? DateTimeString => DateTime.ToString();
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

    public MeasurementGroup(int sampleRate, int bufferMilliseconds, int windowSize, bool mTFix , int maxDataLength, int masurementType, int force, int dFCType, int nFilter, int lPFilter, int hPFilter, int deviceNumber)
    {
        MeasurementType = masurementType;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        MaxDataLength = maxDataLength;
        MeasurementTimeFixed = mTFix;
        DeviceNumber = deviceNumber;
        Force = force;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
    }
    public MeasurementGroup(long measurementId, long patientId, DateTime dateTime, int sampleRate, int bufferMilliseconds, int windowSize, bool mTFix, int maxDataLength, int masurementType, int force, int dFCType, int nFilter, int lPFilter, int hPFilter)
    {
        MeasurementId = measurementId;
        PatientId = patientId;
        MeasurementType = masurementType;
        DateTime = dateTime;
        SampleRate = sampleRate;
        BufferMilliseconds = bufferMilliseconds;
        WindowSize = windowSize;
        MeasurementTimeFixed = mTFix;
        MaxDataLength = maxDataLength;
        Force = force;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
    }
}
