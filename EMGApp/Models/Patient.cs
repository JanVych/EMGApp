using System.Text;
using EMGApp.Views;
using Windows.ApplicationModel.Payments;

namespace EMGApp.Models;
public class Patient
{
    public long? PatientId { get; set; }
    public string Name { get; set; }
    public string Surname {get; set; }
    public int Age { get; set; }
    public int Gender { get; set; }
    public string Address { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public int Condition { get; set; }
    public string Description { get; set; }

    public string ConditionString => ConditionStrings[Condition];
    public string GenderString => GenderStrings[Gender];

    public static readonly Dictionary<int, string> ConditionStrings = new()
    {
       {0, "condition 1"},
       {1, "condition 2"},
       {2, "condition 3"},
       {3, "condition 4"}
    };
    public static readonly Dictionary<int, string> GenderStrings = new()
    {
       {0, "M"},
       {1, "F"},
    };
    public string FullName
    {
        get
        {
            var str = new StringBuilder();
            str.Append(Name);
            str.Append(' ');
            str.Append(Surname);
            return str.ToString();
        }
    }
    public Patient(long? id, string name,string surname, int age, int gender, string address, int weight, int height, int condition, string destription)
    {
        PatientId = id;
        Name = name;
        Surname = surname;
        Age = age;
        Gender = gender;
        Weight = weight;
        Height = height;
        Condition = condition;
        Description = destription;
        Address = address;
    }
    public static string GetStringProperty(Patient patient ,string? propertyName)
    {
        return propertyName switch
        {
            "Name" => patient.FullName,
            "Age" => patient.Age.ToString(),
            "Gender" => patient.GenderString,
            "Address" => patient.Address,
            "Weight" => patient.Weight.ToString(),
            "Height" => patient.Height.ToString(),
            "Condition" => patient.ConditionString,
            _ => string.Empty,
        };
    }
}
