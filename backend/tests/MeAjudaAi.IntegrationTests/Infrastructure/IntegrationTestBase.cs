namespace MeAjudaAi.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetStateAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
