using Autobarn.Messages;
using Autobarn.PricingEngine;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Autobarn.PricingClient;

class PricingClientService(
	IBus bus,
	Pricer.PricerClient grpc,
	ILogger<PricingClientService> logger
) : IHostedService {
	private const string SUBSCRIBER_ID = "Autobarn.PricingClient";
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
		logger.LogInformation("New Vehicle: {message}", message);
		await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000)));
		var priceRequest = new PriceRequest {
			Year = message.Year,
			Color = message.Color,
			Make = message.Make,
			Model = message.Model
		};
		var reply = await grpc.GetPriceAsync(priceRequest);
		logger.LogInformation("Price for new vehicle: {price} {currency}", reply.Price, reply.CurrencyCode);
		var newVehiclePriceMessage = message.WithPrice(reply.Price, reply.CurrencyCode, reply.Timestamp.ToDateTimeOffset());
		await bus.PubSub.PublishAsync(newVehiclePriceMessage);
	}
}
