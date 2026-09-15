using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Messages;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Autobarn.Website.Services;

public class OutboxHostedService(
	IServiceProvider services,
	ILogger<OutboxHostedService> logger
) : BackgroundService {
	private int checks = 0;

	private CancellationTokenSource sleepTokenSource = new();

	private CancellationToken sleepToken;

	public void WakeUpAndDoStuff() {
		sleepTokenSource.Cancel();
	}

	protected override async Task ExecuteAsync(CancellationToken workToken) {
		logger.LogInformation("Starting OutboxHostedService...");
		while (!workToken.IsCancellationRequested) {

			sleepToken = sleepTokenSource.Token;

			logger.LogInformation("checking database for unsent messages {checks}...", checks++);

			var scope = services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AutobarnDbContext>();
			var bus = scope.ServiceProvider.GetRequiredService<IBus>();
			OutboxMessage? messageRecord = null;
			try {
				messageRecord = await db.OutboxMessages
					.AsNoTracking()
					.FirstOrDefaultAsync(m => m.SentAt == null, workToken);
			} catch (Exception ex) {
				logger.LogWarning(ex, "sqlite");
				messageRecord = null;
			}
			if (messageRecord != null) {
				switch (messageRecord.MessageType) {
					case nameof(NewVehicleMessage):
						try {
							var message = JsonSerializer.Deserialize<NewVehicleMessage>(messageRecord.MessageJson);
							await bus.PubSub.PublishAsync(message, workToken);
							messageRecord.SentAt = DateTimeOffset.UtcNow;
							logger.LogInformation("Message {message} sent at {sent}", message, messageRecord.SentAt);
						} catch (Exception ex) {
							messageRecord.SentAt = null;
							messageRecord.FailureCount++;
							messageRecord.FailureMessage = ex.Message;
						} finally {
							try {
								await db.OutboxMessages
									.Where(m => m.Id == messageRecord.Id)
									.ExecuteUpdateAsync(t
											=> t.SetProperty(mr => mr.SentAt, messageRecord.SentAt)
												.SetProperty(mr => mr.FailureCount, messageRecord.FailureCount)
												.SetProperty(mr => mr.FailureMessage, messageRecord.FailureMessage),
										CancellationToken.None);
							} catch (Exception ex) {
								logger.LogWarning(ex, "sqlite");
								messageRecord = null;
							}
						}
						break;
				}
			} else {
				logger.LogInformation("No messages to send - trying again in a bit...");
				try {
					var linkedToken
						= CancellationTokenSource.CreateLinkedTokenSource(workToken, sleepToken).Token;
					await Task.Delay(TimeSpan.FromSeconds(30), sleepToken);
				} catch (OperationCanceledException) {
					logger.LogDebug("WAKEY WAKEY! Let's go to work!");
				}
			}
			if (!sleepTokenSource.IsCancellationRequested) continue;
			sleepTokenSource.Dispose();
			sleepTokenSource = new();
		}
		logger.LogInformation("Stopping OutboxHostedService...");
	}
}
