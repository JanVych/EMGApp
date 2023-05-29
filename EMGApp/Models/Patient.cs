using System.Text;
using EMGApp.Views;
using Windows.ApplicationModel.Payments;

namespace EMGApp.Models;
public class Patient
{
    public long? PatientId 
    { 
        get; set; 
    }
    public string FirstName 
    { 
        get; set; 
    }
    public string LastName 
    {
        get; set;
    }
    public string IdentificationNumber
    {
        get; set;
    }
    public int Age 
    { 
        get; set; 
    }
    public int Gender 
    { 
        get; set; 
    }
    public int Weight 
    { 
        get; set; 
    }
    public int Height 
    { 
        get; set; 
    }
    public string Address
    {
        get; set;
    }
    public string Email
    {
        get; set;
    }
    public string PhoneNumber
    {
        get; set;
    }
    public string Description 
    { 
        get; set; 
    }

    public string GenderString => GenderStrings[Gender];

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
            str.Append(FirstName);
            str.Append(' ');
            str.Append(LastName);
            return str.ToString();
        }
    }
    public Patient(long? id, string firstName,string lastName, string identificationNumber, int age, int gender,
        int weight, int height, string address, string email, string phoneNumber, string destription)
    {
        PatientId = id;
        FirstName = firstName;
        LastName = lastName;
        Age = age;
        Gender = gender;
        Weight = weight;
        Height = height;
        IdentificationNumber = identificationNumber;
        Description = destription;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
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
            "Identification number" => patient.IdentificationNumber,
            _ => string.Empty,
        };
    }
}
