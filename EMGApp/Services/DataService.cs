using System.Diagnostics;
using EMGApp.Contracts.Services;
using EMGApp.Models;

namespace EMGApp.Services;
public class DataService :IDataService
{
    private readonly IDatabaseService _databaseService;
    public long? CurrentPatientId 
    { 
        get; set; 
    }
    public List<Patient> Patients 
    { 
        get; private set; 
    } = new List<Patient>();

    public List<MeasurementGroup> Measurements
    {
        get; private set; 
    } = new List<MeasurementGroup>();

    public DataService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Patients = _databaseService.GetPatients();
        Measurements = _databaseService.GetMeasurements();
    }

    public void AddPatient(Patient patient)
    {
        _databaseService.InsertPatient(patient);
        Patients = _databaseService.GetPatients();
    }

    public void AddMeasurement(MeasurementGroup measurement)
    {
        if (CurrentPatientId == null) { return; }
        if (measurement.PatientId != CurrentPatientId)
        {
            measurement.DateTime = DateTime.Now;
            measurement.PatientId = CurrentPatientId;
            _databaseService.InsertMeasurement(measurement);

            measurement.MeasurementId = _databaseService.GetLastInsertedRowId("measurement");
        }
        foreach (var m in measurement.MeasurementsData)
        {
            if (m.MeasurementDataId == null)
            {
                m.MeasurementId = measurement.MeasurementId;
                _databaseService.InsertMeasurementData(m);
                m.MeasurementDataId = _databaseService.GetLastInsertedRowId("measurement_data");
            } 
        }
        Measurements = _databaseService.GetMeasurements();
    }

    public void RemovePatient(Patient patient)
    {
        if (patient.PatientId == null) { return; }
        foreach (var m in Measurements.FindAll(p => p.PatientId == patient.PatientId))
        {
            if (m.MeasurementId != null)
            {
                _databaseService.Delete("measurement_data", "measurement_id", m.MeasurementId);
            }
        }
        _databaseService.Delete("measurement", "patient_id", patient.PatientId);
        _databaseService.Delete("patient", "patient_id", patient.PatientId);
        Patients = _databaseService.GetPatients();
        Measurements = _databaseService.GetMeasurements(); 
    }
}
