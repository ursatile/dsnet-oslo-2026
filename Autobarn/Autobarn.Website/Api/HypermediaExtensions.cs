using Autobarn.Data.Entities;
using Autobarn.Website.Api.Resources;

namespace Autobarn.Website.Api;


public static class HypermediaExtensions {

	/// <summary>Creates a link to a named API endpoint, honouring the request's PathBase.</summary>
	public static Hyperlink LinkTo(this LinkGenerator links, HttpContext http, string endpointName, object? values = null)
		=> new(links.GetPathByName(http, endpointName, values)
					 ?? throw new InvalidOperationException($"Could not create a link to endpoint '{endpointName}'"));

	public static VehicleMakeResource ToResource(this VehicleMake make, LinkGenerator links, HttpContext http)
		=> new(
			Links: new() {
				["self"] = links.LinkTo(http, Endpoints.GET_MAKE, new { make = make.Code }),
				["models"] = links.LinkTo(http, Endpoints.GET_MODELS_BY_MAKE, new { make = make.Code })
			},
			Code: make.Code,
			Name: make.Name
		);

	public static VehicleModelResource ToResource(this VehicleModel model, LinkGenerator links, HttpContext http)
		=> new(
			Links: new() {
				["self"] = links.LinkTo(http, Endpoints.GET_MODEL, new { make = model.MakeCode, model = model.ModelCode }),
				["make"] = links.LinkTo(http, Endpoints.GET_MAKE, new { make = model.MakeCode }),
				["vehicles"] = links.LinkTo(http, Endpoints.GET_VEHICLES_BY_MODEL, new { make = model.MakeCode, model = model.ModelCode })
			},
			Code: model.Code,
			Name: model.Name
		);

	public static VehicleResource ToResource(this Vehicle vehicle, LinkGenerator links, HttpContext http)
		=> new(
			Links: new() {
				["self"] = links.LinkTo(http, Endpoints.GET_VEHICLE, new { registration = vehicle.Registration }),
				["model"] = links.LinkTo(http, Endpoints.GET_MODEL, new { make = vehicle.Model.MakeCode, model = vehicle.Model.ModelCode }),
				["make"] = links.LinkTo(http, Endpoints.GET_MAKE, new { make = vehicle.Model.MakeCode })
			},
			Registration: vehicle.Registration,
			Year: vehicle.Year,
			Color: vehicle.Color
		);
}
