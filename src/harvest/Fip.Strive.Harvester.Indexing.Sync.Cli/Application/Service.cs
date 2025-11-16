using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Indexing.Sync.Cli.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Indexing.Sync.Cli.Application
{
    public class Service(
        IConnectionMultiplexer redis,
        RedisListAccess redisList,
        ZipInserter zipRepo,
        FileInserter fileRepo,
        ILogger<Service> logger
    ) : BackgroundService
    {
        private const string ZipList = IndexDeclarations.ZipDirtyListKey;
        private const string FileList = IndexDeclarations.FileDirtyListKey;

        private readonly IDatabase _db = redis.GetDatabase();

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("IndexReceiverService started.");

            while (!ct.IsCancellationRequested)
                await TrySyncIndexes(ct);

            logger.LogInformation("IndexReceiverService stopped.");
        }

        private async Task TrySyncIndexes(CancellationToken ct)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);

                await SyncZips(ct);
                await SyncFiles(ct);
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

        private async Task SyncZips(CancellationToken ct)
        {
            var zips = await redisList.RightPopAll<ZipIndex>(_db, ZipList);
            var zipCount = await zipRepo.InsertZipsToPostgres(ct, zips);
            logger.LogInformation("Added {NewZipCount} Zips.", zipCount);
        }

        private async Task SyncFiles(CancellationToken ct)
        {
            var files = await redisList.RightPopAll<FileInstance>(_db, FileList);
            var fileCount = await fileRepo.InsertFilesToPostgres(ct, files);
            logger.LogInformation("Added {NewFileCount} Files.", fileCount);
        }
    }
}
