using CheckersGame.Api.Services;
using CheckersGame.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Central DB — shared with the Razor website (same connection string)
builder.Services.AddDbContext<CheckersCentralDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CheckersCentralDb")));

builder.Services.AddScoped<GameService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
