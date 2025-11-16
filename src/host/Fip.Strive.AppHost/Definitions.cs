using Fip.Strive.AppHost.Extensions;

namespace Fip.Strive.AppHost;

public static class Definitions
{
    private const string ZipsPath = @"E:\strive\data\imports\zips";
    private const string FilesPath = @"E:\strive\data\imports\files";

    public static IResourceBuilder<ProjectResource> AddHarvestIndexMigrator(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<PostgresDatabaseResource> database
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Indexing_Migrator>(name)
            .WithReferenceAndWait(database);
    }

    public static IResourceBuilder<ProjectResource> AddHarvestQueueMigrator(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<PostgresDatabaseResource> database
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Queue_Storage_Postgres_Migrator>(name)
            .WithReferenceAndWait(database);
    }

    public static IResourceBuilder<ProjectResource> AddHarvestPipelineMigrator(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<RabbitMQServerResource> rabbit
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Pipeline_Migrator>(name)
            .WithReferenceAndWait(rabbit);
    }

    public static IResourceBuilder<ProjectResource> AddHarvestIndexingSyncer(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<RedisResource> redis,
        IResourceBuilder<PostgresDatabaseResource> database
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Indexing_Sync_Cli>(name)
            .WithReferenceAndWait(redis)
            .WithReferenceAndWait(database);
    }

    public static IResourceBuilder<ProjectResource> AddHarvestPipelineZipGateStage(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<RabbitMQServerResource> rabbit,
        IResourceBuilder<RedisResource> redis,
        IResourceBuilder<ProjectResource> migration
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Pipeline_ZipGate_Cli>(name)
            .WithReferenceAndWait(rabbit)
            .WithReferenceAndWait(redis)
            .WaitForCompletion(migration)
            .WithEnvironment("Worker:Path", ZipsPath)
            .WithEnvironment("Worker:Overwrite", "false");
    }

    public static IResourceBuilder<ProjectResource> AddHarvestPipelineUnzipperStage(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<RabbitMQServerResource> rabbit,
        IResourceBuilder<ProjectResource> migration
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Pipeline_Unzipper_Cli>(name)
            .WithReferenceAndWait(rabbit)
            .WaitForCompletion(migration)
            .WithEnvironment("Worker:Path", FilesPath)
            .WithEnvironment("Worker:Overwrite", "false");
    }

    public static IResourceBuilder<ProjectResource> AddHarvestPipelineScannerStage(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<RabbitMQServerResource> rabbit,
        IResourceBuilder<RedisResource> redis,
        IResourceBuilder<ProjectResource> migration
    )
    {
        return builder
            .AddProject<Projects.Fip_Strive_Harvester_Pipeline_Scanner_Cli>(name)
            .WithReferenceAndWait(rabbit)
            .WithReferenceAndWait(redis)
            .WaitForCompletion(migration)
            .WithEnvironment("Worker:Path", FilesPath);
    }
}
