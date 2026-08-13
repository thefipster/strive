using Fip.Strive.Tracking.Application.Features.Access;

namespace Fip.Strive.Tracking.Web.Access;

/// <summary>
/// <c>hash-password [password]</c>. With no argument the password is read from stdin, which keeps
/// it out of shell history and out of the process list:
/// <code>docker run --rm -i strive-tracking hash-password &lt; secret.txt</code>
/// </summary>
public static class PasswordCommand
{
    public static int Run(string[] args)
    {
        var password = args.Length > 1 ? args[1] : ReadFromStandardInput();

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("No password given. Pass one as an argument or on stdin.");
            return 1;
        }

        Console.WriteLine(Pbkdf2Password.Create(password));
        return 0;
    }

    private static string? ReadFromStandardInput()
    {
        if (!Console.IsInputRedirected)
            Console.Error.Write("Password: ");

        // TrimEnd rather than Trim: a trailing newline from a pipe is an artefact, a leading space
        // is somebody's password.
        return Console.In.ReadLine()?.TrimEnd();
    }
}
