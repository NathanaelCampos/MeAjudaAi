using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminJobsEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public AdminJobsEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Listar_DeveRetornarJobsRegistrados()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync("/api/admin/jobs");
        var payload = await response.Content.ReadFromJsonAsync<List<BackgroundJobAdminItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Contains(payload!, x => x.JobId == "emails-outbox");
        Assert.Contains(payload, x => x.JobId == "notificacoes-retencao");
    }

    [Fact]
    public async Task Executar_DeveProcessarJobConhecido()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.PostAsync("/api/admin/jobs/notificacoes-retencao/executar", null);
        var payload = await response.Content.ReadFromJsonAsync<ExecutarBackgroundJobAdminResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("notificacoes-retencao", payload!.JobId);
        Assert.True(payload.ExecutadoEm <= DateTime.UtcNow);
    }

    [Fact]
    public async Task Executar_JobInexistente_DeveRetornar404()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.PostAsync("/api/admin/jobs/inexistente/executar", null);
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Job não encontrado.", payload!.Mensagem);
    }

    private static async Task<AuthResponse> LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestWebApplicationFactory.EmailAdmin,
            senha = TestWebApplicationFactory.SenhaAdmin
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}
