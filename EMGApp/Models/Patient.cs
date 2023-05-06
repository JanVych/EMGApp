using System.Text;

namespace EMGApp.Models;
public class Patient
{
    public int? PatientId { get; set; }
    public string Name { get; set; }
    public string Surname {get; set; }
    public int Age { get; set; }
    public int Gender { get; set; }
    public string Address { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public string Condition { get; set; }
    public string Description { get; set; }

    public string GenderString
    {
        get
        {
            if (Gender == 0) { return "M"; }
            else { return "F"; }
        }
    }

    public string FullName
    {
        get
        {
            var str = new StringBuilder();
            str.Append(Name);
            str.Append(" ");
            str.Append(Surname);
            return str.ToString();
        }
    }
    public string AllString
    {
        get
        {
            var str = new StringBuilder();
            str.Append("\tName: ");
            str.Append(FullName);
            str.Append("\tAge: ");
            str.Append(Age);
            str.Append("\tGender: ");
            str.Append(GenderString);
            str.Append("\tAddress: ");
            str.Append(Address);
            str.Append("\tWeight: ");
            str.Append(Weight);
            str.Append("\tHeight: ");
            str.Append(Height);
            str.Append("\tCondition: ");
            str.Append(Condition);
            return str.ToString();
        }
    }
    public Patient(int? id, string name,string surname, int age, int gender, string address, int weight, int height, string condition, string destription)
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
}
