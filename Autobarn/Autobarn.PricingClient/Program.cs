using Autobarn.PricingClient;
using Autobarn.PricingEngine;
using Autobarn.ServiceDefaults;
using EasyNetQ;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
builder.Services.AddEasyNetQ(rabbitmq);
builder.Services.AddHostedService<PricingClientService>();

var grpc = builder.Configuration[ConfigKeys.GrpcPricingServerUrl] ?? "http://localhost:5002";
var channel = GrpcChannel.ForAddress(grpc);
var pricerClient = new Pricer.PricerClient(channel);
builder.Services.AddSingleton(pricerClient);

var host = builder.Build();
host.Run();
