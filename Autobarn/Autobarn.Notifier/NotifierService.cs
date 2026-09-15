using Autobarn.Messages;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;
using System.Text.Json;

namespace Autobarn.Notifier;

class NotifierService(
	IBus bus,
	HubConnection hub,
	ILogger<NotifierService> logger
) : IHostedService {
	private const string SUBSCRIBER_ID = "autobarn.notifier";
	private SubscriptionResult subscription;
	private static readonly Player player = new();

	public async Task StartAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Starting Autobarn NotifierService...");
		subscription = await bus.PubSub.SubscribeAsync<NewVehiclePriceMessage>(SUBSCRIBER_ID,
			HandleNewVehiclePriceMessage, cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Stopping Autobarn NotifierService...");
		await subscription.DisposeAsync();
	}

	private async Task HandleNewVehiclePriceMessage(NewVehiclePriceMessage message) {
		logger.LogInformation("New Vehicle Price: {message}", message);
		var json = JsonSerializer.Serialize(message);
		player.Play("sample.wav");
		await SendWithReconnect(json);
	}

	private async Task SendWithReconnect(string json, int maxRetries = 5) {
		for (var attempt = 0; ; attempt++) {
			try {
				if (hub.State == HubConnectionState.Disconnected) await hub.StartAsync();
				await hub.SendAsync("TellEverybodyAboutANewCar", "Notifier", json);
				return;
			} catch (Exception ex) when (attempt < maxRetries) {
				var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
				logger.LogWarning("Send failed (attempt {attempt}): {message}. Retrying in {delay}s...", attempt + 1, ex.Message, delay.TotalSeconds);
				await Task.Delay(delay);
			}
		}
	}
}
