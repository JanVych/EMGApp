using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGApp.Events;
using EMGApp.Models;

namespace EMGApp.Contracts.Services;
public interface IConnectionService
{
    public MeasurementGroup CurrentMeasurement
    {
        get;  set;
    }

    public int CMIndex 
    { 
        get; set; 
    }
    public EventHandler<DataAvaiableArgs>? DataAvailable
    {
        get; set;
    }
    //public Action? DevicesPropertyChanged
    //{
    //    get; set; 
    //}
    public string[] GetListOfDevices();
    public void CreateConnection(int measurmentType, int sampleRate, int bufferMilliseconds, int windowSize, bool MeasurmentTimeFixed, int maxDataLenght, int deviceNumber);
    public void SelectMeasuredMuscle(int muscleType, int side);
    public void StartRecording();
    public void StopRecording();
}
