using System.Text.Json;
using Autobarn.Messages;

namespace Autobarn.Data.Entities;

#pragma warning disable CS8618
public class OutboxMessage {
	public ulong Id { get; set; }
	public string MessageType { get; set; } = "";
	public string MessageJson { get; set; } = "";
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? SentAt { get; set; }
	public int FailureCount { get; set; } = 0;
	public string? FailureMessage { get; set; } = null;

	public OutboxMessage() { }

	public OutboxMessage(NewVehicleMessage newVehicleMessage) {
		this.MessageType = nameof(NewVehicleMessage);
		this.MessageJson = JsonSerializer.Serialize(newVehicleMessage);
		this.CreatedAt = DateTimeOffset.UtcNow;
	}
}
