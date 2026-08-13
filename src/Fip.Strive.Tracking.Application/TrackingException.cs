namespace Fip.Strive.Tracking.Application;

/// <summary>
/// A rule the user broke — a duplicate name, a missing required value. Thrown rather than returned
/// as a result object so a caller cannot forget to check it; the pages catch it and put the message
/// straight into a snackbar.
/// </summary>
public sealed class TrackingException(string message, Exception? innerException = null)
    : Exception(message, innerException);
