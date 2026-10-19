using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     TLS client certificate used for a given origin.
/// </summary>
public class ClientCertificateSettings
{
    /// <summary>
    ///     Exact origin the certificate is valid for (scheme://host:port).
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    ///     Path to the file with the certificate in PEM format.
    /// </summary>
    public string? CertPath { get; set; }

    /// <summary>
    ///     Certificate in PEM format (base64 in the settings file).
    /// </summary>
    public byte[]? Cert { get; set; }

    /// <summary>
    ///     Path to the file with the private key in PEM format.
    /// </summary>
    public string? KeyPath { get; set; }

    /// <summary>
    ///     Private key in PEM format (base64 in the settings file).
    /// </summary>
    public byte[]? Key { get; set; }

    /// <summary>
    ///     Path to the PFX or PKCS12 encoded private key and certificate chain.
    /// </summary>
    public string? PfxPath { get; set; }

    /// <summary>
    ///     PFX or PKCS12 encoded private key and certificate chain (base64 in the settings file).
    /// </summary>
    public byte[]? Pfx { get; set; }

    /// <summary>
    ///     Passphrase for the private key (PEM or PFX).
    /// </summary>
    public string? Passphrase { get; set; }

    internal ClientCertificate ToClientCertificate() => new()
    {
        Origin = Origin!,
        CertPath = CertPath,
        Cert = Cert,
        KeyPath = KeyPath,
        Key = Key,
        PfxPath = PfxPath,
        Pfx = Pfx,
        Passphrase = Passphrase
    };
}
