using Autobarn.Notifier;
using Autobarn.ServiceDefaults;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
Console.WriteLine(rabbitmq);
builder.Services.AddEasyNetQ(rabbitmq);
builder.Services.AddHostedService<NotifierService>();

var websiteUrl = builder.Configuration[ConfigKeys.AutobarnWebsiteUrl] ?? "https://localhost:5001";
var uriBuilder = new UriBuilder(websiteUrl) { Path = "hub" };

var hub = new HubConnectionBuilder().WithUrl(uriBuilder.Uri).Build();
builder.Services.AddSingleton(hub);

var host = builder.Build();
host.Run();
