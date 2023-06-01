using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Globalization.NumberFormatting;

namespace EMGApp.Helpers;
public class PatinetPropertyNumberFormater : INumberFormatter2, INumberParser
{
    public string FormatDouble(double value)
    {
        return Math.Floor(value).ToString();
    }
    public string FormatInt(long value) => throw new NotImplementedException();
    public string FormatUInt(ulong value) => throw new NotImplementedException();
    public double? ParseDouble(string text)
    {
        if (double.TryParse(text, out var num))
        {
            return num;
        }
        return null;
    }
    public long? ParseInt(string text) => throw new NotImplementedException();
    public ulong? ParseUInt(string text) => throw new NotImplementedException();
}
