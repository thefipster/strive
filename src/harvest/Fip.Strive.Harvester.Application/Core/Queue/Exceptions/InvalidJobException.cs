using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Exceptions;

public class InvalidJobException : Exception
{
    public InvalidJobException(JobEntity job, string message)
        : base($"JobId: {job.Id} | {job.Status} - {message}") { }
}
