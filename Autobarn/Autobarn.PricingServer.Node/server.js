require("./instrumentation");

const fs = require("node:fs");
const path = require("node:path");
const grpc = require("@grpc/grpc-js");
const protoLoader = require("@grpc/proto-loader");
const pino = require("pino");

const logger = pino({ name: "Autobarn.PricingServer.Node" });

// Use the contract from the .NET pricing server so both servers stay wire-compatible.
const PROTO_PATH = path.resolve(__dirname, "../Autobarn.PricingServer/Protos/price.proto");
const PORT = process.env.PORT ?? "5003";
const TLS_CERT_PATH = process.env.TLS_CERT_PATH ?? path.join(__dirname, "certs", "localhost.pem");
const TLS_KEY_PATH = process.env.TLS_KEY_PATH ?? path.join(__dirname, "certs", "localhost.key");

const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
	keepCase: true,
	longs: String,
	enums: String,
	defaults: true,
	oneofs: true
});
const { Pricer } = grpc.loadPackageDefinition(packageDefinition).autobarn.pricing;

const BASE_PRICE = 30000;
const DEPRECIATION_PER_YEAR = 1500;
const MINIMUM_PRICE = 500;

function calculatePrice({ year }) {
	const age = Math.max(0, new Date().getFullYear() - year);
	const depreciated = Math.max(MINIMUM_PRICE, BASE_PRICE - age * DEPRECIATION_PER_YEAR);
	const jitter = 0.9 + Math.random() * 0.2;
	return Math.round(depreciated * jitter);
}

function getPrice(call, callback) {
	const request = call.request;
	logger.info({ year: request.year, make: request.make, model: request.model, color: request.color },
		"Received GetPrice request for %d %s %s %s", request.year, request.make, request.model, request.color);

	const now = Date.now();
	const reply = {
		price: calculatePrice(request),
		currencyCode: "USD",
		timestamp: { seconds: Math.floor(now / 1000), nanos: (now % 1000) * 1e6 }
	};

	logger.info({ price: reply.price, currencyCode: reply.currencyCode },
		"Returning price %d %s", reply.price, reply.currencyCode);
	callback(null, reply);
}

function loadCredentials() {
	if (!fs.existsSync(TLS_CERT_PATH) || !fs.existsSync(TLS_KEY_PATH)) {
		logger.fatal({ TLS_CERT_PATH, TLS_KEY_PATH },
			"TLS certificate not found. Run 'npm run dev-cert' to export the ASP.NET Core dev certificate, " +
			"or set TLS_CERT_PATH and TLS_KEY_PATH.");
		process.exit(1);
	}
	return grpc.ServerCredentials.createSsl(null, [{
		cert_chain: fs.readFileSync(TLS_CERT_PATH),
		private_key: fs.readFileSync(TLS_KEY_PATH)
	}], false);
}

const server = new grpc.Server();
server.addService(Pricer.service, { GetPrice: getPrice });
server.bindAsync(`0.0.0.0:${PORT}`, loadCredentials(), (error, port) => {
	if (error) {
		logger.fatal({ err: error }, "Failed to bind gRPC server");
		process.exit(1);
	}
	logger.info("Autobarn pricing server (Node.js) listening on https://localhost:%d", port);
});
