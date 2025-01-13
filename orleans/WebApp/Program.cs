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
    b.UseMongoDBClient("mongodb://admin:1234@localhost:27017/orleans?authSource=admin");

    b.AddMongoDBGrainStorage(name: "userStore", options =>
    {
        options.Configure(storageOptions =>
        {
            storageOptions.DatabaseName = "orleans";
        });
    });
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

app.MapGet("/{userId}/unlockedActivities", async (string userId) =>
    {
        var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

        var achievements = await grainFactory.GetGrain<IUserGrain>(userId).GetAchievements();
        return Results.Ok(achievements);
    })
    .WithName("GetUserAchievements");

app.MapPost("/activities", async ([FromBody] ProcessActivity activity) =>
    {
        var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

        await grainFactory.GetGrain<IUserGrain>(activity.UserId).ProcessActivity(activity.Activity);
        return Results.Ok();
    })
    .WithName("ProcessActivity");


app.Run();