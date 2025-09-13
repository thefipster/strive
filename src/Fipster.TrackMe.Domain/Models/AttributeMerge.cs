namespace Fipster.TrackMe.Domain.Models;

public class AttributeMerge(DataSources source, Parameters parameter, string value)
{
    public DataSources Source => source;
    public Parameters Parameter => parameter;
    public string Value => value;
}
