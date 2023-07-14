using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGApp.Events;
using EMGApp.Models;
using EMGApp.ViewModels;
using LiveChartsCore.Defaults;

namespace EMGApp.Contracts.Services;
public interface IMeasurementService
{
    public bool IsRecording
    {
        get; set;
    }
    public MeasurementGroup CurrentMeasurement
    {
        get;  set;
    }

    public int CMDataIndex 
    { 
        get; set; 
    }
    public EventHandler<DataAvaiableArgs>? DataAvailable
    {
        get; set;
    }

    public MeasurementGroup CreateDefaultMeasurement();
    public string[] GetListOfDevices();
    public void CreateConnection(MeasurementGroup measurement);
    public void SelectOrAddMuscle(int muscleType, int side, int measurementType, int force);
    public void StartRecording();
    public void StopRecording();
    public double[] CalculateFrequencySpecturm(MeasurementGroup measurement, int mIndex);

    public ObservableCollection<ObservablePoint> GetAvragedDominantValues(MeasurementGroup measurement, int mDataIndex, int size);
}
