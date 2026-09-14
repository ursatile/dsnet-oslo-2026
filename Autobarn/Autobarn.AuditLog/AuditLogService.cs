using Autobarn.Messages;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Autobarn.AuditLog;

class AuditLogService(
	IBus bus, ILogger<AuditLogService> logger
) : IHostedService {
	private const string SUBSCRIBER_ID = "autobarn.auditlog";
	private SubscriptionResult subscription;

	public async Task StartAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Starting Autobarn AuditLogService...");
		subscription = await bus.PubSub.SubscribeAsync<NewVehicleMessage>(SUBSCRIBER_ID,
			HandleNewVehicleMessage, cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Stopping Autobarn AuditLogService...");
		await subscription.DisposeAsync();
	}

	private async Task HandleNewVehicleMessage(NewVehicleMessage message) {
		await Task.Delay(TimeSpan.FromSeconds(1));
		logger.LogInformation("New Vehicle: {message}", message);
	}
}
