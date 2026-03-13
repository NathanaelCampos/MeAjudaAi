using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Servicos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using MeAjudaAi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminServicosEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminServicosEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarServicos_DeveAplicarFiltrosEPaginacao()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-servicos-lista");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-servicos-lista");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();

        var criarResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissional.ProfissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico admin lista",
            Descricao = "Servico admin lista descricao",
            ValorCombinado = 100m
        });

        Assert.Equal(HttpStatusCode.OK, criarResponse.StatusCode);

        var response = await adminClient.GetAsync($"/api/admin/servicos?termo=admin lista&profissionalId={profissional.ProfissionalId}&status={StatusServico.Solicitado}&pagina=1&tamanhoPagina=10");
        var payload = await response.Content.ReadFromJsonAsync<PaginacaoResponse<ServicoAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.TotalRegistros >= 1);
        Assert.NotEmpty(payload.Itens);
        Assert.Contains(payload.Itens, x => x.ProfissionalId == profissional.ProfissionalId && x.Status == StatusServico.Solicitado);
    }

    [Fact]
    public async Task ObterServicoPorId_DeveRetornarDetalhe()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarClienteAsync(clienteClient, "cliente-admin-servicos-detalhe");
        var profissional = await RegistrarProfissionalAsync(profissionalClient, "prof-admin-servicos-detalhe");
        var admin = await LoginAdminAsync(adminClient);

        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cliente.Auth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var cidadeId = await _factory.ObterCidadeIdAsync();

        var criarResponse = await clienteClient.PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            ProfissionalId = profissional.ProfissionalId,
            CidadeId = cidadeId,
            Titulo = "Servico admin detalhe",
            Descricao = "Servico admin detalhe descricao",
            ValorCombinado = 120m
        });

        Assert.Equal(HttpStatusCode.OK, criarResponse.StatusCode);
        var servico = await criarResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        var response = await adminClient.GetAsync($"/api/admin/servicos/{servico!.Id}");
        var payload = await response.Content.ReadFromJsonAsync<ServicoAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(servico.Id, payload!.Id);
        Assert.Equal(cliente.Auth.UsuarioId, await ObterUsuarioIdClientePorServicoAsync(servico.Id));
        Assert.Equal(profissional.ProfissionalId, payload.ProfissionalId);
        Assert.Equal("Servico admin detalhe", payload.Titulo);
    }

    [Fact]
    public async Task ObterServicoPorId_Inexistente_DeveRetornar404()
    {
        using var adminClient = _factory.CreateClient();
        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/servicos/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Serviço não encontrado.", payload!.Mensagem);
    }

    private async Task<Guid> ObterUsuarioIdClientePorServicoAsync(Guid servicoId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Servicos
            .Where(x => x.Id == servicoId)
            .Select(x => x.Cliente.UsuarioId)
            .FirstAsync();
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
}
