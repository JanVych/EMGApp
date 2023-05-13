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
    public int MusleType
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
    public double StartFrequency
    {
        get; set;
    } = 0;
    //
    public int DataIndex
    {
        get; set;
    } = 0;
    public int DominatValuesIndex(int numberOfSamplesOnWindowShift, int windowSize) => DataIndex / numberOfSamplesOnWindowShift - (int)Math.Ceiling((double)windowSize / (double)numberOfSamplesOnWindowShift);
    public MeasurementData(int musleType, int side, int dataSize, int dominatValuesSize)
    {
        MusleType = musleType;
        Side = side; 
        Data = new short[dataSize];
        DominantValues = new double[dominatValuesSize];
    }
}
