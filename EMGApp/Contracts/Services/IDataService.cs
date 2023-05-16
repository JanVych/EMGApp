using EMGApp.Events;
using EMGApp.Models;

namespace EMGApp.Contracts.Services;
public interface IDataService
{
    public List<Patient> Patients
    {
        get; 
    }
    public List<MeasurementGroup> Measurements
    {
        get; 
    }
    public long? CurrentPatientId
    {
        get; set;
    }
    public long? ObservedMeasuremntId
    {
        get; set;
    }
    public EventHandler<ObservedMeasuremntRunStepArgs>? ObservedMeasuremntRunEvent
    {
        get; set;
    }
    public bool ObservedMeasuremntIsRunning
    {
        get; set;
    }
    public int ObservedMeasurementDataIndex
    {
        get; set;
    }
    public void AddPatient(Patient patient);
    public void AddMeasurement(MeasurementGroup measurement);
    public void RemovePatient(Patient patient);
    public List<MeasurementData> GetMeasurementData(MeasurementGroup measurement);
    Task ObservedMeasuremntRunAsync(MeasurementGroup m, int measurementIndex);
}
