namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// Tells subscribers that the job table changed, and nothing more. Carrying the changed row would
/// give the UI a second representation of it that can disagree with the table after a dropped or
/// reordered notification; re-reading cannot.
/// </summary>
public interface IJobNotifier
{
    void Notify();

    IDisposable Subscribe(Action handler);
}
