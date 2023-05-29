using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.Defaults;

namespace EMGApp.Events;
public class DataAvaiableArgs
{
    public ObservablePoint[] Data 
    { 
        get; set; 
    }
    public ObservablePoint DominantValue
    {
        get; set; 
    }
    public DataAvaiableArgs(ObservablePoint[] data, ObservablePoint dominantValue)
    {
        Data = data;
        DominantValue = dominantValue;
    }
}
