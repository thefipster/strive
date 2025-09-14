namespace TheFipster.ActivityAggregator.Pipeline.Extensions;

public class Counter
{
    private int _value;
    public int Value => _value;

    public int Increment() => Interlocked.Increment(ref _value);
}
