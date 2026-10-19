using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Credentials for HTTP authentication.
/// </summary>
public class HttpCredentialsSettings
{
    /// <summary>
    ///     Username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     Restrain sending the credentials to this origin (scheme://host:port).
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    ///     When to send the Authorization header (Unauthorized, Always).
    /// </summary>
    public HttpCredentialsSend? Send { get; set; }

    internal HttpCredentials ToHttpCredentials() => new()
    {
        Username = Username!, Password = Password!, Origin = Origin, Send = Send
    };
}
