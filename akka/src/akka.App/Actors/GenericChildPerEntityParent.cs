using System.Reflection.Metadata.Ecma335;
using Akka.Actor;
using Akka.Cluster.Sharding;
using akka.Domain;

namespace akka.App.Actors;

/// <summary>
/// A generic "child per entity" parent actor.
/// </summary>
/// <remarks>
/// Intended for simplifying unit tests where we don't want to use Akka.Cluster.Sharding.
/// </remarks>
public sealed class GenericChildPerEntityParent : ReceiveActor
{
    public static Props Props(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        return Akka.Actor.Props.Create(() => new GenericChildPerEntityParent(extractor, propsFactory));
    }

    private readonly IMessageExtractor _extractor;
    private Func<string, Props> _propsFactory;

    private Dictionary<string, long> _processedMessages = new();

    public GenericChildPerEntityParent(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        _extractor = extractor;
        _propsFactory = propsFactory;
        
        ReceiveAny(o =>
        {
            var typeName = o.GetType().Name;
            _processedMessages[typeName] = _processedMessages.TryGetValue(typeName, out var count) ? count + 1 : 1;

            if (o is GetProcessedMessages)
            {
                Sender.Tell(_processedMessages);
                return;
            }
                        
            var entityId = _extractor.EntityId(o);
            if (string.IsNullOrEmpty(entityId)) 
                return;
            
            Context.Child(entityId).GetOrElse(() => Context.ActorOf(propsFactory(entityId), entityId))
                .Forward(_extractor.EntityMessage(o));
        });
    }
}