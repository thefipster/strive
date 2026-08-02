using Fip.Strive.Harvester.Application.Defaults;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories;
using Fip.Strive.Harvester.Indexing.Collector.Cli.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Indexing.Collector.Cli.Application
{
    public class Service(
        IConnectionMultiplexer redis,
        RedisListAccess redisList,
        ZipRepository zipRepo,
        FileRepository fileRepo,
        SourceInserter sourceRepo,
        ExtractInserter extractRepo,
        DataRepository dataRepo,
        ILogger<Service> logger
    ) : BackgroundService
    {
        private const string ZipList = IndexDeclarations.ZipDirtyListKey;
        private const string FileList = IndexDeclarations.FileDirtyListKey;
        private const string SourceList = IndexDeclarations.SourceDirtyListKey;
        private const string ExtractList = IndexDeclarations.ExtractDirtyListKey;
        private const string DataList = IndexDeclarations.DataDirtyListKey;

        private readonly IDatabase _db = redis.GetDatabase();

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("Index collector started.");

            while (!ct.IsCancellationRequested)
                await TrySyncIndexes(ct);

            logger.LogInformation("Index collector stopped.");
        }

        private async Task TrySyncIndexes(CancellationToken ct)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);

                await SyncIndexes<ZipIndex>(ZipList, zipRepo.BulkInsert, ct);
                await SyncIndexes<FileInstance>(FileList, fileRepo.BulkInsert, ct);
                await SyncIndexes<SourceIndex>(SourceList, sourceRepo.BulkInsert, ct);
                await SyncIndexes<ExtractIndex>(ExtractList, extractRepo.BulkInsert, ct);
                await SyncIndexes<DataIndex>(DataList, dataRepo.BulkInsert, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                logger.LogInformation("Operation cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Batch processing error. Will retry next cycle.");
            }
        }

        private async Task SyncIndexes<TItem>(
            string list,
            Func<CancellationToken, List<TItem>, Task<int>> inserter,
            CancellationToken ct
        )
        {
            var items = await redisList.RightPopAll<TItem>(_db, list);
            var count = await inserter(ct, items);
            logger.LogInformation("Synced {SyncCount} items from {SyncList}.", count, list);
        }
    }
}
