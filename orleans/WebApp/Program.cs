using Microsoft.AspNetCore.Mvc;
using OrleansTest.Grains;
using OrleansTest.Grains.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseOrleans(b =>
{
    b.UseLocalhostClustering();
    b.UseDashboard(opts =>
    {
        opts.Host = "localhost";
        opts.Port = 8081;
        opts.HostSelf = true;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/{userId}/unlockedActivities", async (Guid userId) =>
    {
        var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

        var achievements = await grainFactory.GetGrain<IUserGrain>(userId).GetAchievements();
        return Results.Ok(achievements);
    })
    .WithName("GetUserAchievements");

app.MapPost("/{userId}/activities", async (Guid userId, [FromBody] Activity activity) =>
    {
        var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

        await grainFactory.GetGrain<IUserGrain>(userId).ProcessActivity(activity);
        return Results.Ok();
    })
    .WithName("ProcessActivity");
    
    

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}