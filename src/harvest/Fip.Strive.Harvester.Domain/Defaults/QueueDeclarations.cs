namespace Fip.Strive.Harvester.Domain.Defaults;

public static class QueueDeclarations
{
    public static class Upload
    {
        public const string Exchange = "strive.harvest.upload.exchange";
        public const string Queue = "strive.harvest.upload.queue";
        public const string Route = "strive.harvest.upload.route";
    }
}
