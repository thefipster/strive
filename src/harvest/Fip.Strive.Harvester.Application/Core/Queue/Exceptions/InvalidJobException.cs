using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Exceptions;

public class InvalidJobException : Exception
{
    public InvalidJobException(JobDetails job, string message)
        : base($"JobId: {job.Id} | {job.Status} - {message}") { }
}
