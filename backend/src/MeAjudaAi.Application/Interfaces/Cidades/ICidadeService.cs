using MeAjudaAi.Application.DTOs.Cidades;

namespace MeAjudaAi.Application.Interfaces.Cidades;

public interface ICidadeService
{
    Task<IReadOnlyList<CidadeResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BairroResponse>> ListarBairrosPorCidadeAsync(
        Guid cidadeId,
        CancellationToken cancellationToken = default);
}