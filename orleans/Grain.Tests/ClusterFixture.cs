using Orleans.TestingHost;

namespace Grain.Tests;

[CollectionDefinition(ClusterCollection.Name)]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}


public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        this.Cluster = builder.Build();
        this.Cluster.Deploy();
    }

    public void Dispose()
    {
        this.Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }
}

// Add this class to configure the storage providers
public class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder
            .AddMemoryGrainStorage("userStore")
            .AddMemoryGrainStorageAsDefault();
    }
}
