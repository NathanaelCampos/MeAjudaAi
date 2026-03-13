using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Avaliacoes;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminAvaliacoesEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminAvaliacoesEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarAvaliacoes_DeveAplicarFiltrosEPaginacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-avaliacoes-lista");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-avaliacoes-lista");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var resultado = await CriarServicoEAvaliacaoAsync(clienteClient, profissionalClient, profissional.ProfissionalId, "avaliacao admin lista");
        var avaliacao = resultado.Avaliacao;

        var response = await adminClient.GetAsync($"/api/admin/avaliacoes?termo=avaliacao admin lista&profissionalId={profissional.ProfissionalId}&statusModeracaoComentario={StatusModeracaoComentario.Pendente}&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<AvaliacaoAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.Contains(payload.Itens, x => x.Id == avaliacao.Id && x.ProfissionalId == profissional.ProfissionalId);
    }

    [Fact]
    public async Task ObterAvaliacaoPorId_DeveRetornarDetalhe()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-avaliacoes-detalhe");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-avaliacoes-detalhe");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var resultado = await CriarServicoEAvaliacaoAsync(clienteClient, profissionalClient, profissional.ProfissionalId, "avaliacao admin detalhe");
        var avaliacao = resultado.Avaliacao;

        var response = await adminClient.GetAsync($"/api/admin/avaliacoes/{avaliacao.Id}");
        var payload = await response.Content.ReadFromJsonAsync<AvaliacaoAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(avaliacao.Id, payload!.Id);
        Assert.Equal("avaliacao admin detalhe", payload.Comentario);
        Assert.Equal(StatusModeracaoComentario.Pendente, payload.StatusModeracaoComentario);
    }

    [Fact]
    public async Task ObterAvaliacaoPorId_Inexistente_DeveRetornar404()
    {
        using var adminClient = _factory.CreateClient();
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/avaliacoes/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Avaliação não encontrada.", payload!.Mensagem);
    }

    [Fact]
    public async Task ObterDashboard_DeveRetornarConsolidadoDaAvaliacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-avaliacoes-dashboard");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-avaliacoes-dashboard");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        profissionalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profissional.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var resultado = await CriarServicoEAvaliacaoAsync(clienteClient, profissionalClient, profissional.ProfissionalId, "avaliacao admin dashboard");
        var avaliacao = resultado.Avaliacao;
        var servico = resultado.Servico;

        var moderarResponse = await adminClient.PutAsJsonAsync($"/api/avaliacoes/{avaliacao.Id}/moderar", new ModerarAvaliacaoRequest
        {
            Acao = AcaoModeracaoAvaliacao.Aprovar
        });
        moderarResponse.EnsureSuccessStatusCode();

        var response = await adminClient.GetAsync($"/api/admin/avaliacoes/{avaliacao.Id}/dashboard");
        var payload = await response.Content.ReadFromJsonAsync<AvaliacaoAdminDashboardResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(avaliacao.Id, payload!.Avaliacao.Id);
        Assert.Equal(StatusModeracaoComentario.Aprovado, payload.Avaliacao.StatusModeracaoComentario);
        Assert.Equal(servico.Id, payload.Servico.Id);
        Assert.Equal("Servico avaliacao admin dashboard", payload.Servico.Titulo);
        Assert.True(payload.Notificacoes.Total >= 1);
    }

    private async Task<ServicoEAvaliacaoCriados> CriarServicoEAvaliacaoAsync(
        HttpClient clienteClient,
        HttpClient profissionalClient,
        Guid profissionalId,
        string comentario)
    {
        var cidadeId = await _factory.ObterCidadeIdAsync();

        var criarServicoResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissionalId,
            CidadeId = cidadeId,
            Titulo = $"Servico {comentario}",
            Descricao = $"Descricao {comentario}",
            ValorCombinado = 120m
        });

        criarServicoResponse.EnsureSuccessStatusCode();
        var servico = (await criarServicoResponse.Content.ReadFromJsonAsync<ServicoResponse>())!;

        (await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/aceitar", null)).EnsureSuccessStatusCode();
        (await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/iniciar", null)).EnsureSuccessStatusCode();
        (await profissionalClient.PutAsync($"/api/servicos/{servico.Id}/concluir", null)).EnsureSuccessStatusCode();

        var criarAvaliacaoResponse = await clienteClient.PostAsJsonAsync("/api/avaliacoes", new CriarAvaliacaoRequest
        {
            ServicoId = servico.Id,
            NotaAtendimento = NotaAtendimento.Excelente,
            NotaServico = NotaServico.Excelente,
            NotaPreco = NotaPreco.BomCustoBeneficio,
            Comentario = comentario
        });

        criarAvaliacaoResponse.EnsureSuccessStatusCode();
        var avaliacao = (await criarAvaliacaoResponse.Content.ReadFromJsonAsync<AvaliacaoResponse>())!;
        return new ServicoEAvaliacaoCriados(servico, avaliacao);
    }

    private static async Task<AuthResponse> LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestWebApplicationFactory.EmailAdmin,
            Senha = TestWebApplicationFactory.SenhaAdmin
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private static async Task<ClienteRegistrado> RegistrarClienteAsync(HttpClient client, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11988887777",
            Senha = senha,
            TipoPerfil = TipoPerfil.Cliente
        });

        response.EnsureSuccessStatusCode();
        return new ClienteRegistrado((await response.Content.ReadFromJsonAsync<AuthResponse>())!, email, senha);
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
        return new ProfissionalRegistrado(auth, profissionalId, email, senha);
    }

    private sealed record ClienteRegistrado(AuthResponse Auth, string Email, string Senha);
    private sealed record ProfissionalRegistrado(AuthResponse Auth, Guid ProfissionalId, string Email, string Senha);
    private sealed record ServicoEAvaliacaoCriados(ServicoResponse Servico, AvaliacaoResponse Avaliacao);
}
