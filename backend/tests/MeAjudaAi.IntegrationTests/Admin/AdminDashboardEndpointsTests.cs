using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminDashboardEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private const string SegredoWebhook = "segredo-webhook-teste";
    private const string HeaderAssinatura = "X-Webhook-Signature";

    private readonly TestWebApplicationFactory _factory;

    public AdminDashboardEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Obter_DeveRetornarIndicadoresGlobais()
    {
        using var adminClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var clienteClient = _factory.CreateClient();
        using var webhookClient = _factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-dashboard");
        var cliente = await RegistrarClienteAsync(clienteClient, "cli-admin-dashboard");

        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Token);

        var planoId = await ObterPlanoIdAsync();
        var (profissaoId, especialidadeId) = await _factory.ObterProfissaoEEspecialidadeAsync();
        var cidadeId = await _factory.ObterCidadeIdAsync();
        var bairroId = await _factory.ObterBairroIdAsync();

        var contratarResponse = await profissionalClient.PostAsJsonAsync("/api/impulsionamentos/contratar", new
        {
            planoImpulsionamentoId = planoId,
            codigoReferenciaPagamento = "admin-dashboard-impulsionamento"
        });
        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);

        var webhookRequest = CriarWebhookRequest("admin-dashboard-impulsionamento", "admin-dashboard-evt");
        var webhookResponse = await webhookClient.SendAsync(webhookRequest);
        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        var servicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissional.ProfissionalId,
            ProfissaoId = profissaoId,
            EspecialidadeId = especialidadeId,
            CidadeId = cidadeId,
            BairroId = bairroId,
            Titulo = "Troca de lampada",
            Descricao = "Troca simples",
            ValorCombinado = 100m
        });
        Assert.Equal(HttpStatusCode.OK, servicoResponse.StatusCode);

        var servico = await servicoResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        Assert.NotNull(servico);

        var aceitar = await profissionalClient.PutAsync($"/api/servicos/{servico!.Id}/aceitar", null);
        var iniciar = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/iniciar", null);
        var concluir = await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/concluir", null);
        Assert.Equal(HttpStatusCode.OK, aceitar.StatusCode);
        Assert.Equal(HttpStatusCode.OK, iniciar.StatusCode);
        Assert.Equal(HttpStatusCode.OK, concluir.StatusCode);

        var avaliacao = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new
        {
            servicoId = servico.Id,
            notaAtendimento = 5,
            notaServico = 5,
            notaPreco = 4,
            comentario = "Muito bom"
        });
        Assert.Equal(HttpStatusCode.OK, avaliacao.StatusCode);

        var bloquearProfissional = await adminClient.PutAsync($"/api/admin/usuarios/{profissional.Auth.UsuarioId}/bloquear", null);
        Assert.Equal(HttpStatusCode.OK, bloquearProfissional.StatusCode);

        var response = await adminClient.GetAsync("/api/admin/dashboard");
        var payload = await response.Content.ReadFromJsonAsync<AdminDashboardResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.Usuarios.Total >= 3);
        Assert.True(payload.Usuarios.Profissionais >= 1);
        Assert.True(payload.Usuarios.Clientes >= 1);
        Assert.True(payload.Servicos.Total >= 1);
        Assert.True(payload.Servicos.Concluidos >= 1);
        Assert.True(payload.Avaliacoes.Total >= 1);
        Assert.True(payload.Impulsionamentos.Total >= 1);
        Assert.True(payload.Webhooks.Total >= 1);
        Assert.True(payload.Notificacoes.TotalAtivas >= 1 || payload.Emails.Total >= 0);
        Assert.True(payload.Series.Servicos.Count >= 1);
        Assert.True(payload.Series.Avaliacoes.Count >= 1);
        Assert.True(payload.Series.Webhooks.Count >= 1);
        Assert.True(payload.Series.Emails.Count >= 0);
        Assert.True(payload.Tendencias.Servicos.Ultimos7Dias >= 1);
        Assert.True(payload.Tendencias.Avaliacoes.Ultimos7Dias >= 1);
        Assert.True(payload.Tendencias.Webhooks.Ultimos7Dias >= 1);
        Assert.True(payload.Pendencias.AvaliacoesPendentesModeracao >= 1);
        Assert.True(payload.Pendencias.ImpulsionamentosPendentesPagamento >= 0);
        Assert.True(payload.Pendencias.ServicosSolicitados >= 0);
        Assert.True(payload.Alertas.WebhooksFalhos >= 0);
        Assert.True(payload.Alertas.EmailsComFalha >= 0);
        Assert.False(payload.Alertas.SemAcaoAdminRecenteSobRisco);
        Assert.NotNull(payload.Alertas.UltimaAcaoAdminEm);
        Assert.Contains(payload.RiscoOperacional, ["baixo", "medio", "alto"]);
        Assert.NotNull(payload.ItensCriticosRecentes);
        Assert.True(payload.ItensCriticosRecentes.AvaliacoesPendentes.Count >= 1);
        Assert.True(payload.ItensCriticosRecentes.WebhooksFalhos.Count >= 0);
        Assert.True(payload.ItensCriticosRecentes.EmailsFalhos.Count >= 0);
        Assert.NotNull(payload.AcoesRecomendadas);
        Assert.True(payload.AcoesRecomendadas.Itens.Count >= 1);
        Assert.NotNull(payload.TopProfissionaisEmAtencao);
        Assert.True(payload.TopProfissionaisEmAtencao.Count >= 1);
        Assert.True(payload.TopProfissionaisEmAtencao[0].ScoreAtencao >= 1);
        Assert.NotNull(payload.TopClientesEmAtencao);
        Assert.True(payload.TopClientesEmAtencao.Count >= 1);
        Assert.True(payload.TopClientesEmAtencao[0].ScoreAtencao >= 1);
        Assert.NotNull(payload.TopUsuariosInativosRecentes);
        Assert.True(payload.TopUsuariosInativosRecentes.Count >= 1);
        Assert.Contains(payload.TopUsuariosInativosRecentes, x => x.UsuarioId == profissional.Auth.UsuarioId);
        Assert.NotNull(payload.AcoesAdminRecentes);
        Assert.True(payload.AcoesAdminRecentes.Count >= 1);
        Assert.NotNull(payload.TopAdminsAtivos);
        Assert.True(payload.TopAdminsAtivos.Count >= 1);
        Assert.Equal(admin.UsuarioId, payload.TopAdminsAtivos[0].AdminUsuarioId);
        Assert.NotNull(payload.SlaOperacional);
        Assert.NotNull(payload.SlaOperacional.UltimaAcaoAdminEm);
        Assert.True(payload.SlaOperacional.MinutosDesdeUltimaAcaoAdmin >= 0);
        Assert.NotNull(payload.DisponibilidadeOperacional);
        Assert.InRange(payload.DisponibilidadeOperacional.PercentualSucessoWebhooks, 0m, 100m);
        Assert.InRange(payload.DisponibilidadeOperacional.PercentualFalhaWebhooks, 0m, 100m);
        Assert.InRange(payload.DisponibilidadeOperacional.PercentualSucessoEmails, 0m, 100m);
        Assert.InRange(payload.DisponibilidadeOperacional.PercentualFalhaEmails, 0m, 100m);
        Assert.NotNull(payload.SaudeOperacional);
        Assert.Contains(payload.SaudeOperacional.Status, ["saudavel", "atencao", "critico"]);
        Assert.Contains(payload.SaudeOperacional.IndicadorCor, ["verde", "amarelo", "vermelho"]);
        Assert.Contains(payload.SaudeOperacional.PrioridadeVisual, ["baixa", "media", "alta"]);
        Assert.InRange(payload.SaudeOperacional.OrdemAtencao, 1, 3);
        Assert.False(string.IsNullOrWhiteSpace(payload.SaudeOperacional.AcaoPrimariaSugerida));
        Assert.False(string.IsNullOrWhiteSpace(payload.SaudeOperacional.Resumo));
        Assert.NotNull(payload.ResumoDecisorio);
        Assert.Contains(payload.ResumoDecisorio.SituacaoGeral, ["baixo", "medio", "alto"]);
        Assert.False(string.IsNullOrWhiteSpace(payload.ResumoDecisorio.FocoPrincipal));
        Assert.False(string.IsNullOrWhiteSpace(payload.ResumoDecisorio.PrincipalGargalo));
        Assert.False(string.IsNullOrWhiteSpace(payload.ResumoDecisorio.RecomendacaoImediata));
    }

    [Fact]
    public async Task Obter_DeveSinalizarRiscoAltoSemAcaoAdminRecente()
    {
        using var adminClient = _factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var adminUsuario = await context.Usuarios.FirstAsync(x => x.Id == admin.UsuarioId);
            var dataAntiga = DateTime.UtcNow.AddDays(-2);

            context.AuditoriasAdminAcoes.Add(new AuditoriaAdminAcao
            {
                AdminUsuarioId = adminUsuario.Id,
                Entidade = "usuario",
                EntidadeId = adminUsuario.Id,
                Acao = "bloquear",
                Descricao = "Acao antiga para teste",
                PayloadJson = "{}",
                DataCriacao = dataAntiga
            });

            context.WebhookPagamentoImpulsionamentoEventos.Add(new WebhookPagamentoImpulsionamentoEvento
            {
                Provedor = "manual",
                EventoExternoId = $"evt-risco-{Guid.NewGuid():N}",
                CodigoReferenciaPagamento = $"ref-risco-{Guid.NewGuid():N}",
                StatusPagamento = "pago",
                PayloadJson = "{}",
                HeadersJson = "{}",
                IpOrigem = "127.0.0.1",
                RequestId = "req-risco-antigo",
                UserAgent = "integration-test",
                ProcessadoComSucesso = false,
                MensagemResultado = "Falha simulada",
                DataCriacao = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await adminClient.GetAsync("/api/admin/dashboard");
        var payload = await response.Content.ReadFromJsonAsync<AdminDashboardResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("alto", payload!.RiscoOperacional);
        Assert.True(payload.Alertas.SemAcaoAdminRecenteSobRisco);
        Assert.NotNull(payload.Alertas.UltimaAcaoAdminEm);
        Assert.NotNull(payload.SlaOperacional);
        Assert.NotNull(payload.SlaOperacional.UltimaAcaoAdminEm);
        Assert.True(payload.SlaOperacional.MinutosDesdeUltimaAcaoAdmin >= 60 * 24);
        Assert.NotNull(payload.SlaOperacional.UltimoWebhookFalhoEm);
        Assert.True(payload.SlaOperacional.MinutosDesdeUltimoWebhookFalho >= 0);
        Assert.NotNull(payload.DisponibilidadeOperacional);
        Assert.True(payload.DisponibilidadeOperacional.PercentualFalhaWebhooks > 0m);
        Assert.NotNull(payload.SaudeOperacional);
        Assert.Equal("critico", payload.SaudeOperacional.Status);
        Assert.Equal("vermelho", payload.SaudeOperacional.IndicadorCor);
        Assert.Equal("alta", payload.SaudeOperacional.PrioridadeVisual);
        Assert.Equal(1, payload.SaudeOperacional.OrdemAtencao);
        Assert.Contains("Acionar administracao", payload.SaudeOperacional.AcaoPrimariaSugerida, StringComparison.Ordinal);
        Assert.Contains(payload.AcoesRecomendadas.Itens, x => x.Contains("Acionar administracao", StringComparison.Ordinal));
        Assert.Contains("Sem acao administrativa recente", payload.ResumoDecisorio.PrincipalGargalo);
    }

    private static HttpRequestMessage CriarWebhookRequest(string codigoReferenciaPagamento, string eventoExternoId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/pagamentos/impulsionamentos")
        {
            Content = JsonContent.Create(new WebhookPagamentoImpulsionamentoRequest
            {
                CodigoReferenciaPagamento = codigoReferenciaPagamento,
                StatusPagamento = "pago",
                EventoExternoId = eventoExternoId
            })
        };
        request.Headers.UserAgent.ParseAdd("integration-test-agent/1.0");

        var payload = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SegredoWebhook));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        request.Headers.Add(HeaderAssinatura, Convert.ToHexString(hash).ToLowerInvariant());

        return request;
    }

    private async Task<Guid> ObterPlanoIdAsync()
    {
        using var client = _factory.CreateClient();
        var planos = await client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");
        return planos![0].Id;
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

    private async Task<ProfissionalRegistrado> RegistrarProfissionalAsync(HttpClient client, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11999999999",
            Senha = senha,
            TipoPerfil = TipoPerfil.Profissional
        });

        response.EnsureSuccessStatusCode();

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(auth.UsuarioId);
        return new ProfissionalRegistrado(auth, profissionalId);
    }

    private static async Task<AuthResponse> RegistrarClienteAsync(HttpClient client, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11988887777",
            Senha = "Senha@123",
            TipoPerfil = TipoPerfil.Cliente
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private sealed record ProfissionalRegistrado(AuthResponse Auth, Guid ProfissionalId);
}
