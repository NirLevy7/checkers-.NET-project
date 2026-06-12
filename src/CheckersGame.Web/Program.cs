using CheckersGame.Data.Context;
using CheckersGame.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<CheckersCentralDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CheckersCentralDb")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Seed sample data on startup (only runs if DB is empty)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CheckersCentralDb>();
    DbSeeder.Seed(db);
}

app.Run();
