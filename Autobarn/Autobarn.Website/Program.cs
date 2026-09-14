using Autobarn.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// A named shared-cache in-memory database exists for as long as at least one connection to it is open,
// so we hold this connection open for the lifetime of the app, and each DbContext opens its own connection.
const string connectionString = "Data Source=autobarn;Mode=Memory;Cache=Shared";
await using var keepAliveConnection = new SqliteConnection(connectionString);
await keepAliveConnection.OpenAsync();

builder.Services.AddDbContext<AutobarnDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddValidation();

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

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
