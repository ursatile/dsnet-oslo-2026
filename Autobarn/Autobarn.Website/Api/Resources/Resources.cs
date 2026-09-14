using System.Text.Json.Serialization;

namespace Autobarn.Website.Api.Resources;

///<summary>A hypermedia link to a related resource.</summary>
///<param name="Href">The URL of the linked resource.</param>
public record Hyperlink(string Href);

///<summary>A set of hypermedia links, keyed by link relation, e.g. "self" or "next".</summary>
public class LinkList : Dictionary<string, Hyperlink>;

public abstract record Resource(
	[property: JsonPropertyName("_links"), JsonPropertyOrder(-1)]
	LinkList Links);

///<summary>The entry point of the Autobarn API, containing links to the top-level collections.</summary>
public record ApiRootResource(LinkList Links) : Resource(Links);

///<summary>A manufacturer who builds vehicles, e.g. Nissan.</summary>
///<param name="Links">Links to this make and its models.</param>
///<param name="Code">The code identifying this manufacturer, e.g. "nissan"</param>
///<param name="Name">The display name of this manufacturer, e.g. "Nissan"</param>
public record VehicleMakeResource(LinkList Links, string Code, string Name) : Resource(Links);

///<summary>A model of vehicle built by a particular manufacturer, e.g. the Nissan Note.</summary>
///<param name="Links">Links to this model, its make, and the vehicles of this model.</param>
///<param name="Code">The code identifying this model, e.g. "nissan-note"</param>
///<param name="Name">The display name of this model, e.g. "Note"</param>
public record VehicleModelResource(LinkList Links, string Code, string Name) : Resource(Links);

///<summary>A specific vehicle which is listed for sale at Autobarn.</summary>
///<param name="Links">Links to this vehicle, its model and its make.</param>
///<param name="Registration">The registration plate which identifies this vehicle, e.g. "OUTATIME"</param>
///<param name="Year">The year this vehicle was manufactured, e.g. 2007</param>
///<param name="Color">The colour of this vehicle, e.g. "Turquoise"</param>
public record VehicleResource(LinkList Links, string Registration, int Year, string Color) : Resource(Links);

///<summary>One page of a paginated collection of resources.</summary>
///<param name="Links">Pagination links: self, first, last, and (where applicable) next and prev.</param>
///<param name="Index">The zero-based index of the first item on this page.</param>
///<param name="Count">The maximum number of items per page.</param>
///<param name="Total">The total number of items in the collection.</param>
///<param name="Items">The items on this page.</param>
public record ResourceList<T>(LinkList Links, int Index, int Count, int Total, IReadOnlyList<T> Items) : Resource(Links);
