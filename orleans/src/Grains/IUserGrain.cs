using OrleansTest.Grains.Models;

namespace OrleansTest.Grains;

public interface IUserGrain : IGrainWithGuidKey
{
    Task ProcessActivity(Activity activity);
    Task<List<Achievement>> GetAchievements();
}

public interface IAchievementLogicGrain : IGrainWithStringKey
{
    Task<IProcessActivityResult> ProcessAchievement(ProcessActivity activity);
}