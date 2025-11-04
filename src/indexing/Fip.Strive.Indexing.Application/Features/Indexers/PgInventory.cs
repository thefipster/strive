using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;

namespace Fip.Strive.Indexing.Application.Features.Indexers;

public class PgInventory(IndexPgContext context) : IInventory
{
    public void Upsert(DateEntry entry)
    {
        var existing = context.Inventory.Find(entry.Date);

        if (existing == null)
            context.Inventory.Add(entry);
        else
            context.Entry(existing).CurrentValues.SetValues(entry);

        context.SaveChanges();
    }

    public IEnumerable<int> GetYears() =>
        context.Inventory.Select(entry => entry.Year).Distinct().OrderBy(year => year).ToList();

    public IEnumerable<DateEntry> GetEntries(int year) =>
        context
            .Inventory.Where(entry => entry.Year == year)
            .OrderBy(entry => entry.Timestamp)
            .ToList();

    public IEnumerable<DateEntry> GetEntries(int year, int month) =>
        context
            .Inventory.Where(entry => entry.Year == year && entry.Month == month)
            .OrderBy(entry => entry.Timestamp)
            .ToList();

    public IEnumerable<DateEntry> GetEntries(int year, int month, int day) =>
        context
            .Inventory.Where(entry =>
                entry.Year == year && entry.Month == month && entry.Day == day
            )
            .OrderBy(entry => entry.Timestamp)
            .ToList();

    public IEnumerable<DateEntry> GetEntries(DateTime inclusiveStart, DateTime exclusiveEnd) =>
        context
            .Inventory.Where(entry => entry.Date >= inclusiveStart && entry.Date < exclusiveEnd)
            .OrderBy(entry => entry.Timestamp)
            .ToList();
}
