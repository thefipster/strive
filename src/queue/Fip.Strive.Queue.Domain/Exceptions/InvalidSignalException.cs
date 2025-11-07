using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Domain.Exceptions;

public class InvalidSignalException : Exception
{
    public InvalidSignalException(JobDetails job, string message)
        : base($"JobId: {job.Id} | {job.Status} - {message}") { }
}
