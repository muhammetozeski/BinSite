using Microsoft.JSInterop;

namespace BinSite.Social.Shared;

/// <summary>Centralizes the community API prefix while preserving each independent product layout.</summary>
public abstract class CommunityComponent : ProductComponent
{
    /// <summary>Reads a filtered community response for the current anonymous browser identity.</summary>
    protected Task<T> ReadAsync<T>(string path) => Api.GetAsync<T>("community/"+path);
    /// <summary>Sends a single community mutation without automatic duplication on retry.</summary>
    protected Task<T> WriteAsync<T>(string path,object data) => Api.PostAsync<T>("community/"+path,data);
    /// <summary>Replaces the current independent product share address without a second API read.</summary>
    protected ValueTask ReplaceAsync(string link) => Javascript.InvokeVoidAsync("binSiteSocial.replaceUrl",link);
    /// <summary>Constructs a same-origin link for a known product and random content identity.</summary>
    protected string ContentLink(string slug,string query,string id) => new Uri(new Uri(Navigation.BaseUri),$"{slug}/?{query}={id}").AbsoluteUri;
}
