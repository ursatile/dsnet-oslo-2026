// OpenTelemetry bootstrap. This must be required before @grpc/grpc-js or pino are loaded,
// so the instrumentations can patch them.
//
// Exporters are configured from the standard OTEL_* environment variables. When this runs under
// the Aspire AppHost, Aspire sets OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_EXPORTER_OTLP_PROTOCOL,
// OTEL_SERVICE_NAME and OTEL_RESOURCE_ATTRIBUTES so traces, metrics and logs go to the dashboard.
const { NodeSDK } = require("@opentelemetry/sdk-node");
const { GrpcInstrumentation } = require("@opentelemetry/instrumentation-grpc");
const { PinoInstrumentation } = require("@opentelemetry/instrumentation-pino");
const { RuntimeNodeInstrumentation } = require("@opentelemetry/instrumentation-runtime-node");

if (!process.env.OTEL_EXPORTER_OTLP_ENDPOINT) {
	// Running standalone: don't try to export to a collector that isn't there.
	process.env.OTEL_TRACES_EXPORTER ??= "none";
	process.env.OTEL_METRICS_EXPORTER ??= "none";
	process.env.OTEL_LOGS_EXPORTER ??= "none";
}

const sdk = new NodeSDK({
	serviceName: process.env.OTEL_SERVICE_NAME ?? "autobarn-pricing-server-node",
	instrumentations: [
		new GrpcInstrumentation(),
		new PinoInstrumentation(),
		new RuntimeNodeInstrumentation()
	]
});

sdk.start();

const shutdown = () => sdk.shutdown().finally(() => process.exit(0));
process.once("SIGTERM", shutdown);
process.once("SIGINT", shutdown);
