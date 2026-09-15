using Microsoft.AspNetCore.SignalR;

namespace Autobarn.Website.Hubs {
	public class AutobarnHub(ILogger<AutobarnHub> logger) : Hub {
		public async Task TellEverybodyAboutANewCar(string user, string message) {
			logger.LogInformation("New car for sale: {user} - {message}", user, message);
			await Clients.All.SendAsync("HeyANewCarIsForSale", user, message);
		}
	}
}
