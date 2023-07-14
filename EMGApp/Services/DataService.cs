using System.Diagnostics;
using EMGApp.Contracts.Services;
using EMGApp.Events;
using EMGApp.Models;

namespace EMGApp.Services;
public class DataService : IDataService
{
    private readonly IDatabaseService _databaseService;

    public List<Patient> Patients
    {
        get; private set;
    } = new List<Patient>();

    public List<MeasurementGroup> Measurements
    {
        get; private set;
    } = new List<MeasurementGroup>();

    // Patient selected for measuring
    public long? CurrentPatientId
    {
        get; set;
    }
    public Patient? CurrentPatient => Patients.FirstOrDefault(p => p.PatientId == CurrentPatientId);

    // Patient selected for observimg
    public long? ObservedPatientId
    {
        get; set; 
    }
    public Patient? ObservedPatient => Patients.FirstOrDefault(p => p.PatientId == ObservedPatientId);

    // Measurement selected for observimg
    public long? ObservedMeasurementId
    {
        get; set;
    }
    public MeasurementGroup? ObservedMeasurement => Measurements.FirstOrDefault(m => m.MeasurementId == ObservedMeasurementId);

    public int ObservedMeasurementDataIndex
    {
        get; set;
    } = 0;
    public bool ObservedMeasuremntIsRunning
    {
        get; set;
    } = false;

    public EventHandler<ObservedMeasuremntRunStepArgs>? ObservedMeasuremntRunEvent
    {
        get; set;
    }

    public DataService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Patients = _databaseService.GetPatients();
        Measurements = _databaseService.GetMeasurements();
        LoadFirstPatient();
    }

    public void LoadFirstPatient()
    {
        CurrentPatientId ??= Patients.FirstOrDefault()?.PatientId;
    }

    public void AddPatient(Patient patient)
    {
        _databaseService.InsertPatient(patient);
        Patients = _databaseService.GetPatients();
        LoadFirstPatient();
    }

    public void AddMeasurement(MeasurementGroup measurement)
    {
        if (CurrentPatientId == null) { return; }
        if (measurement.PatientId != CurrentPatientId)
        {
            measurement.MeasurementDateTime = DateTime.Now;
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

    public List<MeasurementData> GetMeasurementData(MeasurementGroup measurement)
    {
        if (measurement.MeasurementId != null)
        {
            return _databaseService.GetMeasurementData(measurement.MeasurementId);
        }
        return new List<MeasurementData>();
    }

    public void RemovePatient(Patient patient)
    {
        if (patient.PatientId != null)
        {
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
            if (CurrentPatientId == patient.PatientId)
            {
                CurrentPatientId = null;
                LoadFirstPatient();
            }
            
        }
    }
    public void RemoveMeasurement(MeasurementGroup measurement)
    {
        if (measurement.MeasurementId != null) 
        {
            _databaseService.Delete("measurement_data", "measurement_id", measurement.MeasurementId);
            _databaseService.Delete("measurement", "measurement_id", measurement.MeasurementId);
            Measurements = _databaseService.GetMeasurements();
        }
    }
    public async Task ObservedMeasuremntRunAsync(MeasurementGroup m, int measurementIndex)
    {
        ObservedMeasuremntIsRunning = true;
        var mData = m.MeasurementsData[measurementIndex];
        while(ObservedMeasuremntIsRunning && mData.DataIndex <= m.DataSize - m.NumberOfSamplesOnWindowShift)
        {
            await Task.Delay(m.WindowShiftMilliseconds);
            mData.DataIndex += m.NumberOfSamplesOnWindowShift;
            ObservedMeasuremntRunEvent?.Invoke(this, new ObservedMeasuremntRunStepArgs(mData.DataIndex));
        }
        ObservedMeasuremntIsRunning = false;
    }
}
