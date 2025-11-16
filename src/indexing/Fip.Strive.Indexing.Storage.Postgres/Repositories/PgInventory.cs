using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Storage.Postgres.Repositories;

public class PgInventory(IndexPgContext context) : IInventory
{
    public async Task UpsertAsync(DateEntry entry)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var existing = await context.Inventory.FirstOrDefaultAsync(e => e.Date == entry.Date);

            if (existing == null)
            {
                await context.Inventory.AddAsync(entry);
            }
            else
            {
                context.Entry(existing).CurrentValues.SetValues(entry);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<int>> GetYearsAsync() =>
        await context
            .Inventory.AsNoTracking()
            .Select(entry => entry.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToListAsync();

    public async Task<IEnumerable<DateEntry>> GetEntriesAsync(int year) =>
        await context
            .Inventory.AsNoTracking()
            .Where(entry => entry.Year == year)
            .OrderBy(entry => entry.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month) =>
        await context
            .Inventory.AsNoTracking()
            .Where(entry => entry.Year == year && entry.Month == month)
            .OrderBy(entry => entry.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<DateEntry>> GetEntriesAsync(int year, int month, int day) =>
        await context
            .Inventory.AsNoTracking()
            .Where(entry => entry.Year == year && entry.Month == month && entry.Day == day)
            .OrderBy(entry => entry.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<DateEntry>> GetEntriesAsync(
        DateTime inclusiveStart,
        DateTime exclusiveEnd
    ) =>
        await context
            .Inventory.AsNoTracking()
            .Where(entry => entry.Date >= inclusiveStart && entry.Date < exclusiveEnd)
            .OrderBy(entry => entry.Timestamp)
            .ToListAsync();
}
