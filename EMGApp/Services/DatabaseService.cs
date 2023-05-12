using System.Data.SQLite;
using System.Diagnostics;
using EMGApp.Contracts.Services;
using EMGApp.Models;
using Windows.Storage;

namespace EMGApp.Services;
public class DatabaseService :IDatabaseService
{
    private readonly SQLiteConnection _database;
    private readonly string _dbPath;
    public DatabaseService()
    {
        _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, @"SQLDatabase.db");
        _database = InitDatabase();

        CreateTables();
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
    public void ExecuteNonQuery(string commandText) => ExecuteNonQuery(commandText, null);
    public void ExecuteNonQuery(string commandText, (string, object)[]? parameters)
    {
        try
        {
            _database.Open();
            var cmd = new SQLiteCommand(commandText, _database);
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
    public T? ExecuteScalar<T>(string commandText) => ExecuteScalar<T>(commandText, null);
    public T? ExecuteScalar<T>(string commandText, (string, object)[]? parameters)
    {
        object? value = null;
        try
        {
            _database.Open();
            var cmd = new SQLiteCommand(commandText, _database);
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                }
            }
            value = cmd.ExecuteScalar();
            
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
        return (T?)value;
    }
    private void CreateTables()
    {
        var patienTable = @"CREATE TABLE IF NOT EXISTS patient(
                patient_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                name TEXT,
                surname TEXT,
                age INTEGER,
                gender INTEGER,
                address TEXT,
                weight INTEGER,
                height INTEGER,
                condition INTEGER,
                description TEXT)";
        var measurementTable = @"CREATE TABLE IF NOT EXISTS measurement(
                measurement_id INTEGER PRIMARY KEY AUTOINCREMENT,
                patient_id INTEGER,
                date_time DATETIME,
                sample_rate INTEGER,
                buffer_in_milliseconds INTEGER,
                window_size INTEGER,
                measurement_time_fixed BOOLEAN,
                max_data_length INTEGER,
                measurement_type INTEGER,
                force INTEGER,
                dominant_frequency_calculation_type INTEGER,
                notch_filter INTEGER,
                low_pass_filter INTEGER,
                high_pass_filter INTEGER,
                FOREIGN KEY (patient_id) REFERENCES patient (patient_id))";
        var measurementDataTable = @"CREATE TABLE IF NOT EXISTS measurement_data(
                measurement_data_id INTEGER PRIMARY KEY AUTOINCREMENT,
                measurement_id INTEGER,
                musle_type INTEGER,
                side INTEGER,
                data BLOB,
                max_values BLOB,
                slope REAL,
                start_frequency REAL,
                FOREIGN KEY (measurement_id) REFERENCES measurement (measurement_id))";
        var groupTable = @"CREATE TABLE IF NOT EXISTS groups(
                group_name TEXT,
                patient_id INTEGER,
                FOREIGN KEY (patient_id) REFERENCES patient (patient_id),
                PRIMARY KEY (group_name, patient_id))";

        ExecuteNonQuery(patienTable);
        ExecuteNonQuery(measurementTable);
        ExecuteNonQuery(measurementDataTable);
        ExecuteNonQuery(groupTable);
    }
    public void InsertPatient(Patient p)
    {
        var command = @"INSERT INTO patient(name, surname, age, gender, address, weight, height, condition, description) 
                VALUES (@name, @surname, @age, @gender, @address, @weight, @height, @condition, @description)";
        var parameters = new (string, object)[]
        {
            ("@name", p.Name), ("@surname", p.Surname), ("@age", p.Age),
            ("@gender", p.Gender), ("@address",p.Address), ("@weight", p.Weight),
            ("@height", p.Height), ("@condition", p.Condition), ("@description", p.Description)
        };
        ExecuteNonQuery(command, parameters);
    }
    public void InsertMeasurement(MeasurementGroup m)
    {

        if (m.PatientId == null || m.DateTime == null) { return; }
        var command = @"INSERT INTO measurement(patient_id, date_time, sample_rate, buffer_in_milliseconds, window_size, measurement_time_fixed,
                max_data_length, measurement_type, force, dominant_frequency_calculation_type, notch_filter, low_pass_filter, high_pass_filter)
                VALUES (@patient_id, @date_time, @sample_rate, @buffer_in_milliseconds, @window_size, @measurement_time_fixed, @max_data_length,
                @measurement_type, @force, @dominant_frequency_calculation_type, @notch_filter, @low_pass_filter, @high_pass_filter)";
        var parameters = new (string, object)[]
        {
            ("@patient_id", m.PatientId), ("@date_time", m.DateTime),("@sample_rate", m.SampleRate), ("@buffer_in_milliseconds", m.BufferMilliseconds),
            ("@window_size", m.WindowSize), ("@measurement_time_fixed",m.MeasurementFixedTime), ("@max_data_length", m.DataSize),
            ("@measurement_type", m.MeasurementType), ("@force",m.Force), ("@dominant_frequency_calculation_type", m.DominantFrequencyCalculationType),
            ("@notch_filter",m.NotchFilter), ("@low_pass_filter", m.LowPassFilter), ("@high_pass_filter", m.HighPassFilter)
        };
        ExecuteNonQuery(command, parameters);
    }
    public void InsertMeasurementData(MeasurementData mData)
    {
        if (mData.MeasurementId == null) { return; }
        var dataBytes = new byte[mData.Data.Length * sizeof(short)];
        Buffer.BlockCopy(mData.Data, 0, dataBytes, 0, dataBytes.Length);

        var maxValuesBytes = new byte[mData.DominantValues.Length * sizeof(double)];
        Buffer.BlockCopy(mData.DominantValues, 0, maxValuesBytes, 0, maxValuesBytes.Length);

        var command = @"INSERT INTO measurement_data(measurement_id, musle_type, side, data, max_values, slope, start_frequency)
                VALUES (@measurement_id, @musle_type, @side, @data, @max_values, @slope, @start_frequency)";
        var parameters = new (string, object)[]
        {
                ("@measurement_id", mData.MeasurementId), ("@musle_type", mData.MusleType), ("@side", mData.Side),
                ("@data", dataBytes), ("@max_values", maxValuesBytes),
                ("@slope", mData.Slope), ("@start_frequency", mData.StartFrequency)
        };
        ExecuteNonQuery(command, parameters);
    }
    public long GetLastInsertedRowId(string tableName)
    {
        var command = "SELECT seq FROM sqlite_sequence WHERE name = @table_name";
        var parameters = new (string, object)[] { ("@table_name", tableName) };
        return ExecuteScalar<long>(command, parameters);
    }
    public List<Patient> GetPatients()
    {
        var p = new List<Patient>();
        try
        {
            _database.Open();
            var cmd = new SQLiteCommand(_database);
            cmd.CommandText = "SELECT * FROM patient";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                p.Add(new Patient
                (reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4),
                reader.GetString(5), reader.GetInt32(6), reader.GetInt32(7), reader.GetInt32(8), reader.GetString(9)));
            }
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
        return p;
    }
    public List<MeasurementGroup> GetMeasurements()
    {
        var m = new List<MeasurementGroup>();
        try
        {
            _database.Open();
            var cmd = new SQLiteCommand(_database);
            cmd.CommandText = @"SELECT measurement_id, patient_id, date_time, sample_rate, buffer_in_milliseconds,
                            window_size, measurement_time_fixed, max_data_length, measurement_type, force,
                            dominant_frequency_calculation_type, notch_filter, low_pass_filter, high_pass_filter 
                            FROM measurement";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                m.Add(new MeasurementGroup
                (reader.GetInt64(0), reader.GetInt64(1), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4),
                reader.GetInt32(5), reader.GetBoolean(6), reader.GetInt32(7), reader.GetInt32(8), reader.GetInt32(9),
                reader.GetInt32(10), reader.GetInt32(11), reader.GetInt32(12), reader.GetInt32(13)));
            }
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        _database.Close();
        return m;
    }

    public void Delete(string tableName, string columnName, object columnId)
    {
        var command = $"DELETE FROM {tableName} WHERE {columnName} = @column_id";
        var parameters = new (string, object)[] 
        { ("@column_id", columnId) };
        ExecuteNonQuery(command, parameters);
    }

    //public short[]? GetMeasurementData(int measurmentId)
    //{
    //    short[]? data = null;
    //    try
    //    {
    //        _database.Open();
    //        using var cmd = new SQLiteCommand(_database);
    //        cmd.CommandText = "SELECT data FROM measurements WHERE measurment_id = @measurment_id";
    //        cmd.Parameters.AddWithValue("@measurment_id", measurmentId);

    //        var dataBytes = (byte[])cmd.ExecuteScalar();
    //        data = new short[dataBytes.Length / 2];
    //        System.Buffer.BlockCopy(dataBytes, 0, data, 0, dataBytes.Length);
    //    }
    //    catch (SQLiteException ex)
    //    {
    //        Debug.WriteLine(ex.Message);
    //    }
    //    _database.Close();
    //    return data;
    //}
}

