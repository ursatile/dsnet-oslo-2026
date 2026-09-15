using System.ComponentModel.DataAnnotations;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Api.Resources;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Media;
using Autobarn.Messages;
using Autobarn.Website.Services;
using EasyNetQ;

namespace Autobarn.Website.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Tags("Autobarn")]
public class VehiclesApiController(
	AutobarnDbContext db,
	LinkGenerator links,
	// IBus bus - don't need this any more!
	OutboxHostedService outbox
	) : ControllerBase {
	private static SoundPlayer player = new SoundPlayer(EmbeddedResource.OpenStream("car_horn.wav"));

	[HttpGet(Name = Endpoints.GET_VEHICLES)]
	[EndpointSummary("List vehicles")]
	[EndpointDescription("Returns a page of the vehicles currently listed for sale at Autobarn.")]
	[ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
	public async Task<Ok<ResourceList<VehicleResource>>> GetVehicles(
		[Range(0, int.MaxValue)] int index = 0,
		[Range(1, AutobarnApi.MAX_COUNT)] int count = AutobarnApi.DEFAULT_COUNT,
		CancellationToken cancellationToken = default) {
		var total = await db.Vehicles.CountAsync(cancellationToken);
		var vehicles = await db.Vehicles.AsNoTracking()
			.Include(v => v.Model)
			.OrderBy(v => v.Registration)
			.Skip(index).Take(count)
			.ToListAsync(cancellationToken);
		var items = vehicles.Select(v => v.ToResource(links, HttpContext)).ToList();
		var pageLinks = AutobarnApi.Paginate(links, HttpContext, Endpoints.GET_VEHICLES, null, index, count, total);
		return TypedResults.Ok(new ResourceList<VehicleResource>(pageLinks, index, count, total, items));
	}

	[HttpGet("{registration}", Name = Endpoints.GET_VEHICLE)]
	[EndpointSummary("Find a vehicle")]
	[EndpointDescription("Returns the vehicle with the given registration plate, or 404 if no such vehicle is listed.")]
	public async Task<Results<Ok<VehicleResource>, NotFound>> GetVehicle(string registration, CancellationToken cancellationToken)
		=> await db.Vehicles.AsNoTracking()
			.Include(v => v.Model)
			.FirstOrDefaultAsync(v => v.Registration == registration, cancellationToken) is { } vehicle
			? TypedResults.Ok(vehicle.ToResource(links, HttpContext))
			: TypedResults.NotFound();

	[HttpPost]
	[EndpointSummary("Create a new vehicle")]
	[EndpointName(Endpoints.POST_VEHICLE)]
	[ProducesResponseType<Created<VehicleResource>>(StatusCodes.Status201Created, "application/hal+json")]
	[ProducesResponseType<Conflict<string>>(StatusCodes.Status409Conflict)]
	public async Task<Results<Created<VehicleResource>, BadRequest<string>, Conflict<string>>>
		Post([FromBody] VehicleDto dto, CancellationToken cancellationToken) {
		var model = await db.Models
			.Include(m => m.VehicleMake)
			.FirstOrDefaultAsync(m => m.Code == $"{dto.ModelCode}", cancellationToken);
		if (model is null) return TypedResults.BadRequest($"Invalid model code {dto.ModelCode}");
		if (await db.Vehicles.AnyAsync(v => v.Registration == dto.Registration, cancellationToken: cancellationToken))
			return TypedResults.Conflict<string>($"There is already a vehicle with registration {dto.Registration} in our database. Sorry.");
		var vehicle = new Vehicle {
			Registration = dto.Registration!,
			Year = dto.Year!.Value,
			Color = dto.Color,
			Model = model
		};

		var message = new NewVehicleMessage {
			Color = vehicle.Color,
			Year = vehicle.Year,
			Registration = vehicle.Registration,
			Model = vehicle.Model.Name,
			Make = vehicle.Model.VehicleMake.Name,
			CreatedAt = DateTimeOffset.UtcNow
		};

		db.OutboxMessages.Add(new OutboxMessage(message));
		// Power failure here? It's fine!
		db.Vehicles.Add(vehicle);
		await db.SaveChangesAsync(cancellationToken);
		outbox.WakeUpAndDoStuff();

		var createdVehicleResource = vehicle.ToResource(links, HttpContext);
		// player.Play();
		return TypedResults.Created(createdVehicleResource.Links["self"].Href, createdVehicleResource);
	}
}
