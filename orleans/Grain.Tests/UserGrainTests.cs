using FluentAssertions;
using Orleans.TestingHost;
using OrleansTest.Grains;
using OrleansTest.Grains.Models;

namespace Grain.Tests;

[Collection(ClusterCollection.Name)]
public class UserGrainTests(ClusterFixture fixture)
{
    private readonly TestCluster _cluster = fixture.Cluster;

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
    public async Task SaysHelloCorrectly()
    {
        var userGrain = _cluster.GrainFactory.GetGrain<IUserGrain>("user1");
        var activities = new int[] { 1, 2, 3, 4, 5 }.Select(x => CreateActivity(x.ToString()));
        await Task.WhenAll(activities.Select(activity => userGrain.ProcessActivity(activity)).ToArray());
        
        var achievements = await userGrain.GetAchievements();
        achievements.Should().HaveCount(1);
    }
}