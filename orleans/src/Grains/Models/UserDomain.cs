namespace OrleansTest.Grains.Models;

[GenerateSerializer]
public class ActivityData
{
    [Id(0)]
    public string Id { get; set; }
    [Id(1)]
    public string Title { get; set; }
}

[GenerateSerializer]
public class Activity
{
    [Id(0)]
    public string Id { get; set; }
    [Id(1)]
    public string Name { get; set; }
    [Id(2)]
    public string Type { get; set; }
    [Id(3)]
    public ActivityData Data { get; set; }
}

[GenerateSerializer]
public class Achievement
{
    [Id(0)]
    public string Id { get; set; }
    [Id(1)]
    public string Name { get; set; }
    [Id(2)]
    public string TrackedActivityType { get; set; }
    [Id(3)]
    public int RequiredCount { get; set; }
    [Id(4)]
    public string Description { get; set; }
}

[GenerateSerializer]
public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Achievement> UnlockedAchievements { get; set; } = new();
}

[GenerateSerializer]
public class ProcessActivity 
{
    [Id(0)]
    public Activity Activity { get; set; }
    [Id(1)]
    public string UserId { get; set; }
}

public interface IProcessActivityResult
{
    [GenerateSerializer]
    public record ProcessActivityNoOp : IProcessActivityResult
    {
    }

    [GenerateSerializer]
    public record AchievementUnlocked : IProcessActivityResult
    {
        public AchievementUnlocked(Achievement achievement)
        {
            Achievement = achievement;
        }
        
        [Id(0)]
        public Achievement Achievement { get; set; }
    }
}


