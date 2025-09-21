using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;

namespace TheFipster.ActivityAggregator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController(ILiteDbReader<Inventory, int> reader) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Inventory> Get(int year, int? month, int? day)
    {
        if (month.HasValue && day.HasValue)
        {
            var entry = reader.GetById(year * 10000 + month.Value * 100 + day.Value);
            if (entry != null)
                return [entry];

            return [];
        }

        if (month.HasValue)
            return reader.GetFiltered(x => x.Year == year && x.Month == month.Value);

        return reader.GetFiltered(x => x.Year == year);
    }
}
