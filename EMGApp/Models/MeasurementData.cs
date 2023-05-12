using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FftSharp;

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
    public int DominatValuesIndex(int bufferMilliseconds, int windowSize) => DataIndex / bufferMilliseconds - (int)Math.Ceiling((double)windowSize / (double)bufferMilliseconds);
    public MeasurementData(int musleType, int side, int DataSize, int dominatValuesSize)
    {
        MusleType = musleType;
        Side = side; 
        Data = new short[DataSize];
        DominantValues = new double[dominatValuesSize];
    }
}
