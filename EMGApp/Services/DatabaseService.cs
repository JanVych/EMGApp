using System.Data;
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
                first_name TEXT,
                last_name TEXT,
                identification_number TEXT,
                age INTEGER,
                gender INTEGER,
                weight INTEGER,
                height INTEGER,
                address TEXT,
                email TEXT,
                phone_number TEXT,
                description TEXT)";
        var measurementTable = @"CREATE TABLE IF NOT EXISTS measurement(
                measurement_id INTEGER PRIMARY KEY AUTOINCREMENT,
                patient_id INTEGER,
                date_time DATETIME,
                sample_rate INTEGER,
                window_shift_milliseconds INTEGER,
                window_size INTEGER,
                measurement_time_fixed BOOLEAN,
                max_data_length INTEGER,
                dominant_frequency_calculation_type INTEGER,
                notch_filter INTEGER,
                low_pass_filter INTEGER,
                high_pass_filter INTEGER,
                corner_frequency INTEGER,
                FOREIGN KEY (patient_id) REFERENCES patient (patient_id))";
        var measurementDataTable = @"CREATE TABLE IF NOT EXISTS measurement_data(
                measurement_data_id INTEGER PRIMARY KEY AUTOINCREMENT,
                measurement_id INTEGER,
                musle_type INTEGER,
                side INTEGER,
                data BLOB,
                max_values BLOB,
                slope REAL,
                shift REAL,
                measurement_type INTEGER,
                force INTEGER,
                FOREIGN KEY (measurement_id) REFERENCES measurement (measurement_id))";
        var groupTable = @"CREATE TABLE IF NOT EXISTS patient_group(
                patient_group_id INTEGER PRIMARY KEY AUTOINCREMENT,
                patient_group_name TEXT,
                patient_id INTEGER)";
        var groupRealationTable = @"CREATE TABLE IF NOT EXISTS patient_group_relation(
                patient_group_id INTEGER,
                patient_id INTEGER,
                FOREIGN KEY (patient_group_id) REFERENCES patient_group (patient_group_id),
                FOREIGN KEY (patient_id) REFERENCES patient (patient_id),
                PRIMARY KEY (patient_group_id, patient_id))";

        ExecuteNonQuery(patienTable);
        ExecuteNonQuery(measurementTable);
        ExecuteNonQuery(measurementDataTable);
        ExecuteNonQuery(groupTable);
        ExecuteNonQuery(groupRealationTable);
    }
    public void InsertPatient(Patient p)
    {
        var command = @"INSERT INTO patient(first_name, last_name, identification_number, age, gender, weight, height, address, email, phone_number, description) 
                VALUES (@first_name, @last_name, @identification_number, @age, @gender, @weight, @height, @address, @email, @phone_number, @description)";
        var parameters = new (string, object)[]
        {
            ("@first_name", p.FirstName), ("@last_name", p.LastName), ("@identification_number", p.IdentificationNumber),
            ("@age", p.Age), ("@gender", p.Gender), ("@weight", p.Weight), ("@height", p.Height), ("@address",p.Address),
            ("@email",p.Email), ("@phone_number", p.PhoneNumber), ("@description", p.Description)
        };
        ExecuteNonQuery(command, parameters);
    }
    public void InsertMeasurement(MeasurementGroup m)
    {

        if (m.PatientId == null || m.MeasurementDateTime == null) { return; }
        var command = @"INSERT INTO measurement(patient_id, date_time, sample_rate, window_shift_milliseconds, window_size, measurement_time_fixed,
                max_data_length, dominant_frequency_calculation_type, notch_filter, low_pass_filter, high_pass_filter, corner_frequency)
                VALUES (@patient_id, @date_time, @sample_rate, @window_shift_milliseconds, @window_size, @measurement_time_fixed, @max_data_length,
                        @dominant_frequency_calculation_type, @notch_filter, @low_pass_filter, @high_pass_filter, @corner_frequency)";
        var parameters = new (string, object)[]
        {
            ("@patient_id", m.PatientId), ("@date_time", m.MeasurementDateTime),("@sample_rate", m.SampleRate), ("@window_shift_milliseconds", m.WindowShiftMilliseconds),
            ("@window_size", m.WindowLength), ("@measurement_time_fixed",m.MeasurementFixedTime), ("@max_data_length", m.DataSize),
            ("@dominant_frequency_calculation_type", m.DominantFrequencyCalculationType), ("@notch_filter",m.NotchFilter),
            ("@low_pass_filter", m.LowPassFilter), ("@high_pass_filter", m.HighPassFilter), ("@corner_frequency", m.CornerFrequency)
        };
        ExecuteNonQuery(command, parameters);
    }
    public void InsertMeasurementData(MeasurementData mData)
    {
        if (mData.MeasurementId == null) { return; }
        var dataBytes = new byte[mData.Data.Length * sizeof(short)];
        Buffer.BlockCopy(mData.Data, 0, dataBytes, 0, dataBytes.Length);

        var dominantValuesBytes = new byte[mData.DominantValues.Length * sizeof(double)];
        Buffer.BlockCopy(mData.DominantValues, 0, dominantValuesBytes, 0, dominantValuesBytes.Length);

        var command = @"INSERT INTO measurement_data(measurement_id, musle_type, side, data, max_values, slope, shift, measurement_type, force)
                VALUES (@measurement_id, @musle_type, @side, @data, @max_values, @slope, @shift, @measurement_type, @force)";
        var parameters = new (string, object)[]
        {
                ("@measurement_id", mData.MeasurementId), ("@musle_type", mData.MuscleType), ("@side", mData.Side),
                ("@data", dataBytes), ("@max_values", dominantValuesBytes),
                ("@slope", mData.Slope), ("@shift", mData.StartFrequency),
                ("@measurement_type", mData.MeasurementType), ("@force",mData.Force)
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
            var r = cmd.ExecuteReader();
            while (r.Read())
            {
                p.Add(new Patient
                (r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt32(4), r.GetInt32(5),
                r.GetInt32(6), r.GetInt32(7), r.GetString(8), r.GetString(9), r.GetString(10), r.GetString(11)));
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
            cmd.CommandText = @"SELECT measurement_id, patient_id, date_time, sample_rate, window_shift_milliseconds,
                            window_size, measurement_time_fixed, max_data_length, dominant_frequency_calculation_type,
                            notch_filter, low_pass_filter, high_pass_filter, corner_frequency
                            FROM measurement";
            var r = cmd.ExecuteReader();
            while (r.Read())
            {
                m.Add(new MeasurementGroup
                (r.GetInt64(0), r.GetInt64(1), r.GetDateTime(2), r.GetInt32(3), r.GetInt32(4),
                r.GetInt32(5), r.GetBoolean(6), r.GetInt32(7), r.GetInt32(8), r.GetInt32(9),
                r.GetInt32(10), r.GetInt32(11), r.GetInt32(12)));
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

    public void UpdatePatient(Patient p)
    {
        var command = @$"UPDATE patient SET first_name = @first_name, last_name = @last_name, 
                        identification_number = @identification_number, age = @age, gender = @gender, 
                        weight = @weight, height = @height, address = @address, email = @email, 
                        phone_number = @phone_number, description = @description
                        WHERE patient_id = {p.PatientId}";
        var parameters = new (string, object)[]
        {
            ("@first_name", p.FirstName), ("@last_name", p.LastName), ("@identification_number", p.IdentificationNumber),
            ("@age", p.Age), ("@gender", p.Gender), ("@weight", p.Weight), ("@height", p.Height), ("@address",p.Address),
            ("@email",p.Email), ("@phone_number", p.PhoneNumber), ("@description", p.Description)
        };
        ExecuteNonQuery(command, parameters);
    }

    public List<MeasurementData> GetMeasurementData(long? measurmentId)
    {
        var mdata = new List<MeasurementData>();
        try
        {
            _database.Open();
            using var cmd = new SQLiteCommand(_database);
            cmd.CommandText = @"SELECT measurement_id, measurement_data_id, musle_type, side, data, max_values, slope,
                            shift, measurement_type, force
                            FROM measurement_data WHERE measurement_id = @measurement_id";
            cmd.Parameters.AddWithValue("@measurement_id", measurmentId);
            var r = cmd.ExecuteReader(CommandBehavior.KeyInfo);
            while (r.Read()) 
            {
                var dataBlob = r.GetBlob(4,true);
                var dataSize = dataBlob.GetCount();
                var dataBytes = new byte[dataSize];
                var data = new short[dataSize / sizeof(short)];
                dataBlob.Read(dataBytes, dataSize, 0);
                Buffer.BlockCopy(dataBytes, 0, data, 0, dataSize);
                dataBlob.Dispose();

                var dominatValuesBlob = r.GetBlob(5, true);
                var dominatValuesSize = dominatValuesBlob.GetCount();
                var dominatValuesBytes = new byte[dominatValuesSize];
                var dominatValues = new double[dominatValuesSize / sizeof(double)];
                dominatValuesBlob.Read(dominatValuesBytes, dominatValuesSize, 0);
                Buffer.BlockCopy(dominatValuesBytes, 0, dominatValues, 0, dominatValuesSize);
                dominatValuesBlob.Dispose();

                mdata.Add(new MeasurementData(
                    r.GetInt64(0), r.GetInt64(1), r.GetInt32(2), r.GetInt32(3), data, dominatValues,
                    r.GetDouble(6), r.GetDouble(7), r.GetInt32(8), r.GetInt32(9)));
            }
        }
        catch (SQLiteException ex)
        {
            Debug.WriteLine(ex.Message); 
        }
        _database.Close();
        return mdata;
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

