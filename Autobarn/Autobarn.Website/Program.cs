using Autobarn.Data;
using Autobarn.ServiceDefaults;
using Autobarn.Website.Api;
using Autobarn.Website.Hubs;
using Autobarn.Website.Services;
using EasyNetQ;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// A named shared-cache in-memory database exists for as long as at least one connection to it is open,
// so we hold this connection open for the lifetime of the app, and each DbContext opens its own connection.
// const string connectionString = "Data Source=autobarn;Mode=Memory;Cache=Shared";
const string CONNECTION_STRING = "Data Source=autobarndb;Cache=Shared";
await using var keepAliveConnection = new SqliteConnection(CONNECTION_STRING);
await keepAliveConnection.OpenAsync();

builder.Services.AddDbContext<AutobarnDbContext>(options => options.UseSqlite(CONNECTION_STRING));
builder.Services.AddControllersWithViews(); // options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddSignalR();

builder.AddServiceDefaults();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
builder.Services.AddEasyNetQ(rabbitmq);

// Register the OutboxHostedService as a singleton and also as a hosted service
builder.Services.AddSingleton<OutboxHostedService>();
builder.Services.AddHostedService(services
	=> services.GetRequiredService<OutboxHostedService>());
	
var app = builder.Build();
app.Logger.LogInformation("Using in-memory database");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
	app.UseHttpsRedirection();
}

await using (var scope = app.Services.CreateAsyncScope()) {
	var db = scope.ServiceProvider.GetRequiredService<AutobarnDbContext>();
	await db.Database.EnsureCreatedAsync();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapAutobarnApi("/api");
app.MapControllers();
app.MapHub<AutobarnHub>("/hub");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
