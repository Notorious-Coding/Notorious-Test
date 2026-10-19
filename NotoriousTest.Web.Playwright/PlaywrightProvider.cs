using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Shares a single Playwright instance between browser infrastructures.
///     The instance is created by the first <see cref="AcquireAsync" /> and disposed by the last <see cref="ReleaseAsync" />.
/// </summary>
internal static class PlaywrightProvider
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static IPlaywright? _instance;
    private static int _references;

    /// <summary>
    ///     Number of infrastructures currently using the Playwright instance.
    /// </summary>
    internal static int References => _references;

    /// <summary>
    ///     Get the shared Playwright instance, creating it if needed.
    ///     Every call must be followed by a call to <see cref="ReleaseAsync" />.
    /// </summary>
    /// <returns>Shared Playwright instance.</returns>
    public static async Task<IPlaywright> AcquireAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _instance ??= await Microsoft.Playwright.Playwright.CreateAsync();
            _references++;
            return _instance;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    ///     Release the shared Playwright instance, disposing it when nobody uses it anymore.
    /// </summary>
    public static async Task ReleaseAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_references == 0) return;
            if (--_references > 0) return;

            _instance?.Dispose();
            _instance = null;
        }
        finally
        {
            _lock.Release();
        }
    }
}
