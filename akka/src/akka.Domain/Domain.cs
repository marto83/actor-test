namespace akka.Domain;

public class ActivityData
{
    public string Id { get; set; }
    public string Title { get; set; }
}

public class Activity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public ActivityData Data { get; set; }
}

public class Achievement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string TrackedActivityType { get; set; }
    public int RequiredCount { get; set; }
    public string Description { get; set; }
}

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Achievement> UnlockedAchievements { get; set; } = new();
}

public class ProcessActivity : IWithUserId
{
    public Activity Activity { get; set; }
    public string UserId { get; set; }
}

public class GetAchievements : IWithUserId
{
    public string UserId { get; set; }
}

public class AchievementUnlocked : IWithUserId
{
    public Achievement Achievement { get; set; }
    public string UserId { get; set; }
}


public record GetProcessedMessages : IWithUserId
{
    public string UserId { get; set; }
}
