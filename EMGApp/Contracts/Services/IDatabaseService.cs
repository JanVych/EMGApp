using EMGApp.Models;

namespace EMGApp.Contracts.Services;
public interface IDatabaseService
{
    public void ExecuteNonQuery(string commandText, (string, object)[]? parameters);
    public void ExecuteNonQuery(string commandText);
    public T? ExecuteScalar<T>(string commandText, (string, object)[]? parameters);
    public T? ExecuteScalar<T>(string commandText);
    public void InsertPatient(Patient p);
    public void InsertMeasurement(MeasurementGroup m);
    public void InsertMeasurementData(MeasurementData m);
    public long GetLastInsertedRowId(string tableName);
    public List<Patient> GetPatients();
    public List<MeasurementGroup> GetMeasurements();
    public List<MeasurementData> GetMeasurementData(long? measurmentId);
    public void Delete(string tableName, string columnName, object columnId);
}
