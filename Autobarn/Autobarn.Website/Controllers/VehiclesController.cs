using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autobarn.Website.Controllers;

public class VehiclesController(AutobarnDbContext db) : Controller {

	public async Task<IActionResult> Index() {
		var vehicles = await db.Vehicles.ToListAsync();
		return View(vehicles);
	}

	public async Task<IActionResult> Details(string id) {
		var vehicle = await db.Vehicles
			.Include(v => v.Model)
			.ThenInclude(m => m.VehicleMake)
			.FirstOrDefaultAsync(v => v.Registration == id);
		if (vehicle == null) return NotFound();
		return View(vehicle);
	}

	[HttpGet]
	public async Task<IActionResult> Advertise(string id) {
		var carModel = await db.Models
			.Include(m => m.VehicleMake)
			.FirstOrDefaultAsync(m => m.Code == id);
		if (carModel == null) return NotFound();
		var dto = new VehicleDto {
			ModelCode = carModel.Code,
			ModelName = $"{carModel.VehicleMake.Name} {carModel.Name}"
		};
		return View(dto);
	}

	[HttpPost]
	public async Task<IActionResult> Advertise(VehicleDto dto) {
		if (await db.Vehicles.AnyAsync(v => v.Registration == dto.Registration))
			ModelState.AddModelError(nameof(dto.Registration), "That registration is already listed in our database.");

		var carModel = await db.Models.FirstOrDefaultAsync(m => m.Code == dto.ModelCode);

		if (carModel == null)
			ModelState.AddModelError(nameof(dto.ModelCode), $"Sorry, {dto.ModelCode} is not a valid model code.");

		if (!ModelState.IsValid) return View(dto);
		var vehicle = new Vehicle {
			Registration = dto.Registration!,
			Color = dto.Color,
			Model = carModel!,
			Year = dto.Year!.Value
		};
		await db.Vehicles.AddAsync(vehicle);
		await db.SaveChangesAsync();
		return RedirectToAction(nameof(Details), new { id = vehicle.Registration });
	}
}
