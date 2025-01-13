using System.Diagnostics;
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Discovery.Azure;
using Akka.Discovery.Config.Hosting;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Persistence.Azure.Hosting;
using Akka.Persistence.Hosting;
using Akka.Remote.Hosting;
using akka.App.Actors;
using akka.Domain;
using Akka.Persistence.MongoDb.Hosting;

namespace akka.App.Configuration;

public static class AkkaConfiguration
{
    public static IServiceCollection ConfigureWebApiAkka(this IServiceCollection services, IConfiguration configuration,
        Action<AkkaConfigurationBuilder, IServiceProvider> additionalConfig)
    {
        var akkaSettings = configuration.GetRequiredSection("AkkaSettings").Get<AkkaSettings>();
        Debug.Assert(akkaSettings != null, nameof(akkaSettings) + " != null");

        services.AddSingleton(akkaSettings);

        return services.AddAkka(akkaSettings.ActorSystemName, (builder, sp) =>
        {
            builder.ConfigureActorSystem(sp);
            additionalConfig(builder, sp);
        });
    }

    public static AkkaConfigurationBuilder ConfigureActorSystem(this AkkaConfigurationBuilder builder,
        IServiceProvider sp)
    {
        var settings = sp.GetRequiredService<AkkaSettings>();

        return builder
            .ConfigureLoggers(configBuilder =>
            {
                configBuilder.ClearLoggers();
                configBuilder.LogConfigOnStart = settings.LogConfigOnStart;
                configBuilder.AddLoggerFactory();
            })
            .ConfigureNetwork(sp)
            .ConfigurePersistence(sp)
            .ConfigureCounterActors(sp);
    }

    public static AkkaConfigurationBuilder ConfigureNetwork(this AkkaConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<AkkaSettings>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        if (!settings.UseClustering)
            return builder;

        builder
            .WithRemoting(settings.RemoteOptions);

        if (settings.AkkaManagementOptions is { Enabled: true })
        {
            // need to delete seed-nodes so Akka.Management will take precedence
            var clusterOptions = settings.ClusterOptions;
            clusterOptions.SeedNodes = Array.Empty<string>();

            builder
                .WithClustering(clusterOptions)
                .WithAkkaManagement(hostName: settings.AkkaManagementOptions.Hostname,
                    settings.AkkaManagementOptions.Port)
                .WithClusterBootstrap(serviceName: settings.AkkaManagementOptions.ServiceName,
                    portName: settings.AkkaManagementOptions.PortName,
                    requiredContactPoints: settings.AkkaManagementOptions.RequiredContactPointsNr);

            switch (settings.AkkaManagementOptions.DiscoveryMethod)
            {
                case DiscoveryMethod.Kubernetes:
                    break;
                case DiscoveryMethod.AwsEcsTagBased:
                    break;
                case DiscoveryMethod.AwsEc2TagBased:
                    break;
                case DiscoveryMethod.AzureTableStorage:
                {
                    var connectionStringName = configuration.GetSection("AzureStorageSettings")
                        .Get<AzureStorageSettings>()?.ConnectionStringName;
                    Debug.Assert(connectionStringName != null, nameof(connectionStringName) + " != null");
                    var connectionString = configuration.GetConnectionString(connectionStringName);

                    builder.WithAzureDiscovery(options =>
                    {
                        options.ServiceName = settings.AkkaManagementOptions.ServiceName;
                        options.ConnectionString = connectionString;
                    });
                    break;
                }
                case DiscoveryMethod.Config:
                {
                    builder
                        .WithConfigDiscovery(options =>
                        {
                            options.Services.Add(new Service
                            {
                                Name = settings.AkkaManagementOptions.ServiceName,
                                Endpoints = new[]
                                {
                                    $"{settings.AkkaManagementOptions.Hostname}:{settings.AkkaManagementOptions.Port}",
                                }
                            });
                        });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            builder.WithClustering(settings.ClusterOptions);
        }

        return builder;
    }

    public static AkkaConfigurationBuilder ConfigurePersistence(this AkkaConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<AkkaSettings>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        switch (settings.PersistenceMode)
        {
            case PersistenceMode.InMemory:
                return builder.WithInMemoryJournal().WithInMemorySnapshotStore();
            case PersistenceMode.Azure:
            {
                var connectionStringName = configuration.GetSection("AzureStorageSettings")
                    .Get<AzureStorageSettings>()?.ConnectionStringName;
                Debug.Assert(connectionStringName != null, nameof(connectionStringName) + " != null");
                var connectionString = configuration.GetConnectionString(connectionStringName);
                Debug.Assert(connectionString != null, nameof(connectionString) + " != null");

                return builder.WithAzurePersistence(connectionString);
            }
            case PersistenceMode.Mongo:
            {
                var connectionString = configuration.GetConnectionString("Mongo");
                Debug.Assert(connectionString != null, nameof(connectionString) + " != null");

                return builder.WithMongoDbPersistence(journal =>
                {
                    journal.Collection = "EventJournal";
                    journal.ConnectionString = connectionString;
                    journal.AutoInitialize = true;
                    journal.MetadataCollection = "JournalMetadata";
                    journal.UseWriteTransaction = false;
                }, snapshot =>
                {
                    snapshot.Collection = "SnapshotStore";
                    snapshot.ConnectionString = connectionString;
                    snapshot.AutoInitialize = true;
                    snapshot.UseWriteTransaction = false;
                });
            }
        default:
            throw new ArgumentOutOfRangeException();
    }
}

public static AkkaConfigurationBuilder ConfigureCounterActors(this AkkaConfigurationBuilder builder,
    IServiceProvider serviceProvider)
{
    var settings = serviceProvider.GetRequiredService<AkkaSettings>();
    var extractor = CreateMessageRouter();

    if (settings.UseClustering)
    {
        return builder
            .WithShardRegion<UserActor>("user",
                (system, registry, resolver) => userId => Props.Create(() => new UserActor(userId)),
                extractor, settings.ShardOptions);
    }

    return builder.WithActors((system, registry, resolver) =>
    {
        var userParent =
            system.ActorOf(
                GenericChildPerEntityParent.Props(extractor, userId => Props.Create(() => new UserActor(userId))),
                "users");
        registry.Register<UserActor>(userParent);
    });
}

public static HashCodeMessageExtractor CreateMessageRouter()
{
    return HashCodeMessageExtractor.Create(30, o =>
    {
        return o switch
        {
            IWithCounterId counterId => counterId.CounterId,
            IWithUserId userId => userId.UserId,
            _ => null
        };
    }, o => o);
}

}
