using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Messages;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace Autobarn.Website.Services;

public class OutboxHostedService(
	IServiceProvider services,
	ILogger<OutboxHostedService> logger
) : BackgroundService {
	// ServiceDefaults registers an ActivitySource named after the application (i.e. the assembly name) for tracing
	private static readonly ActivitySource activitySource = new(typeof(OutboxHostedService).Assembly.GetName().Name!);

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
				using var activity = StartSendActivity(messageRecord);
				switch (messageRecord.MessageType) {
					case nameof(NewVehicleMessage):
						try {
							if (Random.Shared.Next(5) == 2) throw new("Distributed Systems are hard.");
							var message = JsonSerializer.Deserialize<NewVehicleMessage>(messageRecord.MessageJson);
							await bus.PubSub.PublishAsync(message, workToken);
							messageRecord.SentAt = DateTimeOffset.UtcNow;
							logger.LogInformation("Message {message} sent at {sent}", message, messageRecord.SentAt);
						} catch (Exception ex) {
							activity?.AddException(ex);
							activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
							messageRecord.SentAt = null;
							messageRecord.FailureCount++;
							messageRecord.FailureMessage = ex.Message;
							logger.LogWarning(ex, "Failed to send message {message}", messageRecord);
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
					await Task.Delay(TimeSpan.FromSeconds(30), linkedToken);
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

	// Continue the trace of the request that wrote the message to the outbox, so the publish (and the
	// subscriber's deliver span, via RabbitMQ message headers) show up in the same trace as that request.
	private static Activity? StartSendActivity(OutboxMessage messageRecord) {
		ActivityContext.TryParse(messageRecord.TraceParent, messageRecord.TraceState, out var parentContext);
		return activitySource.StartActivity($"outbox send {messageRecord.MessageType}", ActivityKind.Internal, parentContext)
			?.SetTag("outbox.message.id", messageRecord.Id)
			.SetTag("outbox.message.type", messageRecord.MessageType)
			.SetTag("outbox.message.failure_count", messageRecord.FailureCount);
	}
}
