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
        Assert.Equal(StatusImpulsionamento.Ativo, contratado.Status);

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
        Assert.Equal(StatusImpulsionamento.Ativo, primeiro!.Status);

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

        var meus = await _client.GetFromJsonAsync<List<ImpulsionamentoProfissionalResponse>>("/api/impulsionamentos/meus");

        Assert.NotNull(meus);
        Assert.Contains(meus!, x => x.Id == primeiro.Id && x.Status == StatusImpulsionamento.Ativo);
        Assert.Contains(meus!, x => x.Id == segundo.Id && x.Status == StatusImpulsionamento.PendentePagamento);
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
}
