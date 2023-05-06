using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FftSharp;

namespace EMGApp.Models;
public class MeasurementData
{
    public int? MeasurementDataId
    {
        get; set; 
    }
    public int? MeasurementId
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
    public List<double> MaxValues
    {
        get; set;
    } = new List<double>();
    public double Slope
    {
        get; set;
    } = 0;
    public double Shift
    {
        get; set;
    } = 0;

    public MeasurementData(int musleType, int side, int maxDataLength)
    {
        MusleType = musleType;
        Side = side; 
        Data = new short[maxDataLength];
    }
}
