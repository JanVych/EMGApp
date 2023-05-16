namespace EMGApp.Events;
public class ObservedMeasuremntRunStepArgs
{

    public int Value
    {
        get; set;
    }
    public ObservedMeasuremntRunStepArgs(int value)
    {
        Value = value;
    }
}
