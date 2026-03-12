using MeAjudaAi.Application.DTOs.Avaliacoes;

namespace MeAjudaAi.Application.Interfaces.Avaliacoes;

public interface IAvaliacaoService
{
    Task<AvaliacaoResponse> CriarAsync(
        Guid usuarioId,
        CriarAvaliacaoRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvaliacaoResponse>> ListarPorProfissionalAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvaliacaoResponse>> ListarPendentesAsync(
        CancellationToken cancellationToken = default);

    Task<AvaliacaoResponse?> ModerarAsync(
        Guid avaliacaoId,
        ModerarAvaliacaoRequest request,
        CancellationToken cancellationToken = default);
}