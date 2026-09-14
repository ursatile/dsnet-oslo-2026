using System.ComponentModel.DataAnnotations;
using Autobarn.Data;
using Autobarn.Website.Api.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Net;

namespace Autobarn.Website.Api;

public static class AutobarnApi {

	internal const int DEFAULT_COUNT = 10;
	internal const int MAX_COUNT = 100;

	internal static LinkList Paginate(LinkGenerator links, HttpContext http, string endpointName, object? routeValues,
		int index, int count, int total) {
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
		var result = new LinkList {
			["self"] = Page(index),
			["first"] = Page(0),
			["last"] = Page(Math.Max(0, (total - 1) / count * count))
		};
		if (index + count < total) result["next"] = Page(index + count);
		if (index > 0) result["prev"] = Page(Math.Max(0, index - count));
		return result;

		Hyperlink Page(int pageIndex) => links.LinkTo(http, endpointName,
			new RouteValueDictionary(routeValues) { ["index"] = pageIndex, ["count"] = count });
	}

	public static IEndpointRouteBuilder MapAutobarnApi(this IEndpointRouteBuilder endpoints, string pattern) {

		var api = endpoints.MapGroup(pattern).WithTags("Autobarn");

		api.MapGet("/",
			(LinkGenerator links, HttpContext http) => TypedResults.Ok(new ApiRootResource(new() {
				["self"] = links.LinkTo(http, Endpoints.GET_API_ROOT),
				["vehicles"] = links.LinkTo(http, Endpoints.GET_VEHICLES),
				["makes"] = links.LinkTo(http, Endpoints.GET_MAKES)
			})))
			.WithName(Endpoints.GET_API_ROOT)
			.WithSummary("Autobarn API Discovery Endpoint")
			.WithDescription("Returns a list of links to the other endpoints in the Autobarn API.");

		api.MapGet("/makes",
			async Task<Ok<ResourceList<VehicleMakeResource>>> (AutobarnDbContext db, LinkGenerator links, HttpContext http,
				[Range(0, int.MaxValue)] int index = 0,
				[Range(1, MAX_COUNT)] int count = DEFAULT_COUNT) => {
				var total = await db.Makes.CountAsync(http.RequestAborted);
				var makes = await db.Makes.AsNoTracking()
					.OrderBy(m => m.Code)
					.Skip(index).Take(count)
					.ToListAsync(http.RequestAborted);
				var items = makes.Select(m => m.ToResource(links, http)).ToList();
				var pageLinks = Paginate(links, http, Endpoints.GET_MAKES, null, index, count, total);
				return TypedResults.Ok(new ResourceList<VehicleMakeResource>(pageLinks, index, count, total, items));
			})
			.WithName(Endpoints.GET_MAKES)
			.WithSummary("List vehicle makes")
			.WithDescription("Returns a page of the manufacturers whose vehicles appear in the Autobarn catalogue.")
			.ProducesValidationProblem();

		api.MapGet("/makes/{make}",
			async Task<Results<Ok<VehicleMakeResource>, NotFound>> (AutobarnDbContext db, LinkGenerator links, HttpContext http, string make)
				=> await db.Makes.AsNoTracking()
					.FirstOrDefaultAsync(m => m.Code == make, http.RequestAborted) is { } result
					? TypedResults.Ok(result.ToResource(links, http))
					: TypedResults.NotFound())
			.WithName(Endpoints.GET_MAKE)
			.WithSummary("Find a vehicle make")
			.WithDescription("Returns the manufacturer with the given code, or 404 if no such manufacturer exists.");

		api.MapGet("/makes/{make}/models",
			async Task<Results<Ok<ResourceList<VehicleModelResource>>, NotFound>> (AutobarnDbContext db, LinkGenerator links, HttpContext http,
				string make,
				[Range(0, int.MaxValue)] int index = 0,
				[Range(1, MAX_COUNT)] int count = DEFAULT_COUNT) => {
				if (!await db.Makes.AnyAsync(m => m.Code == make, http.RequestAborted)) return TypedResults.NotFound();
				var query = db.Models.AsNoTracking().Where(m => m.MakeCode == make);
				var total = await query.CountAsync(http.RequestAborted);
				var models = await query
					.OrderBy(m => m.Code)
					.Skip(index).Take(count)
					.ToListAsync(http.RequestAborted);
				var items = models.Select(m => m.ToResource(links, http)).ToList();
				var pageLinks = Paginate(links, http, Endpoints.GET_MODELS_BY_MAKE, new { make }, index, count, total);
				return TypedResults.Ok(new ResourceList<VehicleModelResource>(pageLinks, index, count, total, items));
			})
			.WithName(Endpoints.GET_MODELS_BY_MAKE)
			.WithSummary("List vehicle models by make")
			.WithDescription("Returns a page of the models built by the manufacturer with the given code, or 404 if no such manufacturer exists.")
			.ProducesValidationProblem();

		api.MapGet("/makes/{make}/models/{model}",
			async Task<Results<Ok<VehicleModelResource>, NotFound>> (AutobarnDbContext db, LinkGenerator links, HttpContext http, string make, string model)
				=> await db.Models.AsNoTracking()
					.FirstOrDefaultAsync(m => m.MakeCode == make && m.Code == $"{make}-{model}", http.RequestAborted) is { } result
					? TypedResults.Ok(result.ToResource(links, http))
					: TypedResults.NotFound())
			.WithName(Endpoints.GET_MODEL)
			.WithSummary("Find a vehicle model")
			.WithDescription("Returns the model with the given code built by the manufacturer with the given code, or 404 if no such model exists.");

		api.MapGet("/makes/{make}/models/{model}/vehicles",
			async Task<Results<Ok<ResourceList<VehicleResource>>, NotFound>> (AutobarnDbContext db, LinkGenerator links, HttpContext http,
				string make, string model,
				[Range(0, int.MaxValue)] int index = 0,
				[Range(1, MAX_COUNT)] int count = DEFAULT_COUNT) => {
				var modelCode = $"{make}-{model}";
				if (!await db.Models.AnyAsync(m => m.MakeCode == make && m.Code == modelCode, http.RequestAborted)) return TypedResults.NotFound();
				var query = db.Vehicles.AsNoTracking().Where(v => v.ModelCode == modelCode);
				var total = await query.CountAsync(http.RequestAborted);
				var vehicles = await query
					.Include(v => v.Model)
					.OrderBy(v => v.Registration)
					.Skip(index).Take(count)
					.ToListAsync(http.RequestAborted);
				var items = vehicles.Select(v => v.ToResource(links, http)).ToList();
				var pageLinks = Paginate(links, http, Endpoints.GET_VEHICLES_BY_MODEL, new { make, model }, index, count, total);
				return TypedResults.Ok(new ResourceList<VehicleResource>(pageLinks, index, count, total, items));
			})
			.WithName(Endpoints.GET_VEHICLES_BY_MODEL)
			.WithSummary("List vehicles by model")
			.WithDescription("Returns a page of the vehicles of the given model, or 404 if no such model exists.")
			.ProducesValidationProblem();

		return endpoints;
	}
}
