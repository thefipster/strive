namespace TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

public interface ILiteDbWriter<TItem>
{
    void Set(TItem item);
}
