using System.Data.SQLite;
using System.Diagnostics;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using Windows.Storage;

namespace EMGApp.Services;
public class DataService :IDataService
{
    private readonly ILocalSettingsService _localSettingsService;
    private readonly string _dbPath;
    private readonly SQLiteConnection _database;

    public int? CurrentPatientId 
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

    public DataService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        _dbPath  = Path.Combine(ApplicationData.Current.LocalFolder.Path, @"SQLDatabase.db");
        _database = InitDatabase();
        CreateTables();

        LoadPatients();
        LoadMeasurments();
    }
    public void DatabaseExecuteNonQuery (string commandText, (string,object)[]? parameters)
    {
        try
        {
            _database.Open();
            using var cmd = new SQLiteCommand(commandText, _database);
            if (parameters != null) 
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                }
            }
            cmd.ExecuteNonQuery();
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
    }
    private SQLiteConnection InitDatabase()
    {
        try
        {
            if (!(File.Exists(_dbPath)))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }
            var database = new SQLiteConnection($"Data Source={_dbPath}; foreign keys=true");
            return database;
        }
        catch (SQLiteException ex) 
        {
            Debug.WriteLine(ex.Message);
        }
        return null;
    }
    private void CreateTables()
    {
        var patienTable = @"CREATE TABLE IF NOT EXISTS patients(
                patient_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                name TEXT,
                surname TEXT,
                age INTEGER,
                gender INTEGER,
                address TEXT,
                weight INTEGER,
                height INTEGER,
                condition TEXT,
                description TEXT)";
        var measurementTable = @"CREATE TABLE IF NOT EXISTS measurements(
                measurement_id INTEGER PRIMARY KEY AUTOINCREMENT,
                patient_id INTEGER,
                measurement_type INTEGER,
                date_time TEXT,
                sample_rate INTEGER,
                buffer_in_milliseconds INTEGER,
                window_size INTEGER,
                measurement_time_fixed,
                max_data_length INTEGER,
                FOREIGN KEY (patient_id) REFERENCES patients (patient_id))";
        var measurementDataTable = @"CREATE TABLE IF NOT EXISTS measurement_data(
                measurement_data_id INTEGER PRIMARY KEY AUTOINCREMENT,
                measurement_id INTEGER,
                musle_type INTEGER,
                side INTEGER,
                data BLOB,
                max_values BLOB,
                slope REAL,
                shift REAL,
                FOREIGN KEY (measurement_id) REFERENCES measurements (measurement_id))";
        var groupTable = @"CREATE TABLE IF NOT EXISTS groups(
                group_name TEXT,
                patient_id INTEGER,
                FOREIGN KEY (patient_id) REFERENCES patients (patient_id),
                PRIMARY KEY (group_name, patient_id))";

        DatabaseExecuteNonQuery(patienTable, null);
        DatabaseExecuteNonQuery(measurementTable, null);
        DatabaseExecuteNonQuery(measurementDataTable, null);
        DatabaseExecuteNonQuery(groupTable, null);
    }

    public void AddPatient(Patient patient)
    {
        var command = @"INSERT INTO patients(name, surname, age, gender, address, weight, height, condition, description) 
                VALUES (@name, @surname, @age, @gender, @address, @weight, @height, @condition, @description)";
        var parameters = new (string, object)[]
        {
            ("@name", patient.Name), ("@surname", patient.Surname), ("@age", patient.Age),
            ("@gender", patient.Gender), ("@address",patient.Address), ("@weight", patient.Weight),
            ("@height", patient.Height), ("@condition", patient.Condition), ("@description", patient.Description)
        };

        DatabaseExecuteNonQuery(command, parameters);
        LoadPatients();
    }
    public void AddMeasurement(MeasurementGroup measurement)
    {
        if (CurrentPatientId == null) { return; }
        var dateTime = DateTime.Now.ToString();
        var command = @"INSERT INTO measurements(patient_id, measurement_type, date_time, sample_rate, buffer_in_milliseconds, window_size, measurement_time_fixed, max_data_length)
                VALUES (@patient_id, @measurement_type, @date_time, @sample_rate, @buffer_in_milliseconds, @window_size, @measurement_time_fixed, @max_data_length)";
        var parameters = new (string, object)[]
        {
            ("@patient_id", CurrentPatientId), ("@measurement_type", measurement.MeasurementType), ("@date_time", dateTime), ("@sample_rate", measurement.SampleRate),
            ("@buffer_in_milliseconds", measurement.BufferMilliseconds), ("@window_size", measurement.WindowSize),
            ("@measurement_time_fixed",measurement.MeasurementTimeFixed), ("@max_data_length", measurement.MaxDataLength)
        };
        DatabaseExecuteNonQuery(command, parameters);


        var cmd = new SQLiteCommand("SELECT seq FROM sqlite_sequence WHERE name = @measurements", _database);
        cmd.Parameters.AddWithValue("@measurements", "measurements");

        _database.Open();
        var lastInsertedRowId = (long)cmd.ExecuteScalar();
        _database.Close();
        measurement.MeasurementId = (int)lastInsertedRowId;
        Debug.WriteLine(lastInsertedRowId.ToString());

        AddMeasurmentData(measurement);
        LoadMeasurments();
    }
    public void AddMeasurmentData(MeasurementGroup measurement)
    {
        if (measurement.MeasurementId == null) { return; }
        foreach (var mData in measurement.MeasurementsData)
        {
            var dataBytes = new byte[mData.Data.Length * sizeof(short)];
            Buffer.BlockCopy(mData.Data, 0, dataBytes, 0, dataBytes.Length);

            var maxValuesBytes = new byte[mData.MaxValues.Count * sizeof(double)];
            Buffer.BlockCopy(mData.MaxValues.ToArray(), 0, maxValuesBytes, 0, maxValuesBytes.Length);

            var command = @"INSERT INTO measurement_data(measurement_id, musle_type, side, data, max_values, slope, shift)
                VALUES (@measurement_id, @musle_type, @side, @data, @max_values, @slope, @shift)";
            var parameters = new (string, object)[]
            {
                ("@measurement_id", measurement.MeasurementId), ("@musle_type", mData.MusleType), ("@side", mData.Side),
                ("@data", dataBytes), ("@max_values", maxValuesBytes),
                ("@slope", mData.Slope), ("@shift", mData.Shift)
            };
            DatabaseExecuteNonQuery(command, parameters);
        }
    }
    public void LoadPatients()
    {
        Patients.Clear();
        try
        {
            _database.Open();
            using var cmd = new SQLiteCommand(_database);
            cmd.CommandText = "SELECT * FROM patients";
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Patients.Add(new Patient
                (reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4),
                reader.GetString(5), reader.GetInt32(6), reader.GetInt32(7), reader.GetString(8), reader.GetString(9)));
            }

        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
    }

    public void LoadMeasurments()
    {
        Measurements.Clear();
        try
        {
            _database.Open();
            using var cmd = new SQLiteCommand(_database);
            cmd.CommandText = @"SELECT measurement_id, patient_id, measurement_type, date_time, sample_rate, buffer_in_milliseconds,
                            window_size, measurement_time_fixed, max_data_length FROM measurements";
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Measurements.Add(new MeasurementGroup
                (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3), reader.GetInt32(4),
                reader.GetInt32(5), reader.GetInt32(6), reader.GetBoolean(7), reader.GetInt32(8)));
            }
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
    }
   
    public void DeletePatient(int patientId)
    {
        var commandP = @"DELETE * FROM patients WHERE patient_id = @patient_id ";
        var commandM = @"DELETE * FROM measurements WHERE patient_id = @patient_id ";
        var parameters = new (string, object)[] { ("@patient_id", patientId) };

        DatabaseExecuteNonQuery(commandP, parameters);
        DatabaseExecuteNonQuery(commandM, parameters);
        LoadPatients();
        LoadMeasurments();
    }
    public void DeleteMeasurment(int measurementId)
    {
        var command = @"DELETE * FROM measurements WHERE measurement_id = @measurement_id ";
        var parameters = new (string, object)[] { ("@measurement_id", measurementId) };
        DatabaseExecuteNonQuery(command, parameters);
        LoadMeasurments();
    }
    public short[]? GetMeasurementData(int measurmentId)
    {
        short[]? data = null;
        try
        {
            _database.Open();
            using var cmd = new SQLiteCommand(_database);
            cmd.CommandText = "SELECT data FROM measurements WHERE measurment_id = @measurment_id";
            cmd.Parameters.AddWithValue("@measurment_id", measurmentId);

            var dataBytes = (byte[])cmd.ExecuteScalar();
            data = new short[dataBytes.Length / 2];
            System.Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
        return data;
    }
}
