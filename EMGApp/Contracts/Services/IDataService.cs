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
    public long? ObservedMeasurementId
    {
        get; set;
    }
    public long? ObservedPatientId
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
    public Patient? CurrentPatient
    {
        get;
    }
    public MeasurementGroup? ObservedMeasurement
    {
        get;
    }
    public Patient? ObservedPatient
    {
        get;
    }
    public void AddPatient(Patient patient);
    public void AddMeasurement(MeasurementGroup measurement);
    public void RemovePatient(Patient patient);
    public void RemoveMeasurement(MeasurementGroup measurement);
    public List<MeasurementData> GetMeasurementData(MeasurementGroup measurement);
    Task ObservedMeasuremntRunAsync(MeasurementGroup m, int measurementIndex);
}
