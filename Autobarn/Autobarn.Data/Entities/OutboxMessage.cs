using System.Diagnostics;
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

	/// <summary>W3C traceparent of the activity that created this message, so the outbox can continue the trace when it sends it.</summary>
	public string? TraceParent { get; set; }
	public string? TraceState { get; set; }

	public OutboxMessage() { }

	public OutboxMessage(NewVehicleMessage newVehicleMessage) {
		this.MessageType = nameof(NewVehicleMessage);
		this.MessageJson = JsonSerializer.Serialize(newVehicleMessage);
		this.CreatedAt = DateTimeOffset.UtcNow;
		this.TraceParent = Activity.Current?.Id;
		this.TraceState = Activity.Current?.TraceStateString;
	}
}
