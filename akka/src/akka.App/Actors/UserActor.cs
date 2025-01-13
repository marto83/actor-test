using Akka.Actor;
using akka.Domain;
using Akka.Event;
using Akka.Persistence;

namespace akka.App.Actors;

public class UserActor : ReceivePersistentActor
{
    private User State { get; }
    private Dictionary<string, IActorRef> _achievementProcessors = new();

    public override string PersistenceId => $"user-{State.Id}";

    public UserActor(string userId)
    {
        State = new User { Id = userId };

        Command<ProcessActivity>(message =>
        {
            var achievementProcessor = GetOrCreateAchievementProcessor(message.Activity.Type);
            achievementProcessor.Forward(message);
        });

        Command<GetAchievements>(_ => { Sender.Tell(State.UnlockedAchievements); });

        Command<AchievementUnlocked>(evt =>
        {
            Persist(evt, UpdateState); //TODO: Broadcast event to external bus
        });
        
        Recover<AchievementUnlocked>(UpdateState);
    }

    private void UpdateState(AchievementUnlocked evt)
    {
        Context.GetLogger().Info("Achievement unlocked: {@Achievement}", evt.Achievement);
        State.UnlockedAchievements.Add(evt.Achievement);
    }

    private IActorRef GetOrCreateAchievementProcessor(string activityType)
    {
        if (!_achievementProcessors.ContainsKey(activityType))
        {
            _achievementProcessors[activityType] = Context.ActorOf(
                Props.Create(() => new AchievementProcessorActor(activityType)),
                $"achievement-processor-{activityType}"
            );
        }

        return _achievementProcessors[activityType];
    }
}
