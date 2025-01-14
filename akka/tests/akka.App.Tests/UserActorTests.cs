using akka.App.Actors;
using Akka.Hosting;
using Akka.Hosting.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using akka.App.Configuration;
using akka.Domain;
using Xunit.Abstractions;

namespace akka.App.Tests;

public class UserActorSpecs : TestKit
{
    private const string UserId = "User1";
    public UserActorSpecs(ITestOutputHelper output) : base(output:output)
    {
    }

    private Activity CreateActivity(string activityId)
    {
        return new Activity()
        {
            Id = activityId,
            Name = "Activity1",
            Type = "news",
            Data = new ActivityData()
            {
                Id = "test",
                Title = "Title"
            }
        };
    }
    
    [Fact]
    public async Task UserActor_Processes_Activities_and_Receives_Achievements()
    {
        // arrange (counter actor parent is already running)
        var userActor = ActorRegistry.Get<UserActor>();
        var processActivity = new ProcessActivity()
        {
            UserId = UserId,
            Activity = CreateActivity("activity1")
        };

        var messages = new int[] { 1, 2, 3, 4, 5 }.Select(x => new ProcessActivity()
            { UserId = UserId, Activity = CreateActivity(x.ToString()) });

        foreach (var msg in messages)
        {
            userActor.Tell(msg, TestActor);
        }

        await Task.Delay(100);
        // assert
        var achievements = await userActor.Ask<List<Achievement>>(new GetAchievements { UserId = UserId });
        achievements.Should().HaveCount(1);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var settings = new AkkaSettings() { UseClustering = false, PersistenceMode = PersistenceMode.InMemory };
        services.AddSingleton(settings);
        base.ConfigureServices(context, services);
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.ConfigureCounterActors(provider).ConfigurePersistence(provider);
    }
}