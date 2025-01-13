using Akka.Actor;
using akka.App.Actors;
using akka.Domain;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace akka.App.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController : ControllerBase
{
    private readonly ILogger<ActivityController> _logger;
    private readonly IActorRef _userActor;

    
    public ActivityController(IRequiredActor<UserActor> userActor, ILogger<ActivityController> logger)
    {
        _logger = logger;
        _userActor = userActor.ActorRef;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        return Ok(new {
            Message = "Activity Controller is up and running"
        });
    }
    
    [HttpGet("messages")]
    public async Task<IActionResult> Messages()
    {
        return Ok(await _userActor.Ask(new GetProcessedMessages()));
    }
    

    [HttpPost]
    public async Task<IActionResult> ProcessActivity([FromBody] ProcessActivity request)
    {
        _logger.LogInformation("Processing activity: {@Activity}", request.Activity);
        _userActor.Tell(request);
        return Ok();
    }

    [HttpGet("{userId}/achievements")]
    public async Task<IActionResult> GetAchievements(string userId)
    {
        var achievements = await _userActor.Ask<List<Achievement>>(
            new GetAchievements { UserId = userId });
        return Ok(achievements);
    }
}
