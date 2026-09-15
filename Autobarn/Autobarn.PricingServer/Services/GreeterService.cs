using Autobarn.PricingEngine;
using Grpc.Core;

namespace Autobarn.PricingServer.Services;

public class GreeterService(ILogger<GreeterService> logger) : Pricer.PricerBase {
	public override Task<PriceReply> GetPrice(PriceRequest request, ServerCallContext context) {
		logger.LogInformation("Received GetPrice request for {Year} {Make} {Model} {Color}", request.Year, request.Make, request.Model, request.Color);
		var price = new PriceReply {
			Price = 12345,
			CurrencyCode = "USD",
			Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
		};
		logger.LogInformation("Returning price {Price} {CurrencyCode} at {Timestamp}", price.Price, price.CurrencyCode, price.Timestamp);
		return Task.FromResult(price);
	}
}
