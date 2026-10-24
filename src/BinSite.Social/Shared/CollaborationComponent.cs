using System.Text.Json;
using Microsoft.JSInterop;
namespace BinSite.Social.Shared;
/// <summary>Shares transport and revision attachment without defining any product's domain workflow.</summary>
public abstract class CollaborationComponent:ProductComponent
{
 static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web);
 /// <summary>Reads only one product's bounded public directory.</summary>
 protected Task<SpaceList> DirectoryAsync(string kind)=>Api.GetAsync<SpaceList>("collaboration/"+kind);
 /// <summary>Reads a server-filtered typed workspace for the current cookie identity.</summary>
 protected Task<Space<T>> ReadSpaceAsync<T>(string kind,string id)=>Api.GetAsync<Space<T>>($"collaboration/{kind}/{id}");
 /// <summary>Creates a workspace using the product's validated creation contract.</summary>
 protected Task<Space<T>> CreateSpaceAsync<T>(string kind,object data)=>Api.PostAsync<Space<T>>("collaboration/"+kind,data);
 /// <summary>Attaches the expected revision to one domain action; the server owns authorization and transitions.</summary>
 protected Task<Space<T>> ActionAsync<T>(string kind,Space<T> space,string action,object data)
 {
  var fields=JsonSerializer.SerializeToElement(data,JsonOptions).EnumerateObject().ToDictionary(item=>item.Name,item=>item.Value);
  fields["revision"]=JsonSerializer.SerializeToElement(space.Revision);
  return Api.PostAsync<Space<T>>($"collaboration/{kind}/{space.Id}/{action}",fields);
 }
 /// <summary>Builds a product-specific independent content link.</summary>
 protected string SpaceLink(string kind,string id)=>new Uri(new Uri(Navigation.BaseUri),$"{kind}/?alan={id}").AbsoluteUri;
 /// <summary>Updates only browser history after an already completed server request.</summary>
 protected ValueTask RememberAsync(string kind,string id)=>Javascript.InvokeVoidAsync("binSiteSocial.replaceUrl",SpaceLink(kind,id));
 /// <summary>Resolves a public handle while keeping cookie hashes entirely server-side.</summary>
 protected static string Handle(IEnumerable<Participant> members,string? id)=>members.FirstOrDefault(item=>item.Id==id)?.Name??"—";
}
