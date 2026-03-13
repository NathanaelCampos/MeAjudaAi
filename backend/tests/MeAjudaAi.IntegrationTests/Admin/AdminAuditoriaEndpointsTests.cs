using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeAjudaAi.Application.DTOs.Admin;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Admin;

public class AdminAuditoriaEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminAuditoriaEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BuscarEObterAuditoria_DeveRetornarAcoesAdministrativas()
    {
        using var clienteClient = _factory.CreateClient();
        using var profissionalClient = _factory.CreateClient();
        using var adminClient = _factory.CreateClient();

        var cliente = await RegistrarUsuarioAsync(clienteClient, TipoPerfil.Cliente, "auditoria-admin-cliente");
        var profissional = await RegistrarUsuarioAsync(profissionalClient, TipoPerfil.Profissional, "auditoria-admin-prof");
        var profissionalId = await _factory.ObterProfissionalIdPorUsuarioIdAsync(profissional.UsuarioId);
        var admin = await LoginAdminAsync(adminClient);

        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var bloquearResponse = await adminClient.PutAsync($"/api/admin/usuarios/{cliente.UsuarioId}/bloquear", null);
        var verificarResponse = await adminClient.PutAsync($"/api/admin/profissionais/{profissionalId}/verificar", null);

        Assert.Equal(HttpStatusCode.OK, bloquearResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, verificarResponse.StatusCode);

        var buscaResponse = await adminClient.GetAsync($"/api/admin/auditoria?adminUsuarioId={admin.UsuarioId}&pagina=1&tamanhoPagina=20");
        var buscaPayload = await buscaResponse.Content.ReadFromJsonAsync<PaginacaoResponse<AuditoriaAdminListItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, buscaResponse.StatusCode);
        Assert.NotNull(buscaPayload);
        Assert.Contains(buscaPayload!.Itens, x => x.Entidade == "usuario" && x.EntidadeId == cliente.UsuarioId && x.Acao == "bloquear");
        Assert.Contains(buscaPayload.Itens, x => x.Entidade == "profissional" && x.EntidadeId == profissionalId && x.Acao == "verificar");

        var auditoriaId = buscaPayload.Itens.First(x => x.Entidade == "usuario" && x.EntidadeId == cliente.UsuarioId && x.Acao == "bloquear").Id;
        var detalheResponse = await adminClient.GetAsync($"/api/admin/auditoria/{auditoriaId}");
        var detalhePayload = await detalheResponse.Content.ReadFromJsonAsync<AuditoriaAdminDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, detalheResponse.StatusCode);
        Assert.NotNull(detalhePayload);
        Assert.Equal("usuario", detalhePayload!.Entidade);
        Assert.Equal("bloquear", detalhePayload.Acao);
        Assert.Contains("\"ativo\":false", detalhePayload.PayloadJson);
    }

    [Fact]
    public async Task ObterAuditoriaInexistente_DeveRetornar404()
    {
        using var adminClient = _factory.CreateClient();

        var admin = await LoginAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);

        var response = await adminClient.GetAsync($"/api/admin/auditoria/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<MeAjudaAi.Application.DTOs.Common.MensagemErroResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Auditoria não encontrada.", payload!.Mensagem);
    }

    private static async Task<(Guid UsuarioId, string Token)> RegistrarUsuarioAsync(HttpClient client, TipoPerfil tipoPerfil, string prefixo)
    {
        var email = $"{prefixo}-{Guid.NewGuid():N}@teste.local";
        const string senha = "Senha@123";

        var response = await client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = $"{prefixo} teste",
            Email = email,
            Telefone = "11999999999",
            Senha = senha,
            TipoPerfil = tipoPerfil
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return (auth!.UsuarioId, auth.Token);
    }

    private static async Task<AuthResponse> LoginAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestWebApplicationFactory.EmailAdmin,
            Senha = TestWebApplicationFactory.SenhaAdmin
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return auth!;
    }
}
