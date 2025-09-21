using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;

public class InventoryReader(ActivityContext context) : BaseReader<Inventory, int>(context) { }
