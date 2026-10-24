using Microsoft.JSInterop;

namespace BinSite.Social.Services;

/// <summary>Stores the browser language preference and selects complete Turkish or English interface text.</summary>
public sealed class UiText(IJSRuntime javascript, ILogger<UiText> logger)
{
    const bool IsLogEnabled = true;
    bool Initialized;
    public bool English { get; private set; }
    public event Action? Changed;

    /// <summary>Chooses a complete interface string for the currently selected language.</summary>
    public string Text(string turkish, string english) => English ? english : turkish;

    /// <summary>Loads an optional language preference; unavailable browser storage retains Turkish.</summary>
    public async Task InitializeAsync()
    {
        if (Initialized) return;
        Initialized = true;
        try { English = await javascript.InvokeAsync<string?>("binSiteSocial.getLanguage") == "en"; }
        catch (JSException error) { if (IsLogEnabled) logger.LogWarning(error, "Browser language storage could not be read; retaining the default."); }
        Changed?.Invoke();
    }

    /// <summary>Changes this browser's language and attempts to persist only that preference.</summary>
    public async Task ToggleAsync()
    {
        English = !English;
        try { await javascript.InvokeVoidAsync("binSiteSocial.setLanguage", English ? "en" : "tr"); }
        catch (JSException error) { if (IsLogEnabled) logger.LogWarning(error, "Language changed for this session; browser persistence is unavailable."); }
        Changed?.Invoke();
    }
}
