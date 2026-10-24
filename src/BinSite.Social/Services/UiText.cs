using Microsoft.JSInterop;

namespace BinSite.Social.Services;

/// <summary>Selects the English product copy and normalizes earlier browser language preferences.</summary>
public sealed class UiText(IJSRuntime javascript, ILogger<UiText> logger)
{
    const bool IsLogEnabled = true;
    bool Initialized;
    public bool English => true;
    public event Action? Changed;

    /// <summary>Returns the English copy while preserving the existing component call contract.</summary>
    public string Text(string turkish, string english) => english;

    /// <summary>Sets the browser document and preference to English without reading an obsolete language choice.</summary>
    public async Task InitializeAsync()
    {
        if (Initialized) return;
        Initialized = true;
        try { await javascript.InvokeVoidAsync("binSiteSocial.setLanguage", "en"); }
        catch (JSException error) { if (IsLogEnabled) logger.LogWarning(error, "English remains active; browser language storage is unavailable."); }
        Changed?.Invoke();
    }

    /// <summary>Preserves compatibility with earlier callers while retaining English-only product copy.</summary>
    public async Task ToggleAsync()
    {
        try { await javascript.InvokeVoidAsync("binSiteSocial.setLanguage", "en"); }
        catch (JSException error) { if (IsLogEnabled) logger.LogWarning(error, "Language changed for this session; browser persistence is unavailable."); }
        Changed?.Invoke();
    }
}
