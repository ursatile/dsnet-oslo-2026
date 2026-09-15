using Autobarn.Messages;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Autobarn.PricingClient;

class PricingClientService(
	IBus bus, ILogger<PricingClientService> logger
) : IHostedService {
	private const string SUBSCRIBER_ID = "autobarn.PricingClient";
	private SubscriptionResult subscription;

	public async Task StartAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Starting Autobarn PricingClientService...");
		subscription = await bus.PubSub.SubscribeAsync<NewVehicleMessage>(SUBSCRIBER_ID,
			HandleNewVehicleMessage, cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Stopping Autobarn PricingClientService...");
		await subscription.DisposeAsync();
	}

	private async Task HandleNewVehicleMessage(NewVehicleMessage message) {
		await Task.Delay(TimeSpan.FromSeconds(1));
		logger.LogInformation("New Vehicle: {message}", message);
	}
}
