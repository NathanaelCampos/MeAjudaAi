using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task Enfileirar_DeveCriarExecucaoPendente()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.PostAsync("/api/admin/jobs/notificacoes-retencao/enfileirar", null);
        var payload = await response.Content.ReadFromJsonAsync<EnfileirarBackgroundJobAdminResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("notificacoes-retencao", payload!.JobId);
        Assert.Equal("Pendente", payload.Status);
    }

    [Fact]
    public async Task ListarFila_DeveRetornarExecucaoEnfileirada()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        await adminClient.PostAsync("/api/admin/jobs/notificacoes-retencao/enfileirar", null);

        var response = await adminClient.GetAsync("/api/admin/jobs/fila");
        var payload = await response.Content.ReadFromJsonAsync<List<BackgroundJobFilaItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Contains(payload!, x => x.JobId == "notificacoes-retencao" && x.Status == "Pendente");
    }

    [Fact]
    public async Task ProcessarFila_DeveExecutarPendentes()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var enfileirarResponse = await adminClient.PostAsync("/api/admin/jobs/notificacoes-retencao/enfileirar", null);
        var enfileirarPayload = await enfileirarResponse.Content.ReadFromJsonAsync<EnfileirarBackgroundJobAdminResponse>();

        var response = await adminClient.PostAsync("/api/admin/jobs/fila/processar", null);
        var payload = await response.Content.ReadFromJsonAsync<ProcessarFilaBackgroundJobAdminResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.ExecucoesProcessadas >= 1);

        var filaResponse = await adminClient.GetAsync("/api/admin/jobs/fila");
        var filaPayload = await filaResponse.Content.ReadFromJsonAsync<List<BackgroundJobFilaItemResponse>>();

        Assert.NotNull(enfileirarPayload);
        Assert.NotNull(filaPayload);
        Assert.Contains(filaPayload!, x => x.ExecucaoId == enfileirarPayload!.ExecucaoId && x.Status == "Sucesso");
    }

    [Fact]
    public async Task CancelarExecucao_DeveAtualizarStatusParaCancelado()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var execucaoId = await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Pendente);

        var response = await adminClient.PutAsync($"/api/admin/jobs/fila/{execucaoId}/cancelar", null);
        var payload = await response.Content.ReadFromJsonAsync<BackgroundJobFilaItemResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Cancelado", payload!.Status);
    }

    [Fact]
    public async Task ReabrirExecucao_DeveRetornarParaPendente()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var execucaoId = await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Falha,
            processarAposUtc: DateTime.UtcNow.AddMinutes(10));

        var response = await adminClient.PutAsync($"/api/admin/jobs/fila/{execucaoId}/reabrir", null);
        var payload = await response.Content.ReadFromJsonAsync<BackgroundJobFilaItemResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Pendente", payload!.Status);
        Assert.NotNull(payload.ProcessarAposUtc);
        Assert.True(payload.ProcessarAposUtc <= DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task CancelarExecucao_SucessoNaoDevePermitirAlteracao()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var execucaoId = await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Sucesso);

        var response = await adminClient.PutAsync($"/api/admin/jobs/fila/{execucaoId}/cancelar", null);
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Execução não pode ser cancelada.", payload!.Mensagem);
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

    private async Task<Guid> CriarExecucaoAsync(
        StatusExecucaoBackgroundJob status,
        DateTime? processarAposUtc = null)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var execucao = new BackgroundJobExecucao
        {
            JobId = "notificacoes-retencao",
            NomeJob = "Retenção de notificações internas",
            Origem = "teste",
            Status = status,
            ProcessarAposUtc = processarAposUtc,
            MensagemResultado = "Execução de teste."
        };

        context.BackgroundJobsExecucoes.Add(execucao);
        await context.SaveChangesAsync();

        return execucao.Id;
    }
}
