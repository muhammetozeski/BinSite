using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BinSite.Social.Shared;

/// <summary>Provides a final UI action boundary, language updates and manual copy fallback without prescribing a product layout.</summary>
public abstract class ProductComponent : ComponentBase, IDisposable
{
    [Inject] protected UiText Ui { get; set; } = default!;
    [Inject] protected SocialApi Api { get; set; } = default!;
    [Inject] protected IJSRuntime Javascript { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    protected string Status = "";
    protected bool HasError, Busy;
    protected string? CopyFallback;

    /// <summary>Subscribes the displayed product to this browser's language preference.</summary>
    protected override void OnInitialized() => Ui.Changed += RefreshLanguage;

    /// <summary>Selects a translated product interface string.</summary>
    protected string T(string turkish, string english) => Ui.Text(turkish, english);

    /// <summary>Runs a whole user action, retaining failure state and preventing duplicate writes from a busy form.</summary>
    protected async Task ExecuteAsync(Func<Task> operation, string? success = null)
    {
        if (Busy) return;
        Busy = true;
        HasError = false;
        Status = "";
        try { await operation(); if (success is not null) Status = success; }
        catch (ApiFailure error)
        {
            HasError = true;
            Status = Ui.English ? error.Status switch
            {
                HttpStatusCode.NotFound => "This content was not found or has been hidden after reports.",
                HttpStatusCode.Conflict => "The state changed or this action is no longer available. Refresh to inspect the saved state; your draft has been kept.",
                HttpStatusCode.TooManyRequests => "The short-term limit has been reached. Try again later.",
                HttpStatusCode.BadRequest => "Check the text lengths, distinct options and future date, then try again.",
                _ => "The server could not complete this action. Your form text has been kept for a manual retry."
            } : error.Message;
        }
        catch (Exception) { HasError = true; Status = T("Bağlantı kurulamadı veya yanıt okunamadı. Form metnin korundu; yeniden deneyebilirsin.", "The connection or response failed. Your form text has been kept; you can retry."); }
        finally { Busy = false; }
    }

    /// <summary>Copies a public product link, retaining a selectable field when browser clipboard permission is unavailable.</summary>
    protected async Task CopyAsync(string link)
    {
        await ExecuteAsync(async () =>
        {
            var copied = await Javascript.InvokeAsync<bool>("binSiteSocial.copy", link);
            CopyFallback = copied ? null : link;
            Status = copied ? T("Bağlantı kopyalandı.", "Link copied.") : T("Bağlantıyı aşağıdaki alandan kopyalayabilirsin.", "Copy the link from the field below.");
        });
    }

    /// <summary>Formats a server timestamp in the chosen interface language.</summary>
    protected string DateLabel(long milliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToLocalTime().ToString("g", System.Globalization.CultureInfo.GetCultureInfo(Ui.English ? "en-GB" : "tr-TR"));

    /// <summary>Asks Blazor to repaint this component when the scoped language changes.</summary>
    void RefreshLanguage() => _ = InvokeAsync(StateHasChanged);

    /// <summary>Detaches the component from the browser-scoped language service.</summary>
    public void Dispose() { Ui.Changed -= RefreshLanguage; GC.SuppressFinalize(this); }
}
