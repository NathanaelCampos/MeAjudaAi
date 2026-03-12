using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Interfaces.Impulsionamentos;

public interface IImpulsionamentoService
{
    Task<IReadOnlyList<PlanoImpulsionamentoResponse>> ListarPlanosAsync(
        CancellationToken cancellationToken = default);

    Task<ImpulsionamentoProfissionalResponse> ContratarPlanoAsync(
        Guid usuarioId,
        ContratarPlanoImpulsionamentoRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpulsionamentoProfissionalResponse>> ListarMeusImpulsionamentosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}