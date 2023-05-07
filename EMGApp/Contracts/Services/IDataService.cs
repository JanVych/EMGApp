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
    public int? CurrentPatientId
    {
        get; set;
    }
    public void AddPatient(Patient patient);
    public void AddMeasurement(MeasurementGroup measurement);
}
