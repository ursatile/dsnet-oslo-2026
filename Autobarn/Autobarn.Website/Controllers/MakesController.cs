using Autobarn.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autobarn.Website.Controllers;

public class MakesController(AutobarnDbContext db) : Controller {

	public async Task<IActionResult> Index() {
		var list = await db.Makes.ToListAsync();
		return View(list);
	}

	public async Task<IActionResult> Details(string id) {
		var make = await db.Makes
			.Include(m => m.Models)
			.FirstOrDefaultAsync(m => m.Code == id);
		if (make == null) return NotFound();
		return View(make);
	}
}
