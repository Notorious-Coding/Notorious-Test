namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Browser type.
/// </summary>
/// <param name="_value"></param>
public record BrowserType(string _value)
{
    /// <summary>
    ///     Chromium browser.
    /// </summary>
    public static BrowserType Chromium => new("chromium");

    /// <summary>
    ///     Firefox browser.
    /// </summary>
    public static BrowserType Firefox => new("firefox");

    /// <summary>
    ///     Webkit browser.
    /// </summary>
    public static BrowserType Webkit => new("webkit");

    private readonly string _value = _value;

    /// <summary>
    ///     Implicit conversion to string.
    /// </summary>
    /// <param name="browserType">Type</param>
    /// <returns>Type as string.</returns>
    public static implicit operator string(BrowserType browserType)
    {
        return browserType!._value;
    }

    /// <summary>
    ///     Implicit conversion to string.
    /// </summary>
    /// <param name="browserType">Type</param>
    /// <returns>Type as string.</returns>
    public static implicit operator BrowserType(string browserType)
    {
        return new BrowserType(browserType);
    }


    /// <inheritdoc />
    public override string ToString() => _value;
}
