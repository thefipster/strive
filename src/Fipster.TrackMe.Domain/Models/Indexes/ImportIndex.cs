namespace Fipster.TrackMe.Domain.Models.Indexes;

public class ImportIndex(string filepath, string hash, IEnumerable<Classification> classifications)
{
    public string Filepath => filepath;
    public string Hash => hash;
    public IEnumerable<Classification> Classifications => classifications;
}
