using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.IntegrationTests.Infrastructure;
using MeAjudaAi.IntegrationTests.Jobs;
using Microsoft.EntityFrameworkCore;
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
    public async Task Agendar_DevePersistirProcessarAposFuturo()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var processarApos = DateTime.UtcNow.AddMinutes(45);
        var response = await adminClient.PostAsJsonAsync("/api/admin/jobs/notificacoes-retencao/agendar", new
        {
            processarAposUtc = processarApos
        });
        var payload = await response.Content.ReadFromJsonAsync<EnfileirarBackgroundJobAdminResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("notificacoes-retencao", payload!.JobId);
        Assert.Equal("Pendente", payload.Status);

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var execucao = await context.BackgroundJobsExecucoes.FirstAsync(x => x.Id == payload!.ExecucaoId);
        Assert.Equal(StatusExecucaoBackgroundJob.Pendente, execucao.Status);
        Assert.NotNull(execucao.ProcessarAposUtc);
        Assert.True(Math.Abs((execucao.ProcessarAposUtc!.Value - processarApos).TotalSeconds) < 1);
        Assert.Equal("Execução agendada.", execucao.MensagemResultado);
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
    public async Task ListarFila_FiltraPorJobIdEStatus()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Pendente, jobId: "notificacoes-retencao");
        await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Falha, jobId: "notificacoes-retencao");
        await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Pendente, jobId: "emails-outbox");

        var response = await adminClient.GetAsync("/api/admin/jobs/fila?jobId=notificacoes-retencao&status=Falha&limit=2");
        var payload = await response.Content.ReadFromJsonAsync<List<BackgroundJobFilaItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal("Falha", payload[0].Status);
        Assert.Equal("notificacoes-retencao", payload[0].JobId);
    }

    [Fact]
    public async Task CancelarPorJob_DeveMarcarTodasExecucoesPendentes()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var execucao1 = await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Pendente, jobId: "notificacoes-retencao");
        var execucao2 = await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Processando, jobId: "notificacoes-retencao");
        await CriarExecucaoAsync(StatusExecucaoBackgroundJob.Pendente, jobId: "emails-outbox");

        var response = await adminClient.PostAsync("/api/admin/jobs/notificacoes-retencao/cancelar-todos", null);
        var payload = await response.Content.ReadFromJsonAsync<CancelarBackgroundJobAdminResponse>();
        var texto = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, texto);
        Assert.NotNull(payload);
        Assert.Equal("notificacoes-retencao", payload!.JobId);
        Assert.Equal(2, payload.Canceladas);

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var canceladas = await context.BackgroundJobsExecucoes
            .Where(x => x.Id == execucao1 || x.Id == execucao2)
            .ToListAsync();

        Assert.All(canceladas, x => Assert.Equal(StatusExecucaoBackgroundJob.Cancelado, x.Status));
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
    public async Task ProcessarFila_ReprocessaExecucaoFalhaAgendada()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var execucaoId = await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Falha,
            processarAposUtc: DateTime.UtcNow.AddSeconds(-1),
            jobId: IntegrationTestQueueJobProcessor.JobIdConst,
            nomeJob: "Job de teste da fila",
            tentativasProcessamento: 1);

        var response = await adminClient.PostAsync("/api/admin/jobs/fila/processar", null);
        response.EnsureSuccessStatusCode();

        Assert.Equal(1, IntegrationTestQueueJobProcessor.ExecutionCount);

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var execucao = await context.BackgroundJobsExecucoes.FirstAsync(x => x.Id == execucaoId);

        Assert.Equal(StatusExecucaoBackgroundJob.Sucesso, execucao.Status);
        Assert.Equal(2, execucao.TentativasProcessamento);
        Assert.Null(execucao.ProcessarAposUtc);
        Assert.Equal("Execução concluída com sucesso.", execucao.MensagemResultado);
        Assert.Equal(1, execucao.RegistrosProcessados);
    }

    [Fact]
    public async Task Metricas_DeveReportarContagemPorStatus()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var agora = DateTime.UtcNow;
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Pendente,
            dataCriacao: agora.AddSeconds(-5));
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Processando,
            dataInicioProcessamento: agora.AddSeconds(-3),
            dataCriacao: agora.AddSeconds(-8));
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Sucesso,
            dataInicioProcessamento: agora.AddSeconds(-10),
            dataFinalizacao: agora.AddSeconds(-5),
            dataCriacao: agora.AddSeconds(-15));
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Falha,
            dataInicioProcessamento: agora.AddSeconds(-20),
            dataFinalizacao: agora.AddSeconds(-10),
            dataCriacao: agora.AddSeconds(-25));
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Cancelado,
            dataCriacao: agora.AddSeconds(-30));

        var response = await adminClient.GetAsync("/api/admin/jobs/fila/metricas");
        var payload = await response.Content.ReadFromJsonAsync<BackgroundJobFilaMetricasResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(1, payload!.TotalPendentes);
        Assert.Equal(1, payload.TotalProcessando);
        Assert.Equal(1, payload.TotalSucesso);
        Assert.Equal(1, payload.TotalFalhas);
        Assert.Equal(1, payload.TotalCancelados);
        Assert.Contains("notificacoes-retencao", payload.PorJob.Keys);
        Assert.True(payload.TempoMedioFilaSegundos >= 0);
        Assert.True(payload.TempoMedioEsperaSegundos > 0);
        Assert.True(payload.TempoMedioProcessamentoSegundos > 0);
        Assert.True(payload.TempoMedioFalhaSegundos > 0);
    }

    [Fact]
    public async Task Alertas_DeveRetornarJobsComFilaLongaOuFalhas()
    {
        using var adminClient = Factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var agora = DateTime.UtcNow;
        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Falha,
            dataInicioProcessamento: agora.AddSeconds(-30),
            dataFinalizacao: agora.AddSeconds(-20),
            jobId: "critico-falha",
            dataCriacao: agora.AddSeconds(-40));

        await CriarExecucaoAsync(
            StatusExecucaoBackgroundJob.Pendente,
            jobId: "critico-fila",
            dataCriacao: agora.AddSeconds(-90));

        var response = await adminClient.GetAsync("/api/admin/jobs/fila/alertas");
        var payload = await response.Content.ReadFromJsonAsync<List<BackgroundJobFilaAlertaResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Contains(payload!, x => x.JobId == "critico-falha" && x.NivelAlerta == "Falhas");
        Assert.Contains(payload, x => x.JobId == "critico-fila");
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
        DateTime? processarAposUtc = null,
        string? jobId = null,
        string? nomeJob = null,
        int tentativasProcessamento = 0,
        DateTime? dataInicioProcessamento = null,
        DateTime? dataFinalizacao = null,
        DateTime? dataCriacao = null)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var execucao = new BackgroundJobExecucao
        {
            JobId = jobId ?? "notificacoes-retencao",
            NomeJob = nomeJob ?? "Retenção de notificações internas",
            Origem = "teste",
            Status = status,
            ProcessarAposUtc = processarAposUtc,
            TentativasProcessamento = tentativasProcessamento,
            MensagemResultado = "Execução de teste."
        };

        if (dataCriacao.HasValue)
        {
            execucao.DataCriacao = dataCriacao.Value;
        }

        execucao.DataInicioProcessamento = dataInicioProcessamento;
        execucao.DataFinalizacao = dataFinalizacao;

        context.BackgroundJobsExecucoes.Add(execucao);
        await context.SaveChangesAsync();

        return execucao.Id;
    }
}
