using Autobarn.Data;

namespace Autobarn.Website.Api {
	public static class AutobarnApi {
		extension(IEndpointRouteBuilder routes) {
			public void MapAutobarnApi(string prefix) {
				routes.MapGet($"{prefix}/status", () => new {
					hostname = Environment.MachineName,
					datetime = DateTime.UtcNow
				});

				routes.MapGet($"{prefix}/greeting", (string name = "World") => new {
					greeting = $"Hello {name}"
				});

				routes.MapGet($"{prefix}/makes", (AutobarnDbContext db) => db.Makes.ToList());
				routes.MapGet($"{prefix}/models", (AutobarnDbContext db) => db.Models.ToList());
				routes.MapGet($"{prefix}/vehicles", (AutobarnDbContext db) => db.Vehicles.ToList());
			}
		}
	}
}
	
