using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.Defaults;

namespace EMGApp.Events;
public class DataAvaiableArgs
{
    public ObservablePoint[]? Data 
    { 
        get;
    }
    public ObservablePoint? DominantValue
    {
        get;
    }
    public int DataIndex
    {
    get;
    }
    public DataAvaiableArgs(int dataIndex, ObservablePoint[]? data, ObservablePoint? dominantValue)
    {
        DataIndex = dataIndex;
        Data = data;
        DominantValue = dominantValue;
    }
}
