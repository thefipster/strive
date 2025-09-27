using System.Threading.Channels;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class Transition<TItem>
{
    public Channel<TItem> Items { get; set; } = Channel.CreateBounded<TItem>(100);
}
