using Microsoft.Extensions.Logging;
using OrleansTest.Grains.Models;

namespace OrleansTest.Grains;

public class AchievementLogicGrain : Grain, IAchievementLogicGrain
{
    private readonly ILogger<AchievementLogicGrain> _logger;
    private readonly Achievement _achievement;

    private int _processedCount = 0;
    private HashSet<string> _processedActivityIds = new();
    
    public AchievementLogicGrain(ILogger<AchievementLogicGrain> logger)
    {
        _logger = logger;
        // this will be pulled from a repository or something like
        _achievement = new Achievement()
        {
            Id = "1",
            Name = "First Activity",
            TrackedActivityType = "news",
            RequiredCount = 5,
            Description = "Process 5 activity1 activities"
        };
    }

    // Not happy with this method because there must be a better way. 
    // If I call the userGrain from here weird things happen.
    public async Task<IProcessActivityResult> ProcessAchievement(ProcessActivity activity)
    {
        if (activity.Activity.Type != _achievement.TrackedActivityType)
            return new IProcessActivityResult.ProcessActivityNoOp();

        if (!_processedActivityIds.Add(activity.Activity.Id))
        {
            // already processed
            return new IProcessActivityResult.ProcessActivityNoOp();
        }
        
        _processedCount++;
        
        if(_processedCount >= _achievement.RequiredCount)
        {
            _processedCount = 0;
            _logger.LogInformation("Achievement unlocked: {Achievement}", _achievement.Name);

            return new IProcessActivityResult.AchievementUnlocked(_achievement);
        }
        
        return new IProcessActivityResult.ProcessActivityNoOp();
    }
}

public class UserGrain : Grain, IUserGrain
{
    private Guid GrainKey => this.GetPrimaryKey();
    private User State { get; set; }

    private Dictionary<string, IAchievementLogicGrain> _achievementGrains = new();
    
    public UserGrain()
    {
        State = new User()
        {
            Id = GrainKey,
            Name = "User " + GrainKey
        };
    }
    
    private IAchievementLogicGrain GetActivityProcessorGrain(string activityType)
    {
        if (!_achievementGrains.ContainsKey(activityType))
        {
            _achievementGrains[activityType] = GrainFactory.GetGrain<IAchievementLogicGrain>(GrainKey + "-" + activityType);
        }

        return _achievementGrains[activityType];
    }
    
    public async Task ProcessActivity(Activity activity)
    {
        var processorGrain = GetActivityProcessorGrain(activity.Type);
        var result = await processorGrain.ProcessAchievement(new ProcessActivity()
        {
            Activity = activity,
            UserId = GrainKey
        });
        
        switch (result)
        {
            case IProcessActivityResult.AchievementUnlocked unlocked:
                await UnlockAchievement(unlocked.Achievement);
                break;
            case IProcessActivityResult.ProcessActivityNoOp _:
                break;
        }
    }

    public Task<List<Achievement>> GetAchievements()
    {
        return Task.FromResult(State.UnlockedAchievements);
    }

    public Task UnlockAchievement(Achievement achievement)
    {
        State.UnlockedAchievements.Add(achievement);
        
        return Task.CompletedTask;
    }
}