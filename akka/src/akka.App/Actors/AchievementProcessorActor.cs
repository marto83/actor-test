using Akka.Actor;
using akka.Domain;
using Akka.Event;

namespace akka.App.Actors;

public class AchievementProcessorActor : ReceiveActor
{
    private readonly HashSet<string> _processedActivityIds = new();
    private int _activityCount = 0;
    private readonly Achievement _achievement; // This should be loaded from configuration or database

    public AchievementProcessorActor(string activityType)
    {
        _achievement = new Achievement
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{activityType} Achievement",
            TrackedActivityType = activityType,
            RequiredCount = 5, // Example count
            Description = $"Process {5} {activityType} activities"
        };

        Receive<ProcessActivity>(message =>
        {
            if (message.Activity.Type != activityType ||
                !_processedActivityIds.Add(message.Activity.Id))
                return;

            Context.GetLogger().Info("Increment activity count for achievement: {Type}", _achievement.TrackedActivityType);
            _activityCount++;

            if (_activityCount >= _achievement.RequiredCount)
            {
                _activityCount = 0; // Allow for achievements to be unlocked multiple times
                Context.Parent.Tell(new AchievementUnlocked
                {
                    Achievement = _achievement,
                    UserId = message.UserId
                });
            }
        });
    }
}
