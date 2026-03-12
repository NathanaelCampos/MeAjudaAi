using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.IntegrationTests.Infrastructure;

namespace MeAjudaAi.IntegrationTests.Impulsionamentos;

public class ImpulsionamentosEndpointsTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ImpulsionamentosEndpointsTests(TestWebApplicationFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ContratarEListarMeus_DeveFuncionarParaProfissionalAutenticado()
    {
        var auth = await RegistrarProfissionalAsync();
        var admin = await LoginAdminAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await _client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");

        Assert.NotNull(planos);
        var plano = Assert.Single(planos!.Where(x => x.QuantidadePeriodo == 1));

        var contratarResponse = await _client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = plano.Id,
            CodigoReferenciaPagamento = "pag-teste-001"
        });

        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        Assert.NotNull(contratado);
        Assert.Equal(plano.Id, contratado!.PlanoImpulsionamentoId);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, contratado.Status);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", admin.Token);

        var confirmarResponse = await _client.PutAsync($"/api/impulsionamentos/{contratado.Id}/confirmar-pagamento", null);
        var confirmado = await confirmarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);
        Assert.NotNull(confirmado);
        Assert.Equal(StatusImpulsionamento.Ativo, confirmado!.Status);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var meusResponse = await _client.GetAsync("/api/impulsionamentos/meus");
        var meus = await meusResponse.Content.ReadFromJsonAsync<List<ImpulsionamentoProfissionalResponse>>();

        Assert.Equal(HttpStatusCode.OK, meusResponse.StatusCode);
        Assert.NotNull(meus);
        Assert.Contains(meus!, x => x.Id == contratado.Id && x.Status == StatusImpulsionamento.Ativo);
    }

    [Fact]
    public async Task ContratarDuasVezes_DeveEnfileirarSegundoImpulsionamentoComoPendente()
    {
        var auth = await RegistrarProfissionalAsync();
        var admin = await LoginAdminAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await _client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");

        Assert.NotNull(planos);
        var plano = Assert.Single(planos!.Where(x => x.QuantidadePeriodo == 1));

        var primeiroResponse = await _client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = plano.Id,
            CodigoReferenciaPagamento = "pag-fila-001"
        });

        var primeiro = await primeiroResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, primeiroResponse.StatusCode);
        Assert.NotNull(primeiro);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, primeiro!.Status);

        var segundoResponse = await _client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = plano.Id,
            CodigoReferenciaPagamento = "pag-fila-002"
        });

        var segundo = await segundoResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, segundoResponse.StatusCode);
        Assert.NotNull(segundo);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, segundo!.Status);
        Assert.Equal(primeiro.DataFim, segundo.DataInicio);
        Assert.True(segundo.DataFim > segundo.DataInicio);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", admin.Token);

        var confirmarPrimeiroResponse = await _client.PutAsync($"/api/impulsionamentos/{primeiro.Id}/confirmar-pagamento", null);
        var primeiroConfirmado = await confirmarPrimeiroResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, confirmarPrimeiroResponse.StatusCode);
        Assert.NotNull(primeiroConfirmado);
        Assert.Equal(StatusImpulsionamento.Ativo, primeiroConfirmado!.Status);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);
        var meus = await _client.GetFromJsonAsync<List<ImpulsionamentoProfissionalResponse>>("/api/impulsionamentos/meus");

        Assert.NotNull(meus);
        Assert.Contains(meus!, x => x.Id == primeiro.Id && x.Status == StatusImpulsionamento.Ativo);
        Assert.Contains(meus!, x => x.Id == segundo.Id && x.Status == StatusImpulsionamento.PendentePagamento);
    }

    [Fact]
    public async Task ConfirmarPagamentoPorCodigoReferencia_DeveAtivarImpulsionamentoPendente()
    {
        var auth = await RegistrarProfissionalAsync();
        var admin = await LoginAdminAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var planos = await _client.GetFromJsonAsync<List<PlanoImpulsionamentoResponse>>("/api/impulsionamentos/planos");

        Assert.NotNull(planos);
        var plano = Assert.Single(planos!.Where(x => x.QuantidadePeriodo == 1));

        var contratarResponse = await _client.PostAsJsonAsync("/api/impulsionamentos/contratar", new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = plano.Id,
            CodigoReferenciaPagamento = "pag-ref-endpoint-001"
        });

        var contratado = await contratarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, contratarResponse.StatusCode);
        Assert.NotNull(contratado);
        Assert.Equal(StatusImpulsionamento.PendentePagamento, contratado!.Status);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", admin.Token);

        var confirmarResponse = await _client.PostAsJsonAsync("/api/impulsionamentos/confirmar-pagamento", new ConfirmarPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = "pag-ref-endpoint-001"
        });

        var confirmado = await confirmarResponse.Content.ReadFromJsonAsync<ImpulsionamentoProfissionalResponse>();

        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);
        Assert.NotNull(confirmado);
        Assert.Equal(contratado.Id, confirmado!.Id);
        Assert.Equal(StatusImpulsionamento.Ativo, confirmado.Status);
    }

    private async Task<AuthResponse> RegistrarProfissionalAsync()
    {
        var email = $"profissional-{Guid.NewGuid():N}@teste.local";

        var response = await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = "Profissional Teste",
            Email = email,
            Telefone = "11999999999",
            Senha = "Senha@123",
            TipoPerfil = TipoPerfil.Profissional
        });

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        return auth!;
    }

    private async Task<AuthResponse> LoginAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
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
