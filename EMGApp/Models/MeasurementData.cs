using System.Drawing;
using ABI.Windows.UI;
using Microsoft.UI.Xaml;

namespace EMGApp.Models;
public class MeasurementData
{
    public long? MeasurementDataId
    {
        get; set;
    }
    public long? MeasurementId
    {
        get; set;
    }
    public int MuscleType
    {
        get; set;
    }
    public int Side
    {
        get; set;
    }
    public short[] Data
    {
        get; set;
    }
    public double[] DominantValues
    {
        get; set;
    }
    public double Slope
    {
        get; set;
    } = 0;
    public double Shift
    {
        get; set;
    } = 0;
    //
    public int DataIndex
    {
        get; set;
    } = 0;
    public int DominatValuesIndex(int numberOfSamplesOnWindowShift, int windowLength) => DataIndex / numberOfSamplesOnWindowShift - (int)Math.Ceiling((double)windowLength / (double)numberOfSamplesOnWindowShift);

    public string? MuscleTypeString => MuscleTypeStrings[MuscleType];
    public string? SideString => SideStrings[Side];
    public string? SlopeString => Slope.ToString() + " Hz/s";

    public static readonly Dictionary<int, string> MuscleTypeStrings = new()
    {
       {0, "tibialis anterior"},
       {1, "vastus medialis"},
       {2, "biceps brachii"},
       {3, "triceps brachii"}
    };
    public static readonly Dictionary<int, string> SideStrings = new()
    {
        {0, "right" },
        {1, "left" }
    };
    //
    public bool IsActive = false;
    public MeasurementData(int musleType, int side, int dataSize, int dominatValuesSize)
    {
        MuscleType = musleType;
        Side = side; 
        Data = new short[dataSize];
        DominantValues = new double[dominatValuesSize];
    }
    public MeasurementData(long measurementDataId, long measurementId, int musleType, int side, short[] data, double[] dominatValues, double slope, double startFrequency)
    {
        MeasurementDataId = measurementDataId;
        MeasurementId = measurementId;
        MuscleType = musleType;
        Side = side;
        Data = data;
        DominantValues = dominatValues;
        Slope = slope;
        Shift = startFrequency;
    }
}
