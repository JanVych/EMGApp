using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGApp.Events;
public class DataAvaiableArgs
{
    public double[]? Data { get; set; }
    public int Size { get; set; } = 0;
    public DataAvaiableArgs(double[]? data, int size)
    {
        Data = data;
        Size = size;
    }
}
