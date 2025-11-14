namespace Fip.Strive.Harvester.Domain.Defaults;

public abstract class DirectExchange
{
    public abstract string Exchange { get; }
    public abstract string Route { get; }
    public abstract string Queue { get; }
}

public class UploadExchange : DirectExchange
{
    public override string Exchange => "strive.harvest.upload.exchange";
    public override string Queue => "strive.harvest.upload.queue";
    public override string Route => "strive.harvest.upload.route";
}

public class ImportExchange : DirectExchange
{
    public override string Exchange => "strive.harvest.import.exchange";
    public override string Queue => "strive.harvest.import.queue";
    public override string Route => "strive.harvest.import.route";
}
