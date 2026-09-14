using Autobarn.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autobarn.Website.Controllers;

public class CarModelsController(AutobarnDbContext db) : Controller {
	public async Task<IActionResult> Index() {
		var list = await db.Models
			.Include(m => m.VehicleMake)
			.ToListAsync();
		return View(list);
	}

	public async Task<IActionResult> Details(string id) {
		var carModel = await db.Models
			.Include(m => m.VehicleMake)
			.Include(m => m.Vehicles)
			.FirstOrDefaultAsync(m => m.Code == id);
		if (carModel == null) return NotFound();
		return View(carModel);
	}
}
