using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGApp.Events;
public class DataAvaiableArgs
{
    public double[]? Data 
    { 
        get; set; 
    }
    public double[]? DominantValues
    {
        get; set; 
    }
    public int Size 
    { 
        get; set; 
    }
    public DataAvaiableArgs(double[]? data, int size, double[]? dominantValues)
    {
        Data = data;
        Size = size;
        DominantValues = dominantValues;
    }
}
