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
    public DateTime? MeasurementDateTime
    {
        get; set;
    }
    public int SampleRate
    {
        get; set;
    }
    public int WindowShiftMilliseconds
    {
        get; set;
    }
    public int WindowLength
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
    public int CornerFrequency
    {
        get; set;
    } = 200;

    //not in DB
    //
    public double MovingAvrageWindowTimeSeconds
    {
        get;
    } = 2;
    //
    public double SpectralResolution => SampleRate / (double)WindowLength;
    public double WindowShiftSeconds => (double)WindowShiftMilliseconds / 1000;
    public int FrequencyDataSize => (int)Math.Round(CornerFrequency / SpectralResolution);
    public int MeasuremntMaxTime => DataSize / SampleRate;
    public int NumberOfSamplesOnWindowShift => WindowShiftMilliseconds * SampleRate / 1000;
    public int DominantValuesSize => DataSize / NumberOfSamplesOnWindowShift - (int)Math.Ceiling(WindowLength / (double)NumberOfSamplesOnWindowShift) + 1;
    public string? MeasurementDateTimeString => MeasurementDateTime?.ToString("MM/dd/yyyy HH:mm");
    public string? MeasurementDateString => MeasurementDateTime?.ToString("dddd MM/dd/yyyy");
    public string? MeasurementDayTimeString => MeasurementDateTime?.ToString("HH:mm");
    public string? DominantFrequencyCalculationTypeString => DominantFrequencyCalculationTypeStrings[DominantFrequencyCalculationType];
    public int DeviceNumber
    {
        get; set;
    } = 0;
    public List<MeasurementData> MeasurementsData
    {
        get; set;
    } = new List<MeasurementData>();
    
    public static readonly Dictionary<int, string> DominantFrequencyCalculationTypeStrings = new()
    {
       {0, "Median"},
       {1, "Mean value"}
    };
    public MeasurementGroup(int sampleRate, int windowShiftMilliseconds, int windowSize, bool mTFix , int dataSize, int dFCType,
        int nFilter, int lPFilter, int hPFilter, int cornerFrequency, int deviceNumber)
    {
        SampleRate = sampleRate;
        WindowShiftMilliseconds = windowShiftMilliseconds;
        WindowLength = windowSize;
        DataSize = dataSize;
        MeasurementFixedTime = mTFix;
        DeviceNumber = deviceNumber;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
        CornerFrequency = cornerFrequency;
    }
    public MeasurementGroup(long measurementId, long patientId, DateTime dateTime, int sampleRate, int bufferMilliseconds,
        int windowSize, bool mTFix, int dataSize,int dFCType, int nFilter, int lPFilter, int hPFilter, int cornerFrequency)
    {
        MeasurementId = measurementId;
        PatientId = patientId;
        MeasurementDateTime = dateTime;
        SampleRate = sampleRate;
        WindowShiftMilliseconds = bufferMilliseconds;
        WindowLength = windowSize;
        MeasurementFixedTime = mTFix;
        DataSize = dataSize;
        DominantFrequencyCalculationType = dFCType;
        NotchFilter = nFilter;
        LowPassFilter = lPFilter;
        HighPassFilter = hPFilter;
        CornerFrequency = cornerFrequency;
    }
}
