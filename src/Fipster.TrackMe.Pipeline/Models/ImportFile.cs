namespace Fipster.TrackMe.Pipeline.Models;

public class ImportFile(DirectoryInfo directory, FileInfo file)
{
    public DirectoryInfo Directory => directory;
    public FileInfo File => file;
}
